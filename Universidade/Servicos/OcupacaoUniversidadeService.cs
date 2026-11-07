using System.Text.Json;
using API_HorarioProfessor.Universidade.Contratos;
using Dapper;

namespace API_HorarioProfessor.Universidade.Servicos;

/// <summary>Um horário do curso que o motor não pode usar porque a pessoa está em outro curso.</summary>
public sealed class BloqueioUniversidade
{
    public int IdCadastro { get; set; }
    public string NomeProfessor { get; set; } = "";
    public int Dia { get; set; }   // 1 = segunda … 5 = sexta
    public int Aula { get; set; }  // 1 = primeira aula do dia
    public string Descricao { get; set; } = "";
}

public interface IOcupacaoUniversidadeService
{
    Task<PublicacaoResultadoDto> PublicarAsync(int idEscola, PublicarPedido pedido);
    Task<List<BloqueioUniversidade>> ObterBloqueiosAsync(int idEscola, int anoLetivo, string periodo);
}

/// <summary>
/// Publicação de uma grade salva do curso e conversão das aulas em ocupação da pessoa
/// (horas do relógio). Na hora de montar outro curso, a ocupação vira AC automático.
/// </summary>
public sealed class OcupacaoUniversidadeService : IOcupacaoUniversidadeService
{
    private readonly UniversidadeConexao _conexao;

    public OcupacaoUniversidadeService(UniversidadeConexao conexao) => _conexao = conexao;

    public async Task<PublicacaoResultadoDto> PublicarAsync(int idEscola, PublicarPedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.Usuario)) return new PublicacaoResultadoDto { Mensagem = "Informe o usuário." };
        await using var cn = await _conexao.AbrirAsync();
        var cursoId = await cn.ExecuteScalarAsync<long?>("SELECT id FROM hprof_uni_curso WHERE id_escola = @e AND ativo = 1", new { e = idEscola });
        if (cursoId is null) return new PublicacaoResultadoDto { Mensagem = "Esta escola não é um curso de universidade." };

        var leitura = await LerGradeSalvaAsync(cn, idEscola, pedido.AnoLetivo, pedido.Periodo, pedido.IdPlanilha);
        if (leitura is null) return new PublicacaoResultadoDto { Mensagem = "Grade salva não encontrada para este curso." };
        var (matrizes, turmas) = leitura.Value;
        var horarios = await HorariosAsync(cn, idEscola, pedido.AnoLetivo, pedido.Periodo);
        if (horarios.Count == 0) return new PublicacaoResultadoDto { Mensagem = "O curso não tem os horários das aulas cadastrados." };

        var cadastro = (await cn.QueryAsync<IdNome>(
            "SELECT ProfessorId AS Id, NomeProfessor AS Nome FROM hprof_cadastroprofessor WHERE IdEscola = @e AND AnoLetivo = @a AND Periodo = @p",
            new { e = idEscola, a = pedido.AnoLetivo, p = pedido.Periodo }))
            .GroupBy(x => (x.Nome ?? "").Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        var pessoasPorCadastro = (await cn.QueryAsync<CadastroPessoa>(
            "SELECT id_cadastroprofessor AS IdCadastro, id_pessoa AS IdPessoa FROM hprof_uni_pessoacadastro WHERE id_escola = @e AND ativo = 1", new { e = idEscola }))
            .GroupBy(x => x.IdCadastro).ToDictionary(g => g.Key, g => g.Select(x => x.IdPessoa).Distinct().ToList());

        // aulas de cada pessoa: (pessoa, dia) → lista de (aula, cadastro, turma)
        var aulas = new List<(long Pessoa, int Dia, int Aula, int Cadastro, string Turma, string Nome)>();
        var semProfessor = 0;
        var totalAulas = 0;
        for (var d = 0; d < matrizes.Length && d < 5; d++)
        {
            var dia = matrizes[d] ?? Array.Empty<string[]>();
            for (var l = 0; l < dia.Length && l < horarios.Count; l++)
            {
                var linha = dia[l] ?? Array.Empty<string>();
                for (var t = 0; t < linha.Length; t++)
                {
                    var nome = (linha[t] ?? "").Trim();
                    if (nome.Length == 0) continue;
                    totalAulas++;
                    if (!cadastro.TryGetValue(nome, out var idCad) || !pessoasPorCadastro.TryGetValue(idCad, out var pessoas)) { semProfessor++; continue; }
                    var turma = t < turmas.Count ? turmas[t] : "";
                    foreach (var p in pessoas) aulas.Add((p, d + 1, l + 1, idCad, turma, nome));
                }
            }
        }

        await using var tx = await cn.BeginTransactionAsync();
        await cn.ExecuteAsync(
            "UPDATE hprof_uni_publicacao SET ativo = 0 WHERE id_escola = @e AND ano_letivo = @a AND periodo = @p AND ativo = 1",
            new { e = idEscola, a = pedido.AnoLetivo, p = pedido.Periodo }, tx);
        await cn.ExecuteAsync(
            "UPDATE hprof_uni_pessoaocupacao SET ativo = 0, atualizado_por = @u WHERE id_escola_origem = @e AND ano_letivo = @a AND periodo = @p AND origem = 'PUBLICACAO' AND ativo = 1",
            new { e = idEscola, a = pedido.AnoLetivo, p = pedido.Periodo, u = pedido.Usuario }, tx);
        var idPublicacao = await cn.ExecuteScalarAsync<long>(
            """
            INSERT INTO hprof_uni_publicacao (id_escola, ano_letivo, periodo, id_planilha, publicado_por, total_aulas)
            VALUES (@e, @a, @p, @pl, @u, @t);
            SELECT LAST_INSERT_ID();
            """, new { e = idEscola, a = pedido.AnoLetivo, p = pedido.Periodo, pl = pedido.IdPlanilha, u = pedido.Usuario, t = totalAulas }, tx);

        var totalOcupacoes = 0;
        foreach (var grupo in aulas.GroupBy(x => (x.Pessoa, x.Dia, x.Cadastro, x.Turma)))
        {
            // junta aulas seguidas sem intervalo (ex.: 08:20–09:10 e 09:10–10:00 → 08:20–10:00)
            var ordenadas = grupo.OrderBy(x => x.Aula).ToList();
            var i = 0;
            while (i < ordenadas.Count)
            {
                var inicioAula = ordenadas[i].Aula;
                var fimAula = inicioAula;
                while (i + 1 < ordenadas.Count && ordenadas[i + 1].Aula == fimAula + 1 && horarios[fimAula - 1].Fim == horarios[fimAula].Inicio)
                {
                    i++; fimAula++;
                }
                await cn.ExecuteAsync(
                    """
                    INSERT INTO hprof_uni_pessoaocupacao (id_pessoa, id_escola_origem, ano_letivo, periodo, dia_semana, hora_inicio, hora_fim, descricao, origem, id_publicacao, criado_por)
                    VALUES (@pessoa, @e, @a, @p, @dia, @ini, @fim, @desc, 'PUBLICACAO', @pub, @u)
                    """, new
                    {
                        pessoa = grupo.Key.Pessoa, e = idEscola, a = pedido.AnoLetivo, p = pedido.Periodo, dia = grupo.Key.Dia,
                        ini = horarios[inicioAula - 1].Inicio, fim = horarios[fimAula - 1].Fim,
                        desc = $"{grupo.Key.Turma} · {ordenadas[i].Nome}", pub = idPublicacao, u = pedido.Usuario
                    }, tx);
                totalOcupacoes++;
                i++;
            }
        }
        await tx.CommitAsync();
        return new PublicacaoResultadoDto
        {
            IdPublicacao = idPublicacao, TotalAulas = totalAulas, TotalOcupacoes = totalOcupacoes, CelulasSemProfessorConhecido = semProfessor,
            Mensagem = $"Grade publicada: {totalAulas} aulas, {totalOcupacoes} horários de ocupação gravados." +
                       (semProfessor > 0 ? $" {semProfessor} aula(s) com professor fora do cadastro da universidade não geraram ocupação." : "")
        };
    }

    public async Task<List<BloqueioUniversidade>> ObterBloqueiosAsync(int idEscola, int anoLetivo, string periodo)
    {
        await using var cn = await _conexao.AbrirAsync();
        var ehCurso = await cn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM hprof_uni_curso WHERE id_escola = @e AND ativo = 1", new { e = idEscola });
        if (ehCurso == 0) return new List<BloqueioUniversidade>();

        var horarios = await HorariosAsync(cn, idEscola, anoLetivo, periodo);
        if (horarios.Count == 0) return new List<BloqueioUniversidade>();

        var linhas = (await cn.QueryAsync<CadastroPessoa>(
            """
            SELECT pc.id_cadastroprofessor AS IdCadastro, c.NomeProfessor AS Nome, pc.id_pessoa AS IdPessoa
              FROM hprof_uni_pessoacadastro pc
              JOIN hprof_cadastroprofessor c ON c.ProfessorId = pc.id_cadastroprofessor
             WHERE pc.id_escola = @e AND pc.ativo = 1
               AND c.AnoLetivo = @a AND c.Periodo = @p
            """, new { e = idEscola, a = anoLetivo, p = periodo })).ToList();
        if (linhas.Count == 0) return new List<BloqueioUniversidade>();

        var ocupacoes = (await cn.QueryAsync<OcupacaoLida>(
            """
            SELECT id_pessoa AS IdPessoa, dia_semana AS Dia, hora_inicio AS Inicio, hora_fim AS Fim, descricao AS Descricao
              FROM hprof_uni_pessoaocupacao
             WHERE ativo = 1 AND ano_letivo = @a AND id_escola_origem <> @e AND id_pessoa IN @ids
            """, new { a = anoLetivo, e = idEscola, ids = linhas.Select(x => x.IdPessoa).Distinct().ToList() }))
            .GroupBy(x => x.IdPessoa).ToDictionary(g => g.Key, g => g.ToList());

        var resultado = new List<BloqueioUniversidade>();
        foreach (var linha in linhas)
        {
            if (!ocupacoes.TryGetValue(linha.IdPessoa, out var lista)) continue;
            foreach (var o in lista.Where(oc => oc.Dia is >= 1 and <= 5))
            {
                for (var aula = 1; aula <= horarios.Count; aula++)
                {
                    var (ini, fim) = horarios[aula - 1];
                    if (ini == fim) continue;
                    if (NomesUniversidade.Sobrepoe(ini, fim, o.Inicio, o.Fim))
                        resultado.Add(new BloqueioUniversidade
                        {
                            IdCadastro = linha.IdCadastro, NomeProfessor = linha.Nome, Dia = o.Dia, Aula = aula,
                            Descricao = $"Ocupado em outro curso {NomesUniversidade.Hora(o.Inicio)}–{NomesUniversidade.Hora(o.Fim)} ({o.Descricao})"
                        });
                }
            }
        }
        return resultado
            .GroupBy(b => (b.IdCadastro, b.Dia, b.Aula)).Select(g => g.First()).ToList();
    }

    internal static async Task<List<(TimeSpan Inicio, TimeSpan Fim)>> HorariosAsync(MySqlConnector.MySqlConnection cn, int idEscola, int ano, string periodo)
    {
        var texto = await cn.QueryFirstOrDefaultAsync<string>(
            """
            SELECT Horarios FROM hprof_gradehorariosconfig
             WHERE IdEscola = @e AND Periodo = @p
             ORDER BY (AnoLetivo = @a) DESC, AnoLetivo DESC
             LIMIT 1
            """, new { e = idEscola, p = periodo, a = ano });
        return NomesUniversidade.LerHorariosConfig(texto);
    }

    internal static async Task<(string[][][] Matrizes, List<string> Turmas)?> LerGradeSalvaAsync(MySqlConnector.MySqlConnection cn, int idEscola, int ano, string periodo, int? idPlanilha)
    {
        var registro = await cn.QueryFirstOrDefaultAsync<GradeSalvaLida>(
            idPlanilha is > 0
                ? "SELECT Id, MatrizHorarios AS Matriz, Turmas FROM hprof_horariodistribuicaojson WHERE Id = @id AND IdEscola = @e"
                : "SELECT Id, MatrizHorarios AS Matriz, Turmas FROM hprof_horariodistribuicaojson WHERE IdEscola = @e AND AnoLetivo = @a AND Periodo = @p ORDER BY Id DESC LIMIT 1",
            new { id = idPlanilha, e = idEscola, a = ano, p = periodo });
        if (registro is null || string.IsNullOrWhiteSpace(registro.Matriz)) return null;
        var matrizes = JsonSerializer.Deserialize<string[][][]>(registro.Matriz) ?? Array.Empty<string[][]>();
        var turmas = string.IsNullOrWhiteSpace(registro.Turmas) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(registro.Turmas) ?? new List<string>();
        return (matrizes, turmas);
    }

    internal sealed class GradeSalvaLida
    {
        public int Id { get; set; }
        public string? Matriz { get; set; }
        public string? Turmas { get; set; }
    }

    private sealed class IdNome
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
    }

    private sealed class CadastroPessoa
    {
        public int IdCadastro { get; set; }
        public string Nome { get; set; } = "";
        public long IdPessoa { get; set; }
    }

    private sealed class OcupacaoLida
    {
        public long IdPessoa { get; set; }
        public int Dia { get; set; }
        public TimeSpan Inicio { get; set; }
        public TimeSpan Fim { get; set; }
        public string? Descricao { get; set; }
    }
}
