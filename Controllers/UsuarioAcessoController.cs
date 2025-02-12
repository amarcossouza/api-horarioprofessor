using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioAcessoController : ControllerBase
    {
        private readonly IUsuarioAcessoService _usuarioAcessoService;

        public UsuarioAcessoController(IUsuarioAcessoService usuarioAcessoService)
        {
            _usuarioAcessoService = usuarioAcessoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioAcesso>>> GetAll()
        {
            var usuariosAcesso = await _usuarioAcessoService.GetAllUsuariosAcessoAsync();
            return Ok(usuariosAcesso);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioAcesso>> Get(int id)
        {
            var usuarioAcesso = await _usuarioAcessoService.GetUsuarioAcessoByIdAsync(id);
            if (usuarioAcesso == null)
            {
                return NotFound();
            }
            return Ok(usuarioAcesso);
        }

        [HttpPost]
        public async Task<ActionResult> Create(UsuarioAcesso usuarioAcesso)
        {
            await _usuarioAcessoService.AddUsuarioAcessoAsync(usuarioAcesso);
            return CreatedAtAction(nameof(Get), new { id = usuarioAcesso.UsuarioAcessoId }, usuarioAcesso);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UsuarioAcesso usuarioAcesso)
        {
            if (id != usuarioAcesso.UsuarioAcessoId)
            {
                return BadRequest();
            }

            await _usuarioAcessoService.UpdateUsuarioAcessoAsync(usuarioAcesso);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioAcesso = await _usuarioAcessoService.GetUsuarioAcessoByIdAsync(id);
            if (usuarioAcesso == null)
            {
                return NotFound();
            }

            await _usuarioAcessoService.DeleteUsuarioAcessoAsync(id);
            return NoContent();
        }
    }
}
