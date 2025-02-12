using Microsoft.AspNetCore.Mvc;
using API_HorarioProfessor.Services;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PesquisaLogController : ControllerBase
    {
        private readonly LogService _logService;

        public PesquisaLogController(LogService logService)
        {
            _logService = logService;
        }

        [HttpPost("registrar")]
        public IActionResult RegistrarLog(int escolaId, string nomeEscola, int usuarioId, string usuarioLogin, string descricao, int quantidadePesquisas)
        {
            try
            {
                _logService.RegistrarLog(escolaId,nomeEscola, usuarioId, usuarioLogin, descricao, quantidadePesquisas);
                return Ok(new { Success = true, Message = "Log registrado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("QuantidadeConsultasPorEscolasDiarioOfical")]
        public IActionResult ContarConsultasTodasEscolas()
        {
            try
            {
                var logs = _logService.ContarConsultasTodasEscolas();
                return Ok(new { Success = true, Data = logs });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }


        [HttpGet("consultar")]
        public IActionResult ConsultarLogs(int escolaId)
        {
            try
            {
                var logs = _logService.ObterLogsPorEscola(escolaId);
                return Ok(new { Success = true, Data = logs });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
