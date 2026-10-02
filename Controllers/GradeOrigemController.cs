using API_HorarioProfessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_HorarioProfessor.Controllers;

[ApiController]
[Route("api/grade-origem")]
public sealed class GradeOrigemController : ControllerBase
{
    private readonly IGradeOrigemSnapshotService _service;

    public GradeOrigemController(IGradeOrigemSnapshotService service) => _service = service;

    [HttpGet("snapshot")]
    public async Task<IActionResult> ObterSnapshot(
        [FromQuery] int idEscola,
        [FromQuery] int anoLetivo,
        [FromQuery] string periodo,
        [FromQuery] int quantidadeHorarios = 7)
    {
        if (idEscola <= 0 || anoLetivo <= 0 || string.IsNullOrWhiteSpace(periodo) || quantidadeHorarios <= 0)
            return BadRequest(new { mensagem = "Informe escola, ano letivo, período e quantidade de horários válidos." });

        return Ok(await _service.ObterAsync(idEscola, anoLetivo, periodo.Trim(), quantidadeHorarios));
    }
}
