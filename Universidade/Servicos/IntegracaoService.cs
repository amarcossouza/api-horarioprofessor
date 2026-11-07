using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API_HorarioProfessor.Universidade.Contratos;
using Dapper;
using MySqlConnector;

namespace API_HorarioProfessor.Universidade.Servicos;

public interface IIntegracaoService
{
    Task<ClienteIntegracao?> AutenticarClienteAsync(string? chave);
    Task<ReceberLoteResposta> ReceberAsync(long idInstituicao, long? idCliente, string origem, string tipo, string conteudoJson, string enviadoPor, string? ip, bool aplicarSeValido = false);
    Task<List<LoteResumoDto>> ListarAsync(long idInstituicao, string? status);
    Task<LoteDetalheDto?> DetalheAsync(long idInstituicao, long idLote, string? usuarioQueVisualizou, string? ip, bool calcularPrevia);
    Task<ReceberLoteResposta> AprovarAsync(long idInstituicao, long idLote, string usuario, string? ip);
    Task<ReceberLoteResposta> RejeitarAsync(long idInstituicao, long idLote, string usuario, string? motivo, string? ip);
}

/// <summary>
/// Fluxo dos envios: receber (guarda como chegou) → validar → aprovar → gravar.
/// Toda etapa fica na auditoria com usuário, data e IP.
/// </summary>
public sealed class IntegracaoService : IIntegracaoService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly UniversidadeConexao _conexao;
    private readonly IntegracaoValidador _validador;
    private readonly IntegracaoAplicador _aplicador;
    private readonly ILogger<IntegracaoService> _logger;

    public IntegracaoService(UniversidadeConexao conexao, IntegracaoValidador validador, IntegracaoAplicador aplicador, ILogger<IntegracaoService> logger)
    {
        _conexao = conexao; _validador = validador; _aplicador = aplicador; _logger = logger;
    }

    public static string Sha256(string texto) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto))).ToLowerInvariant();

    public async Task<ClienteIntegracao?> AutenticarClienteAsync(string? chave)
    {
        if (string.IsNullOrWhiteSpace(chave)) return null;
        await using var cn = await _conexao.AbrirAsync();
        return await cn.QuerySingleOrDefaultAsync<ClienteIntegracao>(
            """
            SELECT c.id AS Id, c.id_instituicao AS IdInstituicao, c.sigla AS Sigla, c.aplicacao_automatica AS AplicacaoAutomatica
              FROM hprof_uni_integracaocliente c
              JOIN hprof_uni_instituicao i ON i.id = c.id_instituicao AND i.ativo = 1
             WHERE c.chave_hash = @h AND c.ativo = 1
            """, new { h = Sha256(chave.Trim()) });
    }

    public async Task<ReceberLoteResposta> ReceberAsync(long idInstituicao, long? idCliente, string origem, string tipo, string conteudoJson, string enviadoPor, string? ip, bool aplicarSeValido = false)
    {
        tipo = tipo.Trim().ToUpperInvariant();
        if (tipo is not (TiposLote.Pessoas or TiposLote.Curso))
            return Falha($"Tipo de envio '{tipo}' desconhecido. Use PESSOAS ou CURSO.");

        string codigoEnvio; int ano; string periodo; int itens;
        try
        {
            if (tipo == TiposLote.Pessoas)
            {
                var l = JsonSerializer.Deserialize<LotePessoasConteudo>(conteudoJson, Json) ?? new LotePessoasConteudo();
                codigoEnvio = l.CodigoEnvio; ano = l.AnoLetivo; periodo = l.Periodo; itens = l.Professores.Count + l.Vinculos.Count;
            }
            else
            {
                var l = JsonSerializer.Deserialize<LoteCursoConteudo>(conteudoJson, Json) ?? new LoteCursoConteudo();
                codigoEnvio = l.CodigoEnvio; ano = l.AnoLetivo; periodo = l.Periodo; itens = l.Atribuicoes.Count + l.Disciplinas.Count + l.Turmas.Count;
            }
        }
        catch (JsonException ex)
        {
            return Falha($"O conteúdo não é um JSON válido para {tipo}: {ex.Message}");
        }
        if (string.IsNullOrWhiteSpace(codigoEnvio)) return Falha("Informe codigoEnvio: um identificador único deste envio.");
        if (string.IsNullOrWhiteSpace(periodo)) periodo = "Manha";

        var hash = Sha256(conteudoJson);
        await using var cn = await _conexao.AbrirAsync();
        var existente = await cn.QuerySingleOrDefaultAsync<LoteExistente>(
            "SELECT id AS Id, hash_conteudo AS Hash, status AS Status FROM hprof_uni_integracaolote WHERE id_instituicao = @i AND codigo_envio = @c",
            new { i = idInstituicao, c = codigoEnvio.Trim() });
        if (existente is not null)
        {
            if (existente.Hash == hash)
                return new ReceberLoteResposta { IdLote = existente.Id, CodigoEnvio = codigoEnvio, Status = existente.Status, JaRecebido = true, Mensagem = "Este envio já foi recebido; nada foi alterado." };
            return Falha($"Já existe um envio {codigoEnvio} com conteúdo diferente. Use outro codigoEnvio.");
        }

        var idLote = await cn.ExecuteScalarAsync<long>(
            """
            INSERT INTO hprof_uni_integracaolote (id_instituicao, id_cliente, origem, codigo_envio, tipo, ano_letivo, periodo,
                                                  conteudo_json, hash_conteudo, status, total_itens, enviado_por, recebido_de_ip)
            VALUES (@i, @cli, @o, @c, @t, @a, @p, @j, @h, 'RECEBIDO', @n, @u, @ip);
            SELECT LAST_INSERT_ID();
            """, new { i = idInstituicao, cli = idCliente, o = origem, c = codigoEnvio.Trim(), t = tipo, a = ano, p = periodo, j = conteudoJson, h = hash, n = itens, u = enviadoPor, ip });
        await AuditarAsync(cn, null, idLote, "RECEBIDO", enviadoPor, origem == OrigensLote.Api ? "INTEGRACAO" : "USUARIO", ip, $"Origem {origem}, {itens} itens");

        var resposta = await ValidarAsync(cn, idInstituicao, idLote, tipo, conteudoJson, primeiraValidacao: true);
        resposta.CodigoEnvio = codigoEnvio;

        var automatico = aplicarSeValido;
        if (!automatico && idCliente is not null)
            automatico = await cn.ExecuteScalarAsync<bool>("SELECT aplicacao_automatica FROM hprof_uni_integracaocliente WHERE id = @id", new { id = idCliente });
        if (automatico && resposta.Status == StatusLote.Validado)
            return await AprovarAsync(idInstituicao, idLote, enviadoPor, ip);
        return resposta;
    }

    /// <summary>
    /// primeiraValidacao=true grava todas as mensagens. Na aprovação o envio é conferido de novo
    /// (o banco pode ter mudado); se aparecer erro, só os erros novos são gravados.
    /// </summary>
    private async Task<ReceberLoteResposta> ValidarAsync(MySqlConnection cn, long idInstituicao, long idLote, string tipo, string conteudoJson, bool primeiraValidacao)
    {
        List<LoteMensagemDto> mensagens;
        if (tipo == TiposLote.Pessoas)
            mensagens = await _validador.ValidarPessoasAsync(cn, idInstituicao, JsonSerializer.Deserialize<LotePessoasConteudo>(conteudoJson, Json)!);
        else
            mensagens = await _validador.ValidarCursoAsync(cn, idInstituicao, JsonSerializer.Deserialize<LoteCursoConteudo>(conteudoJson, Json)!);

        var erros = mensagens.Count(x => x.Severidade == "ERRO");
        var avisos = mensagens.Count - erros;
        if (!primeiraValidacao && erros == 0)
            return new ReceberLoteResposta { IdLote = idLote, Status = StatusLote.Validado, TotalAvisos = avisos, Mensagens = mensagens };
        foreach (var msg in primeiraValidacao ? mensagens : mensagens.Where(x => x.Severidade == "ERRO").ToList())
        {
            await cn.ExecuteAsync(
                "INSERT INTO hprof_uni_integracaoerro (id_lote, secao, item, campo, mensagem, severidade) VALUES (@l, @s, @i, @c, @m, @sev)",
                new { l = idLote, s = Cortar(msg.Secao, 40), i = msg.Item, c = msg.Campo is null ? null : Cortar(msg.Campo, 60), m = Cortar(msg.Mensagem, 500), sev = msg.Severidade });
        }
        var status = erros > 0 ? StatusLote.ComErros : StatusLote.Validado;
        await cn.ExecuteAsync(
            "UPDATE hprof_uni_integracaolote SET status = @s, total_erros = @e, total_avisos = @a, validado_em = NOW(3) WHERE id = @l",
            new { s = status, e = erros, a = avisos, l = idLote });
        await AuditarAsync(cn, null, idLote, "VALIDADO", "sistema", "SISTEMA", null,
            (primeiraValidacao ? "" : "Nova conferência na aprovação: ") + $"{erros} erro(s), {avisos} aviso(s)");
        return new ReceberLoteResposta
        {
            IdLote = idLote, Status = status, TotalErros = erros, TotalAvisos = avisos, Mensagens = mensagens,
            Mensagem = erros > 0 ? "Envio recebido com erros. Corrija e envie de novo com outro codigoEnvio." : "Envio recebido e validado. Aguardando aprovação."
        };
    }

    public async Task<List<LoteResumoDto>> ListarAsync(long idInstituicao, string? status)
    {
        await using var cn = await _conexao.AbrirAsync();
        return (await cn.QueryAsync<LoteResumoDto>(
            SelectLote + " WHERE id_instituicao = @i AND (@s IS NULL OR status = @s) ORDER BY id DESC LIMIT 200",
            new { i = idInstituicao, s = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant() })).ToList();
    }

    public async Task<LoteDetalheDto?> DetalheAsync(long idInstituicao, long idLote, string? usuarioQueVisualizou, string? ip, bool calcularPrevia)
    {
        await using var cn = await _conexao.AbrirAsync();
        var lote = await cn.QuerySingleOrDefaultAsync<LoteResumoDto>(SelectLote + " WHERE id = @l AND id_instituicao = @i", new { l = idLote, i = idInstituicao });
        if (lote is null) return null;
        if (!string.IsNullOrWhiteSpace(usuarioQueVisualizou))
            await AuditarAsync(cn, null, idLote, "VISUALIZADO", usuarioQueVisualizou, "USUARIO", ip, null);

        var detalhe = new LoteDetalheDto
        {
            Lote = lote,
            ConteudoJson = await cn.ExecuteScalarAsync<string>("SELECT conteudo_json FROM hprof_uni_integracaolote WHERE id = @l", new { l = idLote }) ?? "",
            Mensagens = (await cn.QueryAsync<LoteMensagemDto>(
                "SELECT secao AS Secao, item AS Item, campo AS Campo, mensagem AS Mensagem, severidade AS Severidade FROM hprof_uni_integracaoerro WHERE id_lote = @l ORDER BY severidade, secao, item",
                new { l = idLote })).ToList(),
            Auditoria = (await cn.QueryAsync<AuditoriaDto>(
                "SELECT acao AS Acao, usuario AS Usuario, tipo_ator AS TipoAtor, ip AS Ip, ocorrido_em AS OcorridoEm, detalhe AS Detalhe FROM hprof_uni_integracaoauditoria WHERE id_lote = @l ORDER BY ocorrido_em, id",
                new { l = idLote })).ToList(),
            Alteracoes = (await cn.QueryAsync<AlteracaoDto>(
                "SELECT tabela AS Tabela, id_registro AS IdRegistro, operacao AS Operacao, CAST(valor_antes AS CHAR) AS ValorAntes, CAST(valor_depois AS CHAR) AS ValorDepois, gravado_em AS GravadoEm FROM hprof_uni_integracaoalteracao WHERE id_lote = @l ORDER BY id LIMIT 1000",
                new { l = idLote })).ToList()
        };

        if (calcularPrevia && lote.Status == StatusLote.Recebido)
        {
            // ainda não conferido (ex.: chegou antes dos professores): confere sem gravar a conferência
            var conferencia = lote.Tipo == TiposLote.Pessoas
                ? await _validador.ValidarPessoasAsync(cn, idInstituicao, JsonSerializer.Deserialize<LotePessoasConteudo>(detalhe.ConteudoJson, Json)!)
                : await _validador.ValidarCursoAsync(cn, idInstituicao, JsonSerializer.Deserialize<LoteCursoConteudo>(detalhe.ConteudoJson, Json)!);
            detalhe.Mensagens.AddRange(conferencia);
            if (conferencia.Any(x => x.Severidade == "ERRO")) calcularPrevia = false;
        }
        if (calcularPrevia && lote.Status is StatusLote.Validado or StatusLote.Recebido)
        {
            // uma prévia ou gravação por envio de cada vez; se já houver uma em andamento, não espera
            if (!await TravarAsync(cn, idLote, 0))
            {
                detalhe.Mensagens.Add(new LoteMensagemDto
                {
                    Secao = "previa", Severidade = "AVISO",
                    Mensagem = "A prévia não foi calculada agora porque este envio está sendo conferido ou gravado neste momento. Atualize a página em alguns instantes."
                });
                return detalhe;
            }
            try
            {
                // prévia = a própria gravação, dentro de uma transação que é desfeita no fim
                await using var tx = await cn.BeginTransactionAsync();
                try
                {
                    var g = new Gravacao(cn, tx, idLote, idInstituicao, "previa");
                    detalhe.Previa = await GravarAsync(g, lote.Tipo, detalhe.ConteudoJson);
                    detalhe.Mensagens.AddRange(g.Mensagens);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao calcular a prévia do lote {Lote}", idLote);
                    detalhe.Mensagens.Add(new LoteMensagemDto { Secao = "previa", Severidade = "ERRO", Mensagem = $"Não foi possível calcular a prévia: {ex.Message}" });
                }
                finally
                {
                    await tx.RollbackAsync();
                }
            }
            finally
            {
                await LiberarAsync(cn, idLote);
            }
        }
        return detalhe;
    }

    public async Task<ReceberLoteResposta> AprovarAsync(long idInstituicao, long idLote, string usuario, string? ip)
    {
        if (string.IsNullOrWhiteSpace(usuario)) return Falha("Informe o usuário que está aprovando.");
        await using var cn = await _conexao.AbrirAsync();
        // espera uma prévia em andamento do mesmo envio terminar, em vez de disputar o banco com ela
        if (!await TravarAsync(cn, idLote, 240))
            return Falha("Há uma prévia ou gravação deste envio em andamento. Aguarde um pouco e tente de novo.");
        try
        {
            return await AprovarTravadoAsync(cn, idInstituicao, idLote, usuario, ip);
        }
        finally
        {
            await LiberarAsync(cn, idLote);
        }
    }

    private async Task<ReceberLoteResposta> AprovarTravadoAsync(MySqlConnection cn, long idInstituicao, long idLote, string usuario, string? ip)
    {
        var lote = await cn.QuerySingleOrDefaultAsync<LoteResumoDto>(SelectLote + " WHERE id = @l AND id_instituicao = @i", new { l = idLote, i = idInstituicao });
        if (lote is null) return Falha("Envio não encontrado.");
        if (lote.Status is not (StatusLote.Validado or StatusLote.Recebido or StatusLote.ComErros))
            return Falha($"Este envio não pode ser aprovado. Situação atual: {lote.Status}.");

        var conteudo = await cn.ExecuteScalarAsync<string>("SELECT conteudo_json FROM hprof_uni_integracaolote WHERE id = @l", new { l = idLote }) ?? "";
        ReceberLoteResposta revalidacao;
        if (lote.Status == StatusLote.Validado)
            revalidacao = await ValidarAsync(cn, idInstituicao, idLote, lote.Tipo, conteudo, primeiraValidacao: false); // o banco pode ter mudado
        else
        {
            // envio que chegou antes dos dados de que depende (ex.: curso antes dos professores): confere agora
            await cn.ExecuteAsync("DELETE FROM hprof_uni_integracaoerro WHERE id_lote = @l", new { l = idLote }); // só as mensagens da conferência anterior deste envio
            revalidacao = await ValidarAsync(cn, idInstituicao, idLote, lote.Tipo, conteudo, primeiraValidacao: true);
        }
        if (revalidacao.Status != StatusLote.Validado) return revalidacao;

        await AuditarAsync(cn, null, idLote, "APROVADO", usuario, "USUARIO", ip, null);
        await using var tx = await cn.BeginTransactionAsync();
        PreviaDto resultado;
        List<LoteMensagemDto> avisos;
        try
        {
            // trava o envio: se duas pessoas aprovarem ao mesmo tempo, só a primeira grava
            var travado = await cn.ExecuteAsync(
                "UPDATE hprof_uni_integracaolote SET status = 'APLICADO', aprovado_por = @u, aprovado_em = NOW(3), aplicado_em = NOW(3) WHERE id = @l AND status = 'VALIDADO'",
                new { u = usuario, l = idLote }, tx);
            if (travado == 0)
            {
                await tx.RollbackAsync();
                return Falha("Este envio já foi gravado ou mudou de situação. Atualize a página.");
            }
            var g = new Gravacao(cn, tx, idLote, idInstituicao, usuario);
            resultado = await GravarAsync(g, lote.Tipo, conteudo);
            avisos = g.Mensagens;
            await AuditarAsync(cn, tx, idLote, "APLICADO", usuario, "USUARIO", ip,
                Cortar(string.Join("; ", resultado.Linhas.Where(x => x.Novos + x.Atualizados + x.Desativados > 0)
                    .Select(x => $"{x.Onde}: {x.Novos} novo(s), {x.Atualizados} alterado(s), {x.Desativados} desativado(s)")), 1000));
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Falha ao gravar o lote {Lote}", idLote);
            await AuditarAsync(cn, null, idLote, "FALHOU", usuario, "USUARIO", ip, Cortar(ex.Message, 1000));
            return Falha($"A gravação foi desfeita por um erro: {ex.Message}");
        }

        foreach (var aviso in avisos)
        {
            await cn.ExecuteAsync(
                "INSERT INTO hprof_uni_integracaoerro (id_lote, secao, item, campo, mensagem, severidade) VALUES (@l, @s, @i, @c, @m, 'AVISO')",
                new { l = idLote, s = Cortar(aviso.Secao, 40), i = aviso.Item, c = aviso.Campo is null ? null : Cortar(aviso.Campo, 60), m = Cortar(aviso.Mensagem, 500) });
        }
        return new ReceberLoteResposta
        {
            IdLote = idLote, CodigoEnvio = lote.CodigoEnvio, Status = StatusLote.Aplicado, TotalAvisos = revalidacao.TotalAvisos + avisos.Count,
            Mensagem = $"Envio aprovado e gravado: {resultado.TotalGravacoes} registro(s).", Mensagens = revalidacao.Mensagens.Concat(avisos).ToList()
        };
    }

    public async Task<ReceberLoteResposta> RejeitarAsync(long idInstituicao, long idLote, string usuario, string? motivo, string? ip)
    {
        if (string.IsNullOrWhiteSpace(usuario)) return Falha("Informe o usuário.");
        await using var cn = await _conexao.AbrirAsync();
        var linhas = await cn.ExecuteAsync(
            "UPDATE hprof_uni_integracaolote SET status = 'REJEITADO', motivo_rejeicao = @m, aprovado_por = @u, aprovado_em = NOW(3) WHERE id = @l AND id_instituicao = @i AND status IN ('RECEBIDO','VALIDADO','COM_ERROS')",
            new { m = Cortar(motivo ?? "", 500), u = usuario, l = idLote, i = idInstituicao });
        if (linhas == 0) return Falha("Envio não encontrado ou já gravado.");
        await AuditarAsync(cn, null, idLote, "REJEITADO", usuario, "USUARIO", ip, motivo);
        return new ReceberLoteResposta { IdLote = idLote, Status = StatusLote.Rejeitado, Mensagem = "Envio rejeitado. Nada foi gravado." };
    }

    private async Task<PreviaDto> GravarAsync(Gravacao g, string tipo, string conteudo) =>
        tipo == TiposLote.Pessoas
            ? await _aplicador.AplicarPessoasAsync(g, JsonSerializer.Deserialize<LotePessoasConteudo>(conteudo, Json)!)
            : await _aplicador.AplicarCursoAsync(g, JsonSerializer.Deserialize<LoteCursoConteudo>(conteudo, Json)!);

    /// <summary>Trava com nome do MySQL (GET_LOCK) por envio; vale para a conexão e é solta no fim.</summary>
    private static async Task<bool> TravarAsync(MySqlConnection cn, long idLote, int esperaSegundos) =>
        await cn.ExecuteScalarAsync<long?>("SELECT GET_LOCK(@n, @t)", new { n = $"hprof_uni_lote_{idLote}", t = esperaSegundos }) == 1;

    private static async Task LiberarAsync(MySqlConnection cn, long idLote)
    {
        try { await cn.ExecuteScalarAsync<long?>("SELECT RELEASE_LOCK(@n)", new { n = $"hprof_uni_lote_{idLote}" }); }
        catch { /* a trava some sozinha quando a conexão fecha */ }
    }

    private static Task AuditarAsync(MySqlConnection cn, MySqlTransaction? tx, long idLote, string acao, string usuario, string ator, string? ip, string? detalhe) =>
        cn.ExecuteAsync(
            "INSERT INTO hprof_uni_integracaoauditoria (id_lote, acao, usuario, tipo_ator, ip, detalhe) VALUES (@l, @a, @u, @t, @ip, @d)",
            new { l = idLote, a = acao, u = Cortar(usuario, 100), t = ator, ip, d = detalhe is null ? null : Cortar(detalhe, 1000) }, tx);

    private static string Cortar(string texto, int max) => texto.Length <= max ? texto : texto[..max];

    private static ReceberLoteResposta Falha(string mensagem) => new() { Status = "FALHA", Mensagem = mensagem, TotalErros = 1 };

    private sealed class LoteExistente
    {
        public long Id { get; set; }
        public string Hash { get; set; } = "";
        public string Status { get; set; } = "";
    }

    private const string SelectLote = """
        SELECT id AS Id, codigo_envio AS CodigoEnvio, tipo AS Tipo, origem AS Origem, status AS Status,
               ano_letivo AS AnoLetivo, periodo AS Periodo, total_itens AS TotalItens, total_erros AS TotalErros,
               total_avisos AS TotalAvisos, enviado_por AS EnviadoPor, recebido_em AS RecebidoEm, recebido_de_ip AS RecebidoDeIp,
               aprovado_por AS AprovadoPor, aprovado_em AS AprovadoEm, aplicado_em AS AplicadoEm, motivo_rejeicao AS MotivoRejeicao
          FROM hprof_uni_integracaolote
        """;
}
