using System.Text.Json;
using API_HorarioProfessor.Universidade.Contratos;
using Dapper;
using MySqlConnector;

namespace API_HorarioProfessor.Universidade.Servicos;

public interface IUniversidadeConsultaService
{
    Task<ContextoUniversidadeDto?> ObterContextoAsync(int idEscola);
    Task<List<CursoDto>> ListarCursosAsync(long idInstituicao);
    Task<List<PainelLinhaDto>> ObterPainelAsync(long idInstituicao, int anoLetivo, string periodo);
    Task<List<PessoaDto>> ListarPessoasAsync(long idInstituicao);
    Task<PessoaDetalheDto?> ObterPessoaAsync(long idInstituicao, long idPessoa, int anoLetivo, string periodo);
    Task<List<AtribuicaoDto>> ListarAtribuicoesCursoAsync(int idEscola, int anoLetivo, string periodo);
    Task<List<TurmaUniversidadeDto>> ListarTurmasCursoAsync(int idEscola);
    Task<List<PlanilhaSalvaDto>> ListarGradesSalvasAsync(int idEscola, int anoLetivo, string periodo);
    Task<GradeCursoDto> ObterGradeAsync(int idEscola, int anoLetivo, string periodo, int? idPlanilha);
    Task<SemanaProfessorDto?> ObterSemanaAsync(long idInstituicao, long idPessoa, int anoLetivo);
    Task<ConferenciaDto> ObterConferenciaAsync(long idInstituicao, int anoLetivo, string periodo);
}

/// <summary>Leituras da área Universidade. Só lê o banco.</summary>
public sealed class UniversidadeConsultaService : IUniversidadeConsultaService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly UniversidadeConexao _conexao;

    public UniversidadeConsultaService(UniversidadeConexao conexao) => _conexao = conexao;

    private const string SelectCurso = """
        SELECT c.id AS Id, c.id_instituicao AS IdInstituicao, c.id_escola AS IdEscola, c.codigo AS Codigo, c.nome AS Nome,
               c.turno AS Turno, c.dias AS Dias, CAST(c.intervalos_json AS CHAR) AS IntervalosJson, c.ordem AS Ordem,
               COALESCE(e.QuantidadeHorarios, 0) AS QuantidadeHorarios, COALESCE(e.Nome, '') AS NomeEscola
          FROM hprof_uni_curso c
          LEFT JOIN hprof_escolas e ON e.EscolaId = c.id_escola
        """;

    public async Task<ContextoUniversidadeDto?> ObterContextoAsync(int idEscola)
    {
        await using var cn = await _conexao.AbrirAsync();
        var inst = await cn.QueryFirstOrDefaultAsync<InstituicaoDto>(
            """
            SELECT i.id AS Id, i.sigla AS Sigla, i.nome AS Nome, i.id_escola_principal AS IdEscolaPrincipal
              FROM hprof_uni_instituicao i
             WHERE i.ativo = 1 AND (i.id_escola_principal = @e
                   OR i.id = (SELECT c.id_instituicao FROM hprof_uni_curso c WHERE c.id_escola = @e AND c.ativo = 1 LIMIT 1))
             LIMIT 1
            """, new { e = idEscola });
        if (inst is null) return null;
        var cursos = await CursosAsync(cn, inst.Id);
        return new ContextoUniversidadeDto
        {
            Instituicao = inst, Cursos = cursos, IdEscolaAtual = idEscola,
            CursoAtual = cursos.FirstOrDefault(c => c.IdEscola == idEscola)
        };
    }

    public async Task<List<CursoDto>> ListarCursosAsync(long idInstituicao)
    {
        await using var cn = await _conexao.AbrirAsync();
        return await CursosAsync(cn, idInstituicao);
    }

    private static async Task<List<CursoDto>> CursosAsync(MySqlConnection cn, long idInstituicao) =>
        (await cn.QueryAsync<CursoDto>(SelectCurso + " WHERE c.id_instituicao = @i AND c.ativo = 1 ORDER BY c.ordem, c.nome", new { i = idInstituicao })).ToList();

    public async Task<List<PessoaDto>> ListarPessoasAsync(long idInstituicao)
    {
        await using var cn = await _conexao.AbrirAsync();
        return (await cn.QueryAsync<PessoaDto>(
            "SELECT id AS Id, nome AS Nome, nome_curto AS NomeCurto, documento AS Documento, carga_maxima AS CargaMaxima, ativo AS Ativo FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i ORDER BY nome",
            new { i = idInstituicao })).ToList();
    }

    public async Task<List<PainelLinhaDto>> ObterPainelAsync(long idInstituicao, int anoLetivo, string periodo)
    {
        await using var cn = await _conexao.AbrirAsync();
        return await PainelAsync(cn, idInstituicao, anoLetivo, periodo);
    }

    private static async Task<List<PainelLinhaDto>> PainelAsync(MySqlConnection cn, long idInstituicao, int anoLetivo, string periodo)
    {
        var cursos = await CursosAsync(cn, idInstituicao);
        var pessoas = (await cn.QueryAsync<PessoaDto>(
            "SELECT id AS Id, nome AS Nome, nome_curto AS NomeCurto, documento AS Documento, carga_maxima AS CargaMaxima, ativo AS Ativo FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i AND ativo = 1 ORDER BY nome",
            new { i = idInstituicao })).ToList();
        var vinculos = (await cn.QueryAsync<CargaLida>(
            """
            SELECT v.id_pessoa AS IdPessoa, v.id_escola AS IdEscola, v.aulas_reservadas AS Aulas, v.ordem_processamento AS Ordem
              FROM hprof_uni_pessoavinculo v
              JOIN hprof_uni_pessoaprofessor p ON p.id = v.id_pessoa
             WHERE p.id_instituicao = @i AND v.ativo = 1 AND v.ano_letivo = @a AND v.periodo = @per
            """, new { i = idInstituicao, a = anoLetivo, per = periodo })).ToList();
        var atribuidas = (await cn.QueryAsync<CargaLida>(
            """
            SELECT pc.id_pessoa AS IdPessoa, pc.id_escola AS IdEscola, SUM(c.QuantidadeAula) AS Aulas, 0 AS Ordem
              FROM hprof_uni_pessoacadastro pc
              JOIN hprof_uni_pessoaprofessor p ON p.id = pc.id_pessoa
              JOIN hprof_cadastroprofessor c ON c.ProfessorId = pc.id_cadastroprofessor
             WHERE p.id_instituicao = @i AND pc.ativo = 1 AND c.AnoLetivo = @a AND c.Periodo = @per
             GROUP BY pc.id_pessoa, pc.id_escola
            """, new { i = idInstituicao, a = anoLetivo, per = periodo })).ToList();

        var linhas = new List<PainelLinhaDto>();
        foreach (var p in pessoas)
        {
            var linha = new PainelLinhaDto { IdPessoa = p.Id, Nome = p.Nome, Documento = p.Documento, CargaMaxima = p.CargaMaxima };
            foreach (var curso in cursos)
            {
                var v = vinculos.FirstOrDefault(x => x.IdPessoa == p.Id && x.IdEscola == curso.IdEscola);
                var a = atribuidas.FirstOrDefault(x => x.IdPessoa == p.Id && x.IdEscola == curso.IdEscola);
                if (v is null && a is null) continue;
                linha.Cursos.Add(new CargaCursoDto
                {
                    IdEscola = curso.IdEscola, Curso = curso.Nome, CodigoCurso = curso.Codigo,
                    AulasReservadas = v?.Aulas ?? 0, AulasAtribuidas = a?.Aulas ?? 0, Ordem = v?.Ordem ?? curso.Ordem
                });
            }
            linha.TotalAtribuido = linha.Cursos.Sum(c => c.AulasAtribuidas);
            linha.Livre = p.CargaMaxima - linha.TotalAtribuido;
            linha.QuantidadeCursos = linha.Cursos.Count(c => c.AulasAtribuidas > 0);
            linha.Situacao = linha.TotalAtribuido > p.CargaMaxima ? "ACIMA_DA_CARGA"
                : linha.Cursos.Any(c => c.AulasReservadas > 0 && c.AulasAtribuidas > c.AulasReservadas) ? "ACIMA_DO_RESERVADO"
                : linha.TotalAtribuido == 0 ? "SEM_AULAS" : "OK";
            linhas.Add(linha);
        }
        return linhas;
    }

    public async Task<PessoaDetalheDto?> ObterPessoaAsync(long idInstituicao, long idPessoa, int anoLetivo, string periodo)
    {
        await using var cn = await _conexao.AbrirAsync();
        var pessoa = await cn.QuerySingleOrDefaultAsync<PessoaDto>(
            "SELECT id AS Id, nome AS Nome, nome_curto AS NomeCurto, documento AS Documento, carga_maxima AS CargaMaxima, ativo AS Ativo FROM hprof_uni_pessoaprofessor WHERE id = @p AND id_instituicao = @i",
            new { p = idPessoa, i = idInstituicao });
        if (pessoa is null) return null;
        var painel = (await PainelAsync(cn, idInstituicao, anoLetivo, periodo)).FirstOrDefault(x => x.IdPessoa == idPessoa);
        var detalhe = new PessoaDetalheDto { Pessoa = pessoa, Vinculos = painel?.Cursos ?? new List<CargaCursoDto>() };
        foreach (var curso in await CursosAsync(cn, idInstituicao))
        {
            if (detalhe.Vinculos.All(v => v.IdEscola != curso.IdEscola))
                detalhe.Vinculos.Add(new CargaCursoDto { IdEscola = curso.IdEscola, Curso = curso.Nome, CodigoCurso = curso.Codigo, Ordem = curso.Ordem });
            var atribuicoes = await AtribuicoesAsync(cn, curso.IdEscola, anoLetivo, periodo);
            detalhe.Atribuicoes.AddRange(atribuicoes.Where(a => a.IdPessoa == idPessoa));
        }
        detalhe.TotalAtribuido = detalhe.Vinculos.Sum(v => v.AulasAtribuidas);
        return detalhe;
    }

    public async Task<List<AtribuicaoDto>> ListarAtribuicoesCursoAsync(int idEscola, int anoLetivo, string periodo)
    {
        await using var cn = await _conexao.AbrirAsync();
        return await AtribuicoesAsync(cn, idEscola, anoLetivo, periodo);
    }

    private static async Task<List<AtribuicaoDto>> AtribuicoesAsync(MySqlConnection cn, int idEscola, int anoLetivo, string periodo)
    {
        // As ligações entre tabelas novas e antigas são sempre por número (id), nunca por texto,
        // para não depender da collation das tabelas atuais.
        var lista = (await cn.QueryAsync<AtribuicaoDto>(
            """
            SELECT c.ProfessorId AS IdCadastro, c.IdEscola AS IdEscola, c.NomeProfessor AS NomeProfessorMotor,
                   c.TurmasAtribuidas AS Turma, c.Materia AS Sigla, c.QuantidadeAula AS Aulas,
                   COALESCE(pc.id_pessoa, 0) AS IdPessoa, COALESCE(p.nome, '') AS Pessoa, COALESCE(pc.papel, 'PRINCIPAL') AS Papel
              FROM hprof_cadastroprofessor c
              LEFT JOIN hprof_uni_pessoacadastro pc ON pc.id_cadastroprofessor = c.ProfessorId AND pc.ativo = 1
              LEFT JOIN hprof_uni_pessoaprofessor p ON p.id = pc.id_pessoa
             WHERE c.IdEscola = @e AND c.AnoLetivo = @a AND c.Periodo = @p
            """, new { e = idEscola, a = anoLetivo, p = periodo })).ToList();
        var materias = (await cn.QueryAsync<SiglaNome>(
            "SELECT Sigla, Descricao AS Nome FROM hprof_materiaaulasporano WHERE EscolaId = @e AND AnoLetivo = @a AND Periodo = @p",
            new { e = idEscola, a = anoLetivo, p = periodo }))
            .GroupBy(x => x.Sigla ?? "", StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Nome ?? "", StringComparer.OrdinalIgnoreCase);
        var turmas = await TurmasAsync(cn, idEscola);
        var nomeTurma = turmas.ToDictionary(t => t.Sigla, t => t, StringComparer.OrdinalIgnoreCase);
        var curso = await cn.ExecuteScalarAsync<string>("SELECT nome FROM hprof_uni_curso WHERE id_escola = @e", new { e = idEscola }) ?? "";
        foreach (var a in lista)
        {
            a.Curso = curso;
            a.Disciplina = materias.TryGetValue(a.Sigla ?? "", out var n) && n.Length > 0 ? n : a.Sigla ?? "";
            a.TurmaNome = nomeTurma.TryGetValue(a.Turma ?? "", out var t) ? t.Nome : a.Turma ?? "";
        }
        var ordem = turmas.ToDictionary(t => t.Sigla, t => t.Ordem, StringComparer.OrdinalIgnoreCase);
        return lista.OrderBy(a => ordem.GetValueOrDefault(a.Turma ?? "", 999)).ThenBy(a => a.Disciplina).ThenBy(a => a.Papel == "PRINCIPAL" ? 0 : 1).ToList();
    }

    public async Task<List<TurmaUniversidadeDto>> ListarTurmasCursoAsync(int idEscola)
    {
        await using var cn = await _conexao.AbrirAsync();
        return await TurmasAsync(cn, idEscola);
    }

    private static async Task<List<TurmaUniversidadeDto>> TurmasAsync(MySqlConnection cn, int idEscola) =>
        (await cn.QueryAsync<TurmaUniversidadeDto>(
            """
            SELECT t.sigla AS Sigla, t.nome AS Nome, t.ordem AS Ordem
              FROM hprof_uni_turma t JOIN hprof_uni_curso c ON c.id = t.id_curso
             WHERE c.id_escola = @e AND t.ativo = 1 ORDER BY t.ordem
            """, new { e = idEscola })).ToList();

    public async Task<List<PlanilhaSalvaDto>> ListarGradesSalvasAsync(int idEscola, int anoLetivo, string periodo)
    {
        await using var cn = await _conexao.AbrirAsync();
        var publicada = await cn.ExecuteScalarAsync<int?>(
            "SELECT id_planilha FROM hprof_uni_publicacao WHERE id_escola = @e AND ano_letivo = @a AND periodo = @p AND ativo = 1 ORDER BY id DESC LIMIT 1",
            new { e = idEscola, a = anoLetivo, p = periodo });
        var lista = (await cn.QueryAsync<PlanilhaSalvaDto>(
            "SELECT Id, NomePlanilha, NomePlanilhaEscola, DataCadastro FROM hprof_horariodistribuicaojson WHERE IdEscola = @e AND AnoLetivo = @a AND Periodo = @p ORDER BY Id DESC LIMIT 50",
            new { e = idEscola, a = anoLetivo, p = periodo })).ToList();
        foreach (var x in lista) x.Publicada = x.Id == publicada;
        return lista;
    }

    public async Task<GradeCursoDto> ObterGradeAsync(int idEscola, int anoLetivo, string periodo, int? idPlanilha)
    {
        await using var cn = await _conexao.AbrirAsync();
        var curso = await cn.QuerySingleOrDefaultAsync<CursoDto>(SelectCurso + " WHERE c.id_escola = @e", new { e = idEscola });
        var resultado = new GradeCursoDto();
        if (curso is null) { resultado.Mensagem = "Esta escola não é um curso de universidade."; return resultado; }
        resultado.Curso = curso;
        resultado.NomeInstituicao = await cn.ExecuteScalarAsync<string>("SELECT nome FROM hprof_uni_instituicao WHERE id = @i", new { i = curso.IdInstituicao }) ?? "";
        resultado.Dias = curso.Dias.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        resultado.Intervalos = string.IsNullOrWhiteSpace(curso.IntervalosJson) ? new List<IntervaloDto>()
            : JsonSerializer.Deserialize<List<IntervaloDto>>(curso.IntervalosJson, Json) ?? new List<IntervaloDto>();
        var horarios = await OcupacaoUniversidadeService.HorariosAsync(cn, idEscola, anoLetivo, periodo);
        resultado.Horarios = horarios.Select((h, i) => new HorarioAulaDto { Aula = i + 1, Inicio = NomesUniversidade.Hora(h.Inicio), Fim = NomesUniversidade.Hora(h.Fim) }).ToList();
        resultado.Turmas = await TurmasAsync(cn, idEscola);

        if (idPlanilha is null or 0)
            idPlanilha = await cn.ExecuteScalarAsync<int?>(
                "SELECT id_planilha FROM hprof_uni_publicacao WHERE id_escola = @e AND ano_letivo = @a AND periodo = @p AND ativo = 1 ORDER BY id DESC LIMIT 1",
                new { e = idEscola, a = anoLetivo, p = periodo });
        var registro = await cn.QueryFirstOrDefaultAsync<OcupacaoUniversidadeService.GradeSalvaLida>(
            idPlanilha is > 0
                ? "SELECT Id, MatrizHorarios AS Matriz, Turmas FROM hprof_horariodistribuicaojson WHERE Id = @id AND IdEscola = @e"
                : "SELECT Id, MatrizHorarios AS Matriz, Turmas FROM hprof_horariodistribuicaojson WHERE IdEscola = @e AND AnoLetivo = @a AND Periodo = @p ORDER BY Id DESC LIMIT 1",
            new { id = idPlanilha, e = idEscola, a = anoLetivo, p = periodo });
        if (registro is null || string.IsNullOrWhiteSpace(registro.Matriz))
        {
            resultado.Mensagem = "Este curso ainda não tem grade salva. Processe e salve a grade do curso.";
            return resultado;
        }
        resultado.IdPlanilha = registro.Id;
        resultado.NomePlanilha = await cn.ExecuteScalarAsync<string>("SELECT COALESCE(NomePlanilhaEscola, NomePlanilha) FROM hprof_horariodistribuicaojson WHERE Id = @id", new { id = registro.Id });
        resultado.Publicada = await cn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM hprof_uni_publicacao WHERE id_escola = @e AND id_planilha = @id AND ativo = 1", new { e = idEscola, id = registro.Id }) > 0;

        var matrizes = JsonSerializer.Deserialize<string[][][]>(registro.Matriz) ?? Array.Empty<string[][]>();
        var turmasGrade = string.IsNullOrWhiteSpace(registro.Turmas) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(registro.Turmas) ?? new List<string>();
        foreach (var sigla in turmasGrade.Where(s => resultado.Turmas.All(t => !string.Equals(t.Sigla, s, StringComparison.OrdinalIgnoreCase))))
            resultado.Turmas.Add(new TurmaUniversidadeDto { Sigla = sigla, Nome = sigla, Ordem = 900 + resultado.Turmas.Count });

        var atribuicoes = await AtribuicoesAsync(cn, idEscola, anoLetivo, periodo);
        var porNome = atribuicoes.GroupBy(a => (a.NomeProfessorMotor ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        for (var d = 0; d < matrizes.Length && d < 5; d++)
        {
            var dia = matrizes[d] ?? Array.Empty<string[]>();
            for (var l = 0; l < dia.Length; l++)
            {
                var linha = dia[l] ?? Array.Empty<string>();
                for (var t = 0; t < linha.Length && t < turmasGrade.Count; t++)
                {
                    var nome = (linha[t] ?? "").Trim();
                    if (nome.Length == 0) continue;
                    var celula = new CelulaGradeDto { Dia = d + 1, Aula = l + 1, Turma = turmasGrade[t], NomeMotor = nome };
                    if (porNome.TryGetValue(nome, out var linhasCadastro))
                    {
                        celula.Sigla = linhasCadastro[0].Sigla;
                        celula.Disciplina = linhasCadastro[0].Disciplina;
                        celula.Professores = linhasCadastro.Where(x => x.Pessoa.Length > 0)
                            .OrderBy(x => x.Papel == "PRINCIPAL" ? 0 : 1).Select(x => x.Pessoa).Distinct().ToList();
                    }
                    if (celula.Professores.Count == 0) celula.Professores.Add(nome);
                    if (celula.Disciplina.Length == 0) celula.Disciplina = nome.Contains('-') ? nome[(nome.IndexOf('-') + 1)..] : "";
                    resultado.Celulas.Add(celula);
                }
            }
        }
        return resultado;
    }

    public async Task<SemanaProfessorDto?> ObterSemanaAsync(long idInstituicao, long idPessoa, int anoLetivo)
    {
        await using var cn = await _conexao.AbrirAsync();
        var pessoa = await cn.QuerySingleOrDefaultAsync<PessoaDto>(
            "SELECT id AS Id, nome AS Nome, nome_curto AS NomeCurto, documento AS Documento, carga_maxima AS CargaMaxima, ativo AS Ativo FROM hprof_uni_pessoaprofessor WHERE id = @p AND id_instituicao = @i",
            new { p = idPessoa, i = idInstituicao });
        if (pessoa is null) return null;
        var aulas = await OcupacoesAsync(cn, idInstituicao, anoLetivo, idPessoa);
        return new SemanaProfessorDto { Pessoa = pessoa, Aulas = aulas, TotalAulasPublicadas = aulas.Count };
    }

    private static async Task<List<OcupacaoDto>> OcupacoesAsync(MySqlConnection cn, long idInstituicao, int anoLetivo, long? idPessoa) =>
        (await cn.QueryAsync<OcupacaoDto>(
            """
            SELECT o.id_pessoa AS IdPessoa, p.nome AS Pessoa, o.id_escola_origem AS IdEscolaOrigem, COALESCE(c.nome, '') AS Curso,
                   o.dia_semana AS Dia, o.hora_inicio AS Inicio, o.hora_fim AS Fim, o.descricao AS Descricao
              FROM hprof_uni_pessoaocupacao o
              JOIN hprof_uni_pessoaprofessor p ON p.id = o.id_pessoa
              LEFT JOIN hprof_uni_curso c ON c.id_escola = o.id_escola_origem
             WHERE p.id_instituicao = @i AND o.ativo = 1 AND o.ano_letivo = @a AND (@p IS NULL OR o.id_pessoa = @p)
             ORDER BY p.nome, o.dia_semana, o.hora_inicio
            """, new { i = idInstituicao, a = anoLetivo, p = idPessoa })).ToList();

    public async Task<ConferenciaDto> ObterConferenciaAsync(long idInstituicao, int anoLetivo, string periodo)
    {
        await using var cn = await _conexao.AbrirAsync();
        var painel = await PainelAsync(cn, idInstituicao, anoLetivo, periodo);
        var ocupacoes = await OcupacoesAsync(cn, idInstituicao, anoLetivo, null);
        var conflitos = new List<ConflitoDto>();
        foreach (var grupo in ocupacoes.GroupBy(o => (o.IdPessoa, o.Dia)))
        {
            var lista = grupo.OrderBy(o => o.Inicio).ToList();
            for (var i = 0; i < lista.Count; i++)
                for (var j = i + 1; j < lista.Count; j++)
                    if (NomesUniversidade.Sobrepoe(lista[i].Inicio, lista[i].Fim, lista[j].Inicio, lista[j].Fim))
                        conflitos.Add(new ConflitoDto { Pessoa = lista[i].Pessoa, Dia = grupo.Key.Dia, Primeira = lista[i], Segunda = lista[j] });
        }
        var cursos = await CursosAsync(cn, idInstituicao);
        var publicados = (await cn.QueryAsync<int>(
            "SELECT DISTINCT id_escola FROM hprof_uni_publicacao WHERE ativo = 1 AND ano_letivo = @a AND periodo = @p", new { a = anoLetivo, p = periodo })).ToHashSet();
        return new ConferenciaDto
        {
            TotalPessoas = painel.Count,
            PessoasEmMaisDeUmCurso = painel.Count(p => p.QuantidadeCursos > 1),
            TotalOcupacoes = ocupacoes.Count,
            Conflitos = conflitos,
            AcimaDaCarga = painel.Where(p => p.Situacao == "ACIMA_DA_CARGA").ToList(),
            CursosSemPublicacao = cursos.Where(c => !publicados.Contains(c.IdEscola)).Select(c => c.Nome).ToList()
        };
    }

    private sealed class CargaLida
    {
        public long IdPessoa { get; set; }
        public int IdEscola { get; set; }
        public int Aulas { get; set; }
        public int Ordem { get; set; }
    }

    private sealed class SiglaNome
    {
        public string? Sigla { get; set; }
        public string? Nome { get; set; }
    }
}
