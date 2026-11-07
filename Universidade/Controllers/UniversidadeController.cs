using API_HorarioProfessor.Universidade.Contratos;
using API_HorarioProfessor.Universidade.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace API_HorarioProfessor.Universidade.Controllers;

/// <summary>
/// Endpoints da área Universidade usados pelo frontend (MVC).
/// Controller leve: valida a entrada e chama os serviços do módulo.
/// </summary>
[ApiController]
[Route("api/universidade")]
public sealed class UniversidadeController : ControllerBase
{
    private readonly IUniversidadeConsultaService _consulta;
    private readonly IUniversidadeCadastroService _cadastro;
    private readonly IIntegracaoService _integracao;
    private readonly IOcupacaoUniversidadeService _ocupacao;

    public UniversidadeController(IUniversidadeConsultaService consulta, IUniversidadeCadastroService cadastro,
        IIntegracaoService integracao, IOcupacaoUniversidadeService ocupacao)
    {
        _consulta = consulta; _cadastro = cadastro; _integracao = integracao; _ocupacao = ocupacao;
    }

    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();

    private static IActionResult? ConferirAno(int anoLetivo, string? periodo) =>
        anoLetivo is < 2000 or > 2100 || string.IsNullOrWhiteSpace(periodo)
            ? new BadRequestObjectResult(new { mensagem = "Informe ano letivo e período válidos." })
            : null;

    private IActionResult Resposta(ReceberLoteResposta r) =>
        r.Status == "FALHA" ? BadRequest(r) : Ok(r);

    // ---------- contexto e cursos ----------

    /// <summary>Instituição e cursos a partir da escola logada (escola da instituição ou de um curso).</summary>
    [HttpGet("contexto")]
    public async Task<IActionResult> Contexto([FromQuery] int idEscola)
    {
        if (idEscola <= 0) return BadRequest(new { mensagem = "Informe a escola." });
        var contexto = await _consulta.ObterContextoAsync(idEscola);
        return contexto is null ? NotFound(new { mensagem = "Esta escola não pertence a uma universidade." }) : Ok(contexto);
    }

    [HttpGet("{idInstituicao:long}/cursos")]
    public async Task<IActionResult> Cursos(long idInstituicao) => Ok(await _consulta.ListarCursosAsync(idInstituicao));

    // ---------- professores (pessoas) e carga ----------

    [HttpGet("{idInstituicao:long}/painel")]
    public async Task<IActionResult> Painel(long idInstituicao, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha") =>
        ConferirAno(anoLetivo, periodo) ?? Ok(await _consulta.ObterPainelAsync(idInstituicao, anoLetivo, periodo));

    [HttpGet("{idInstituicao:long}/pessoas")]
    public async Task<IActionResult> Pessoas(long idInstituicao) => Ok(await _consulta.ListarPessoasAsync(idInstituicao));

    [HttpGet("{idInstituicao:long}/pessoas/{idPessoa:long}")]
    public async Task<IActionResult> Pessoa(long idInstituicao, long idPessoa, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha")
    {
        var erro = ConferirAno(anoLetivo, periodo);
        if (erro is not null) return erro;
        var pessoa = await _consulta.ObterPessoaAsync(idInstituicao, idPessoa, anoLetivo, periodo);
        return pessoa is null ? NotFound(new { mensagem = "Professor não encontrado." }) : Ok(pessoa);
    }

    /// <summary>Cria ou altera a pessoa e a divisão da carga. Gera um envio de origem TELA, com auditoria.</summary>
    [HttpPost("{idInstituicao:long}/pessoas")]
    public async Task<IActionResult> SalvarPessoa(long idInstituicao, [FromBody] SalvarPessoaPedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.Nome) || string.IsNullOrWhiteSpace(pedido.Documento) && pedido.Id is null or 0)
            return BadRequest(new { mensagem = "Informe nome e matrícula." });
        if (string.IsNullOrWhiteSpace(pedido.Usuario)) return BadRequest(new { mensagem = "Informe o usuário." });
        return Resposta(await _cadastro.SalvarPessoaAsync(idInstituicao, pedido, Ip));
    }

    [HttpGet("{idInstituicao:long}/pessoas/{idPessoa:long}/semana")]
    public async Task<IActionResult> Semana(long idInstituicao, long idPessoa, [FromQuery] int anoLetivo)
    {
        var semana = await _consulta.ObterSemanaAsync(idInstituicao, idPessoa, anoLetivo);
        return semana is null ? NotFound(new { mensagem = "Professor não encontrado." }) : Ok(semana);
    }

    [HttpGet("{idInstituicao:long}/conferencia")]
    public async Task<IActionResult> Conferencia(long idInstituicao, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha") =>
        ConferirAno(anoLetivo, periodo) ?? Ok(await _consulta.ObterConferenciaAsync(idInstituicao, anoLetivo, periodo));

    // ---------- curso: atribuições, grades e publicação ----------

    [HttpGet("cursos/{idEscola:int}/atribuicoes")]
    public async Task<IActionResult> Atribuicoes(int idEscola, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha") =>
        ConferirAno(anoLetivo, periodo) ?? Ok(await _consulta.ListarAtribuicoesCursoAsync(idEscola, anoLetivo, periodo));

    [HttpGet("cursos/{idEscola:int}/turmas")]
    public async Task<IActionResult> Turmas(int idEscola) => Ok(await _consulta.ListarTurmasCursoAsync(idEscola));

    [HttpPost("cursos/{idEscola:int}/atribuicoes")]
    public async Task<IActionResult> IncluirAtribuicao(int idEscola, [FromBody] NovaAtribuicaoPedido pedido)
    {
        if (pedido.IdPessoa <= 0 || string.IsNullOrWhiteSpace(pedido.TurmaSigla) || string.IsNullOrWhiteSpace(pedido.Disciplina) || pedido.Aulas <= 0)
            return BadRequest(new { mensagem = "Informe professor, fase, disciplina e aulas." });
        if (string.IsNullOrWhiteSpace(pedido.Usuario)) return BadRequest(new { mensagem = "Informe o usuário." });
        return Resposta(await _cadastro.IncluirAtribuicaoAsync(idEscola, pedido, Ip));
    }

    [HttpGet("cursos/{idEscola:int}/grades-salvas")]
    public async Task<IActionResult> GradesSalvas(int idEscola, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha") =>
        ConferirAno(anoLetivo, periodo) ?? Ok(await _consulta.ListarGradesSalvasAsync(idEscola, anoLetivo, periodo));

    /// <summary>Grade do curso no formato da universidade (fase, horário do relógio, disciplina e professor).</summary>
    [HttpGet("cursos/{idEscola:int}/grade")]
    public async Task<IActionResult> Grade(int idEscola, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha", [FromQuery] int? idPlanilha = null) =>
        ConferirAno(anoLetivo, periodo) ?? Ok(await _consulta.ObterGradeAsync(idEscola, anoLetivo, periodo, idPlanilha));

    /// <summary>Publica uma grade salva do curso e grava a ocupação dos professores.</summary>
    [HttpPost("cursos/{idEscola:int}/publicar")]
    public async Task<IActionResult> Publicar(int idEscola, [FromBody] PublicarPedido pedido)
    {
        var erro = ConferirAno(pedido.AnoLetivo, pedido.Periodo);
        if (erro is not null) return erro;
        if (pedido.IdPlanilha <= 0) return BadRequest(new { mensagem = "Escolha a grade salva que será publicada." });
        var resultado = await _ocupacao.PublicarAsync(idEscola, pedido);
        return resultado.IdPublicacao > 0 ? Ok(resultado) : BadRequest(resultado);
    }

    /// <summary>Horários bloqueados para o motor neste curso (o que o AC automático vai usar).</summary>
    [HttpGet("cursos/{idEscola:int}/bloqueios")]
    public async Task<IActionResult> Bloqueios(int idEscola, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha") =>
        ConferirAno(anoLetivo, periodo) ?? Ok(await _ocupacao.ObterBloqueiosAsync(idEscola, anoLetivo, periodo));

    // ---------- planilha colada ----------

    [HttpPost("{idInstituicao:long}/planilha/ler")]
    public async Task<IActionResult> LerPlanilha(long idInstituicao, [FromBody] PlanilhaPedido pedido) =>
        Ok(await _cadastro.LerPlanilhaAsync(idInstituicao, pedido));

    [HttpPost("{idInstituicao:long}/planilha/enviar")]
    public async Task<IActionResult> EnviarPlanilha(long idInstituicao, [FromBody] PlanilhaPedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.Usuario)) return BadRequest(new { mensagem = "Informe o usuário." });
        return Resposta(await _cadastro.EnviarPlanilhaAsync(idInstituicao, pedido, Ip));
    }

    // ---------- envios: lista, prévia, aprovação e auditoria ----------

    [HttpGet("{idInstituicao:long}/lotes")]
    public async Task<IActionResult> Lotes(long idInstituicao, [FromQuery] string? status = null) =>
        Ok(await _integracao.ListarAsync(idInstituicao, status));

    [HttpGet("{idInstituicao:long}/lotes/{idLote:long}")]
    public async Task<IActionResult> Lote(long idInstituicao, long idLote, [FromQuery] string? usuario = null, [FromQuery] bool previa = true)
    {
        var detalhe = await _integracao.DetalheAsync(idInstituicao, idLote, usuario, Ip, previa);
        return detalhe is null ? NotFound(new { mensagem = "Envio não encontrado." }) : Ok(detalhe);
    }

    [HttpPost("{idInstituicao:long}/lotes/{idLote:long}/aprovar")]
    public async Task<IActionResult> Aprovar(long idInstituicao, long idLote, [FromBody] DecisaoLotePedido pedido) =>
        Resposta(await _integracao.AprovarAsync(idInstituicao, idLote, pedido.Usuario, Ip));

    [HttpPost("{idInstituicao:long}/lotes/{idLote:long}/rejeitar")]
    public async Task<IActionResult> Rejeitar(long idInstituicao, long idLote, [FromBody] DecisaoLotePedido pedido) =>
        Resposta(await _integracao.RejeitarAsync(idInstituicao, idLote, pedido.Usuario, pedido.Motivo, Ip));
}
