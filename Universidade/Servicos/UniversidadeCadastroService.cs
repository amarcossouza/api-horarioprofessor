using System.Text.Json;
using API_HorarioProfessor.Universidade.Contratos;
using Dapper;

namespace API_HorarioProfessor.Universidade.Servicos;

public interface IUniversidadeCadastroService
{
    Task<ReceberLoteResposta> SalvarPessoaAsync(long idInstituicao, SalvarPessoaPedido pedido, string? ip);
    Task<ReceberLoteResposta> IncluirAtribuicaoAsync(int idEscola, NovaAtribuicaoPedido pedido, string? ip);
    Task<PlanilhaLeituraDto> LerPlanilhaAsync(long idInstituicao, PlanilhaPedido pedido);
    Task<ReceberLoteResposta> EnviarPlanilhaAsync(long idInstituicao, PlanilhaPedido pedido, string? ip);
}

/// <summary>
/// Cadastros feitos pela tela e pela planilha colada. Tudo vira um envio (lote) com origem
/// TELA ou PLANILHA e passa pela mesma validação e auditoria da integração por API.
/// A tela grava na hora (o próprio usuário é quem aprova); a planilha espera aprovação.
/// </summary>
public sealed class UniversidadeCadastroService : IUniversidadeCadastroService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly UniversidadeConexao _conexao;
    private readonly IIntegracaoService _integracao;

    public UniversidadeCadastroService(UniversidadeConexao conexao, IIntegracaoService integracao)
    {
        _conexao = conexao; _integracao = integracao;
    }

    public async Task<ReceberLoteResposta> SalvarPessoaAsync(long idInstituicao, SalvarPessoaPedido pedido, string? ip)
    {
        await using var cn = await _conexao.AbrirAsync();
        var documento = pedido.Documento.Trim();
        if (pedido.Id is > 0)
        {
            // na edição a matrícula não muda: ela é o que identifica a pessoa
            documento = await cn.ExecuteScalarAsync<string>(
                "SELECT documento FROM hprof_uni_pessoaprofessor WHERE id = @id AND id_instituicao = @i", new { id = pedido.Id, i = idInstituicao }) ?? documento;
        }
        else
        {
            var jaExiste = await cn.ExecuteScalarAsync<string>(
                "SELECT nome FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i AND documento = @d", new { i = idInstituicao, d = documento });
            if (jaExiste is not null)
                return new ReceberLoteResposta { Status = "FALHA", TotalErros = 1, Mensagem = $"A matrícula {documento} já está cadastrada para {jaExiste}." };
        }
        var cursos = (await cn.QueryAsync<CursoCodigo>(
            "SELECT id_escola AS IdEscola, codigo AS Codigo FROM hprof_uni_curso WHERE id_instituicao = @i", new { i = idInstituicao }))
            .ToDictionary(c => c.IdEscola, c => c.Codigo);
        var lote = new LotePessoasConteudo
        {
            CodigoEnvio = $"TELA-PESSOA-{documento}-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            AnoLetivo = pedido.AnoLetivo,
            Periodo = string.IsNullOrWhiteSpace(pedido.Periodo) ? "Manha" : pedido.Periodo,
            Professores = { new LoteProfessor { Nome = pedido.Nome, Documento = documento, CargaMaxima = pedido.CargaMaxima, Ativo = pedido.Ativo } },
            Vinculos = pedido.Vinculos.Where(v => cursos.ContainsKey(v.IdEscola))
                .Select(v => new LoteVinculo { Professor = documento, Curso = cursos[v.IdEscola], AulasReservadas = v.AulasReservadas, Ordem = v.Ordem }).ToList()
        };
        return await _integracao.ReceberAsync(idInstituicao, null, OrigensLote.Tela, TiposLote.Pessoas,
            JsonSerializer.Serialize(lote, Json), pedido.Usuario, ip, aplicarSeValido: true);
    }

    public async Task<ReceberLoteResposta> IncluirAtribuicaoAsync(int idEscola, NovaAtribuicaoPedido pedido, string? ip)
    {
        await using var cn = await _conexao.AbrirAsync();
        var curso = await cn.QuerySingleOrDefaultAsync<CursoCodigo>(
            "SELECT id AS Id, id_instituicao AS IdInstituicao, id_escola AS IdEscola, codigo AS Codigo FROM hprof_uni_curso WHERE id_escola = @e", new { e = idEscola });
        if (curso is null) return new ReceberLoteResposta { Status = "FALHA", Mensagem = "Esta escola não é um curso de universidade." };
        var turma = await cn.QuerySingleOrDefaultAsync<TurmaCodigo>(
            "SELECT codigo AS Codigo, nome AS Nome FROM hprof_uni_turma WHERE id_curso = @c AND sigla = @s", new { c = curso.Id, s = pedido.TurmaSigla.Trim() });
        if (turma is null) return new ReceberLoteResposta { Status = "FALHA", Mensagem = $"Turma {pedido.TurmaSigla} não encontrada no curso." };
        var documentos = new List<string>();
        foreach (var id in new[] { pedido.IdPessoa, pedido.IdPessoaApoio ?? 0 }.Where(x => x > 0))
        {
            var doc = await cn.ExecuteScalarAsync<string>("SELECT documento FROM hprof_uni_pessoaprofessor WHERE id = @id AND id_instituicao = @i", new { id, i = curso.IdInstituicao });
            if (doc is null) return new ReceberLoteResposta { Status = "FALHA", Mensagem = "Professor não encontrado." };
            documentos.Add(doc);
        }
        var lote = MontarLoteParcial(curso.Codigo, pedido.AnoLetivo, pedido.Periodo, $"TELA-ATRIB-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            new[] { (turma.Codigo, pedido.Disciplina.Trim(), documentos, pedido.Aulas) });
        return await _integracao.ReceberAsync(curso.IdInstituicao, null, OrigensLote.Tela, TiposLote.Curso,
            JsonSerializer.Serialize(lote, Json), pedido.Usuario, ip, aplicarSeValido: true);
    }

    public async Task<PlanilhaLeituraDto> LerPlanilhaAsync(long idInstituicao, PlanilhaPedido pedido)
    {
        var (leitura, _) = await LerAsync(idInstituicao, pedido);
        return leitura;
    }

    public async Task<ReceberLoteResposta> EnviarPlanilhaAsync(long idInstituicao, PlanilhaPedido pedido, string? ip)
    {
        var (leitura, lote) = await LerAsync(idInstituicao, pedido);
        if (lote is null || leitura.Prontas == 0)
            return new ReceberLoteResposta { Status = "FALHA", Mensagem = "Nenhuma linha pronta para enviar." };
        return await _integracao.ReceberAsync(idInstituicao, null, OrigensLote.Planilha, TiposLote.Curso,
            JsonSerializer.Serialize(lote, Json), pedido.Usuario, ip, aplicarSeValido: false);
    }

    /// <summary>Lê o texto colado (colunas separadas por tabulação, ponto e vírgula ou vírgula).</summary>
    private async Task<(PlanilhaLeituraDto Leitura, LoteCursoConteudo? Lote)> LerAsync(long idInstituicao, PlanilhaPedido pedido)
    {
        var leitura = new PlanilhaLeituraDto();
        await using var cn = await _conexao.AbrirAsync();
        var curso = await cn.QuerySingleOrDefaultAsync<CursoCodigo>(
            "SELECT id AS Id, id_instituicao AS IdInstituicao, id_escola AS IdEscola, codigo AS Codigo, nome AS Nome FROM hprof_uni_curso WHERE id_escola = @e AND id_instituicao = @i",
            new { e = pedido.IdEscola, i = idInstituicao });
        if (curso is null)
        {
            leitura.Linhas.Add(new PlanilhaLinhaDto { Linha = 0, Mensagem = "Escolha um curso da instituição." });
            return (leitura, null);
        }
        leitura.Curso = curso.Nome;

        var linhasTexto = (pedido.Texto ?? "").Replace("\r", "").Split('\n').Where(l => l.Trim().Length > 0).ToList();
        if (linhasTexto.Count == 0) return (leitura, null);
        var separador = linhasTexto[0].Contains('\t') ? '\t' : linhasTexto[0].Contains(';') ? ';' : ',';
        var cabecalho = linhasTexto[0].Split(separador).Select(c => NomesUniversidade.SemAcentos(c).Trim().ToUpperInvariant()).ToList();
        int Coluna(params string[] nomes) => cabecalho.FindIndex(c => nomes.Any(n => c.StartsWith(n, StringComparison.Ordinal)));
        var cDoc = Coluna("MATRICULA", "DOCUMENTO", "CPF");
        var cProf = Coluna("PROFESSOR", "NOME");
        var cFase = Coluna("FASE", "TURMA", "PERIODO");
        var cDisc = Coluna("DISCIPLINA", "MATERIA", "COMPONENTE");
        var cAulas = Coluna("AULAS", "CH", "CARGA");
        var cApoio = Coluna("APOIO", "SEGUNDO", "CO-DOCENTE", "CODOCENTE");
        var temCabecalho = cDoc >= 0 && cDisc >= 0;
        if (!temCabecalho) { cDoc = 0; cProf = 1; cFase = 3; cDisc = 4; cAulas = 5; cApoio = -1; } // ordem do modelo: Matrícula, Professor, Curso, Fase, Disciplina, Aulas
        leitura.ColunasReconhecidas = new[] { (cDoc, "Matrícula"), (cProf, "Professor"), (cFase, "Fase"), (cDisc, "Disciplina"), (cAulas, "Aulas"), (cApoio, "Apoio") }
            .Where(x => x.Item1 >= 0).Select(x => x.Item2).ToList();

        var turmas = (await cn.QueryAsync<TurmaCodigo>(
            "SELECT codigo AS Codigo, nome AS Nome, sigla AS Sigla FROM hprof_uni_turma WHERE id_curso = @c AND ativo = 1", new { c = curso.Id })).ToList();
        var pessoas = (await cn.QueryAsync<PessoaDto>(
            "SELECT id AS Id, nome AS Nome, documento AS Documento, carga_maxima AS CargaMaxima, ativo AS Ativo FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i AND ativo = 1",
            new { i = idInstituicao })).ToDictionary(p => p.Documento, p => p, StringComparer.OrdinalIgnoreCase);
        var jaAtribuido = (await cn.QueryAsync<PessoaAulas>(
            """
            SELECT pc.id_pessoa AS IdPessoa, SUM(c.QuantidadeAula) AS Aulas
              FROM hprof_uni_pessoacadastro pc JOIN hprof_cadastroprofessor c ON c.ProfessorId = pc.id_cadastroprofessor
             WHERE pc.ativo = 1 AND c.AnoLetivo = @a GROUP BY pc.id_pessoa
            """, new { a = pedido.AnoLetivo })).ToDictionary(x => x.IdPessoa, x => x.Aulas);

        // aulas de atribuições que a planilha vai substituir (mesmo código) não contam duas vezes
        var existentes = (await cn.QueryAsync<CodigoAulas>(
            """
            SELECT m.codigo_externo AS Codigo, c.QuantidadeAula AS Aulas
              FROM hprof_uni_integracaomapa m JOIN hprof_cadastroprofessor c ON c.ProfessorId = m.id_interno
             WHERE m.id_instituicao = @i AND m.entidade = 'ATRIBUICAO' AND m.ativo = 1
            """, new { i = idInstituicao })).GroupBy(x => x.Codigo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Aulas, StringComparer.OrdinalIgnoreCase);

        string Celula(string[] partes, int i) => i >= 0 && i < partes.Length ? partes[i].Trim().Trim('"') : "";
        TurmaCodigo? AcharTurma(string texto)
        {
            var t = NomesUniversidade.SemAcentos(texto).Trim().ToUpperInvariant().Replace("º", "").Replace("ª", "");
            if (t.Length == 0) return null;
            return turmas.FirstOrDefault(x =>
                string.Equals(NomesUniversidade.SemAcentos(x.Nome).ToUpperInvariant().Replace("ª", "").Replace("º", ""), t, StringComparison.Ordinal) ||
                string.Equals(x.Sigla, t, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Codigo, texto.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (t.All(char.IsDigit) && string.Equals(x.Sigla, t + "F", StringComparison.OrdinalIgnoreCase)));
        }

        var somaNaPlanilha = new Dictionary<long, int>();
        var itens = new List<(string Turma, string Disciplina, List<string> Documentos, int Aulas)>();
        for (var i = temCabecalho ? 1 : 0; i < linhasTexto.Count; i++)
        {
            var partes = linhasTexto[i].Split(separador);
            var linha = new PlanilhaLinhaDto
            {
                Linha = i + 1, Documento = Celula(partes, cDoc), Professor = Celula(partes, cProf), Fase = Celula(partes, cFase),
                Disciplina = Celula(partes, cDisc), Apoio = cApoio >= 0 ? Celula(partes, cApoio) : null
            };
            int.TryParse(Celula(partes, cAulas), out var aulas);
            linha.Aulas = aulas;
            leitura.Linhas.Add(linha);

            if (!pessoas.TryGetValue(linha.Documento, out var pessoa)) { linha.Mensagem = $"Matrícula {linha.Documento} não cadastrada na instituição."; continue; }
            if (linha.Professor.Length == 0) linha.Professor = pessoa.Nome;
            var turma = AcharTurma(linha.Fase);
            if (turma is null) { linha.Mensagem = $"Fase '{linha.Fase}' não existe em {curso.Nome}."; continue; }
            if (linha.Disciplina.Length == 0) { linha.Mensagem = "Informe a disciplina."; continue; }
            if (aulas is < 1 or > 20) { linha.Mensagem = "Aulas deve ficar entre 1 e 20."; continue; }
            var documentos = new List<string> { pessoa.Documento };
            if (!string.IsNullOrWhiteSpace(linha.Apoio))
            {
                if (!pessoas.ContainsKey(linha.Apoio)) { linha.Mensagem = $"Professor de apoio {linha.Apoio} não cadastrado."; continue; }
                documentos.Add(linha.Apoio);
            }
            var codigoAtribuicao = IntegracaoAplicador.ChaveDoCurso(curso.Codigo, $"{turma.Codigo}-{Codigo(linha.Disciplina)}-{Codigo(pessoa.Documento)}");
            var total = jaAtribuido.GetValueOrDefault(pessoa.Id) - existentes.GetValueOrDefault(codigoAtribuicao)
                        + somaNaPlanilha.GetValueOrDefault(pessoa.Id) + aulas;
            if (total > pessoa.CargaMaxima) { linha.Mensagem = $"Passa da carga: {total} de {pessoa.CargaMaxima}."; continue; }
            somaNaPlanilha[pessoa.Id] = somaNaPlanilha.GetValueOrDefault(pessoa.Id) + aulas;
            linha.Ok = true;
            linha.Mensagem = $"OK · {turma.Nome} · total do professor {total} de {pessoa.CargaMaxima}";
            itens.Add((turma.Codigo, linha.Disciplina, documentos, aulas));
        }
        var lote = itens.Count == 0 ? null : MontarLoteParcial(curso.Codigo, pedido.AnoLetivo, pedido.Periodo,
            $"PLANILHA-{curso.Codigo}-{DateTime.UtcNow:yyyyMMddHHmmssfff}", itens);
        return (leitura, lote);
    }

    private static LoteCursoConteudo MontarLoteParcial(string codigoCurso, int ano, string periodo, string codigoEnvio,
        IEnumerable<(string Turma, string Disciplina, List<string> Documentos, int Aulas)> itens)
    {
        var lote = new LoteCursoConteudo
        {
            CodigoEnvio = codigoEnvio, AnoLetivo = ano, Periodo = string.IsNullOrWhiteSpace(periodo) ? "Manha" : periodo, Parcial = true,
            Curso = new LoteCurso { Codigo = codigoCurso }
        };
        foreach (var (turma, disciplina, documentos, aulas) in itens)
        {
            var codigoDisciplina = $"{turma}-{Codigo(disciplina)}";
            var existente = lote.Disciplinas.FirstOrDefault(d => d.Codigo == codigoDisciplina);
            if (existente is null)
                lote.Disciplinas.Add(new LoteDisciplina { Codigo = codigoDisciplina, Nome = disciplina, Turma = turma, AulasSemana = aulas });
            else
                existente.AulasSemana += aulas; // várias linhas da mesma disciplina somam
            lote.Atribuicoes.Add(new LoteAtribuicao
            {
                Codigo = $"{codigoDisciplina}-{Codigo(documentos[0])}", Disciplina = codigoDisciplina, Professores = documentos, AulasSemana = aulas,
                BlocoAulas = aulas % 2 == 0 ? 2 : 1
            });
        }
        return lote;
    }

    private static string Codigo(string texto)
    {
        var limpo = new string(NomesUniversidade.SemAcentos(texto).ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        return limpo.Length > 40 ? limpo[..40] : limpo;
    }

    private sealed class CursoCodigo
    {
        public long Id { get; set; }
        public long IdInstituicao { get; set; }
        public int IdEscola { get; set; }
        public string Codigo { get; set; } = "";
        public string Nome { get; set; } = "";
    }

    private sealed class TurmaCodigo
    {
        public string Codigo { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Sigla { get; set; } = "";
    }

    private sealed class CodigoAulas
    {
        public string Codigo { get; set; } = "";
        public int Aulas { get; set; }
    }

    private sealed class PessoaAulas
    {
        public long IdPessoa { get; set; }
        public int Aulas { get; set; }
    }
}
