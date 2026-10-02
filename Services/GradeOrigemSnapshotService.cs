using API_HorarioProfessor.Dto;
using Dapper;
using MySqlConnector;

namespace API_HorarioProfessor.Services;

public sealed class GradeOrigemSnapshotService : IGradeOrigemSnapshotService
{
    private readonly string _connectionString;

    public GradeOrigemSnapshotService(string connectionString) =>
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<GradeOrigemSnapshotDto> ObterAsync(
        int idEscola,
        int anoLetivo,
        string periodo,
        int quantidadeHorarios)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var parametros = new { IdEscola = idEscola, AnoLetivo = anoLetivo, Periodo = periodo };
        var professores = (await connection.QueryAsync<ProfessorOrigem>("""
            SELECT ProfessorId, NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula
              FROM hprof_cadastroprofessor
             WHERE IdEscola = @IdEscola AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo
             ORDER BY NomeProfessor, ProfessorId
            """, parametros)).ToList();

        var turmas = (await connection.QueryAsync<TurmaOrigem>("""
            SELECT TurmaId, Turmas, Polivalente
              FROM hprof_turmapadraoescola
             WHERE IdEscola = @IdEscola AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo
             ORDER BY Turmas
            """, parametros)).ToList();

        var disciplinas = (await connection.QueryAsync<DisciplinaOrigem>("""
            SELECT Id, Descricao, Sigla, TodosOsAnos, QuantidadeAulas
              FROM hprof_materiaaulasporano
             WHERE EscolaId = @IdEscola AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo
             ORDER BY Sigla, Id
            """, parametros)).ToList();

        var regras = (await connection.QueryAsync<RegraOrigem>("""
            SELECT o.Id, o.IdProfessor_fk AS IdProfessor, p.NomeProfessor,
                   o.Linha, o.Coluna, o.Opcao, o.OpcaoDaSemana, o.Tipo, o.Sigla, o.Ativo
              FROM hprof_opcaohorarioprocessamento o
              LEFT JOIN hprof_cadastroprofessor p ON p.ProfessorId = o.IdProfessor_fk
             WHERE o.IdEscola = @IdEscola AND o.AnoLetivo = @AnoLetivo
               AND o.Periodo = @Periodo AND o.Ativo = 1
             ORDER BY o.IdProfessor_fk, o.Opcao, o.Linha, o.Coluna
            """, parametros)).ToList();

        var origemRegra = "PROCESSAMENTO";
        if (regras.Count == 0)
        {
            regras = (await connection.QueryAsync<RegraOrigem>("""
                SELECT o.Id, o.IdProfessor_fk AS IdProfessor, p.NomeProfessor,
                       o.Linha, o.Coluna, o.Opcao, o.OpcaoDaSemana, o.Tipo, o.Sigla, o.Ativo
                  FROM hprof_opcaohorario o
                  LEFT JOIN hprof_cadastroprofessor p ON p.ProfessorId = o.IdProfessor_fk
                 WHERE o.IdEscola = @IdEscola AND o.AnoLetivo = @AnoLetivo
                   AND o.Periodo = @Periodo AND o.Ativo = 1 AND o.Sigla <> 'REMOVE'
                 ORDER BY o.IdProfessor_fk, o.Opcao, o.Linha, o.Coluna
                """, parametros)).ToList();
            origemRegra = "CADASTRO";
        }

        var nomesTurmas = turmas.Select(t => t.Turmas?.Trim() ?? "")
            .Where(t => t.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return new GradeOrigemSnapshotDto
        {
            Contexto = new ContextoGradeSnapshotDto
            {
                IdEscola = idEscola,
                AnoLetivo = anoLetivo,
                Periodo = periodo,
                QuantidadeHorarios = quantidadeHorarios,
                GeradoEm = DateTime.UtcNow
            },
            Turmas = nomesTurmas,
            TurmasDetalhes = turmas.Select(t => new TurmaGradeSnapshotDto
            {
                IdTurma = t.TurmaId,
                Nome = t.Turmas?.Trim() ?? "",
                Polivalente = string.Equals(t.Polivalente, "SIM", StringComparison.OrdinalIgnoreCase)
            }).ToList(),
            Professores = professores.GroupBy(p => new { p.ProfessorId, p.NomeProfessor }).Select(g =>
                new ProfessorGradeSnapshotDto
                {
                    IdProfessor = g.Key.ProfessorId,
                    Nome = g.Key.NomeProfessor?.Trim() ?? "",
                    CargaTotal = g.Sum(p => p.QuantidadeAula),
                    Atribuicoes = g.Select(p => new AtribuicaoProfessorSnapshotDto
                    {
                        Turma = p.TurmasAtribuidas?.Trim() ?? "",
                        Disciplina = p.Materia?.Trim() ?? "",
                        Sigla = p.Materia?.Trim() ?? "",
                        QuantidadeAulas = p.QuantidadeAula
                    }).ToList()
                }).ToList(),
            Disciplinas = disciplinas.Select(d => new DisciplinaGradeSnapshotDto
            {
                IdOrigem = d.Id,
                Descricao = d.Descricao?.Trim() ?? "",
                Sigla = d.Sigla?.Trim() ?? "",
                AnoEscolar = 0,
                TodosOsAnos = d.TodosOsAnos != 0,
                QuantidadeAulas = d.QuantidadeAulas
            }).ToList(),
            Regras = regras.Select(r => MapearRegra(r, origemRegra, nomesTurmas)).ToList(),
            Matrizes = CriarMatrizesVazias(quantidadeHorarios, nomesTurmas.Count)
        };
    }

    private static RegraGradeSnapshotDto MapearRegra(
        RegraOrigem regra,
        string origem,
        IReadOnlyList<string> turmas)
    {
        var diaIndex = MapearDia(regra.OpcaoDaSemana);
        var colunaIndex = regra.Coluna > 0 ? regra.Coluna - 1 : regra.Coluna;
        return new RegraGradeSnapshotDto
        {
            IdOrigem = regra.Id,
            Categoria = regra.Opcao == 1 ? "BLOQUEIO" : "PREFERENCIA",
            IdProfessor = regra.IdProfessor,
            NomeProfessor = regra.NomeProfessor,
            DiaIndex = diaIndex,
            Slot = regra.Linha > 0 ? regra.Linha - 1 : regra.Linha,
            Turma = colunaIndex >= 0 && colunaIndex < turmas.Count ? turmas[colunaIndex] : null,
            DiaOrigem = regra.OpcaoDaSemana,
            LinhaOrigem = regra.Linha,
            ColunaOrigem = regra.Coluna,
            Tipo = regra.Tipo,
            Sigla = regra.Sigla,
            Prioridade = regra.Opcao,
            Obrigatoria = regra.Opcao == 1,
            Ativa = regra.Ativo == 1,
            Origem = origem,
            Descricao = $"Opção {regra.Opcao}: {regra.Tipo} {regra.Sigla}".Trim()
        };
    }

    private static int? MapearDia(string? dia)
    {
        var normalizado = dia?.Trim().ToUpperInvariant() ?? "";
        if (normalizado.StartsWith("SEG", StringComparison.Ordinal)) return 0;
        if (normalizado.StartsWith("TER", StringComparison.Ordinal)) return 1;
        if (normalizado.StartsWith("QUA", StringComparison.Ordinal)) return 2;
        if (normalizado.StartsWith("QUI", StringComparison.Ordinal)) return 3;
        if (normalizado.StartsWith("SEX", StringComparison.Ordinal)) return 4;
        return null;
    }

    private static GradeMatrizesSnapshotDto CriarMatrizesVazias(int linhas, int colunas)
    {
        List<List<string>> Criar() => Enumerable.Range(0, linhas)
            .Select(_ => Enumerable.Repeat("", colunas).ToList()).ToList();
        return new GradeMatrizesSnapshotDto
        {
            Segunda = Criar(), Terca = Criar(), Quarta = Criar(), Quinta = Criar(), Sexta = Criar()
        };
    }

    private sealed class ProfessorOrigem
    {
        public int ProfessorId { get; init; }
        public string? NomeProfessor { get; init; }
        public string? TurmasAtribuidas { get; init; }
        public string? Materia { get; init; }
        public int QuantidadeAula { get; init; }
    }

    private sealed class TurmaOrigem
    {
        public int TurmaId { get; init; }
        public string? Turmas { get; init; }
        public string? Polivalente { get; init; }
    }

    private sealed class DisciplinaOrigem
    {
        public int Id { get; init; }
        public string? Descricao { get; init; }
        public string? Sigla { get; init; }
        public int TodosOsAnos { get; init; }
        public int QuantidadeAulas { get; init; }
    }

    private sealed class RegraOrigem
    {
        public int Id { get; init; }
        public int IdProfessor { get; init; }
        public string? NomeProfessor { get; init; }
        public int Linha { get; init; }
        public int Coluna { get; init; }
        public int Opcao { get; init; }
        public string? OpcaoDaSemana { get; init; }
        public string? Tipo { get; init; }
        public string? Sigla { get; init; }
        public int Ativo { get; init; }
    }
}
