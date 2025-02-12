using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParametroDiarioOficialController : ControllerBase
    {
        private readonly IParametroDiarioOficialService _service;

        public ParametroDiarioOficialController(IParametroDiarioOficialService service)
        {
            _service = service;
        }

        [HttpGet("todos")]
        public async Task<ActionResult<IEnumerable<Models.ParametroDiarioOficial>>> ObterParametros()
        {
            var parametros = await _service.GetParametrosAsync();
            return Ok(parametros);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Models.ParametroDiarioOficial>> ObterParametro(int id)
        {
            var parametro = await _service.GetParametroByIdAsync(id);

            if (parametro == null)
            {
                return NotFound();
            }

            return Ok(parametro);
        }

        [HttpPost]
        public async Task<ActionResult<Models.ParametroDiarioOficial>> AdicionarParametro(Models.ParametroDiarioOficial parametro)
        {
            await _service.AddParametroAsync(parametro);
            return CreatedAtAction(nameof(ObterParametro), new { id = parametro.Codigo }, parametro);
        }

        [HttpDelete("id/{id}")]
        public async Task<IActionResult> DeletarParametro(int id)
        {
            var parametro = await _service.GetParametroByIdAsync(id);
            if (parametro == null)
            {
                return NotFound();
            }

            await _service.DeleteParametroAsync(id);
            return NoContent();
        }

        [HttpGet("testar-conexao")]
        public async Task<IActionResult> TestarConexao()
        {
            var resultado = await _service.TestarConexaoAsync();
            if (resultado == "Conexão bem-sucedida")
            {
                return Ok(resultado);
            }
            else
            {
                return StatusCode(500, resultado);
            }
        }

        [HttpGet("codigo-escola/{codigoEscola}")]
        public async Task<ActionResult<IEnumerable<Models.ParametroDiarioOficial>>> GetParametrosByCodigoEscola(int codigoEscola)
        {
            var parametros = await _service.GetParametrosByCodigoEscolaAsync(codigoEscola);
            if (parametros == null || !parametros.Any())
            {
                return NotFound();
            }
            return Ok(parametros);
        }

    }
}
