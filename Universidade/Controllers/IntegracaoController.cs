using System.Text.Json;
using API_HorarioProfessor.Universidade.Contratos;
using API_HorarioProfessor.Universidade.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace API_HorarioProfessor.Universidade.Controllers;

/// <summary>
/// Integração para o sistema acadêmico da universidade.
/// Autenticação pelo cabeçalho X-Api-Key (chave própria de cada instituição).
/// Todo envio fica guardado como chegou, é validado e espera aprovação do gestor
/// (a não ser que o cliente esteja configurado para gravação automática).
/// </summary>
[ApiController]
[Route("api/integracao/v1")]
public sealed class IntegracaoController : ControllerBase
{
    private readonly IIntegracaoService _integracao;
    private readonly IUniversidadeConsultaService _consulta;

    public IntegracaoController(IIntegracaoService integracao, IUniversidadeConsultaService consulta)
    {
        _integracao = integracao; _consulta = consulta;
    }

    private async Task<ClienteIntegracao?> ClienteAsync() =>
        await _integracao.AutenticarClienteAsync(Request.Headers["X-Api-Key"].FirstOrDefault());

    private static IActionResult NaoAutorizado() =>
        new UnauthorizedObjectResult(new { mensagem = "Chave de integração ausente ou inválida (cabeçalho X-Api-Key)." });

    /// <summary>Recebe professores (pessoa, matrícula e carga total) e, opcionalmente, a divisão da carga.</summary>
    [HttpPost("professores")]
    public Task<IActionResult> Professores([FromBody] JsonElement corpo) => ReceberAsync(TiposLote.Pessoas, corpo);

    /// <summary>Recebe um curso: horários, turmas, disciplinas, atribuições e indisponibilidades.</summary>
    [HttpPost("cursos")]
    public Task<IActionResult> Cursos([FromBody] JsonElement corpo) => ReceberAsync(TiposLote.Curso, corpo);

    private async Task<IActionResult> ReceberAsync(string tipo, JsonElement corpo)
    {
        var cliente = await ClienteAsync();
        if (cliente is null) return NaoAutorizado();
        var resposta = await _integracao.ReceberAsync(cliente.IdInstituicao, cliente.Id, OrigensLote.Api, tipo, corpo.GetRawText(),
            cliente.Sigla, HttpContext.Connection.RemoteIpAddress?.ToString());
        if (resposta.Status == "FALHA") return BadRequest(resposta);
        return resposta.JaRecebido ? Ok(resposta) : Accepted(resposta);
    }

    [HttpGet("lotes/{idLote:long}")]
    public async Task<IActionResult> Lote(long idLote)
    {
        var cliente = await ClienteAsync();
        if (cliente is null) return NaoAutorizado();
        var detalhe = await _integracao.DetalheAsync(cliente.IdInstituicao, idLote, null, null, calcularPrevia: false);
        if (detalhe is null) return NotFound(new { mensagem = "Envio não encontrado." });
        return Ok(new { detalhe.Lote, detalhe.Mensagens });
    }

    /// <summary>Grade publicada do curso, para o sistema da universidade.</summary>
    [HttpGet("grades/{codigoCurso}")]
    public async Task<IActionResult> Grade(string codigoCurso, [FromQuery] int anoLetivo, [FromQuery] string periodo = "Manha")
    {
        var cliente = await ClienteAsync();
        if (cliente is null) return NaoAutorizado();
        var curso = (await _consulta.ListarCursosAsync(cliente.IdInstituicao))
            .FirstOrDefault(c => string.Equals(c.Codigo, codigoCurso, StringComparison.OrdinalIgnoreCase));
        if (curso is null) return NotFound(new { mensagem = $"Curso {codigoCurso} não encontrado." });
        var grade = await _consulta.ObterGradeAsync(curso.IdEscola, anoLetivo, periodo, null);
        return grade.Publicada ? Ok(grade) : NotFound(new { mensagem = "O curso ainda não tem grade publicada." });
    }
}
