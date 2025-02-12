using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessorNewCadastroController : ControllerBase
    {
        private readonly IProfessorNewCadastroService _professorService;

        public ProfessorNewCadastroController(IProfessorNewCadastroService professorService)
        {
            _professorService = professorService;
        }

        // GET: api/ProfessorNewCadastro/todos
        [HttpGet("todos")]
        public async Task<IActionResult> ObterTodosProfessores(int idEscola, int Anoletivo, string Periodo)
        {
            int OrdenarMaiorNumeroOpcao1OuOpcao2 = 1;
            var professores = await _professorService.ObterTodosProfessoresAsync(idEscola, Anoletivo, Periodo, OrdenarMaiorNumeroOpcao1OuOpcao2);
            return Ok(professores);
        }

        // GET: api/ProfessorNewCadastro/{professorId}
        [HttpGet("{professorId}")]
        public async Task<IActionResult> ObterProfessorPorId(int professorId)
        {
            var professor = await _professorService.ObterProfessorPorIdAsync(professorId);
            if (professor == null)
                return NotFound();

            return Ok(professor);
        }

        // POST: api/ProfessorNewCadastro/adicionar
        [HttpPost("adicionar")]
        public async Task<IActionResult> AdicionarProfessor([FromBody] ProfessorNewCadastroDto professor)
        {
            await _professorService.AdicionarProfessorAsync(professor);
            return CreatedAtAction(nameof(ObterProfessorPorId), new { professorId = professor.ProfessorId }, professor);
        }

        // PUT: api/ProfessorNewCadastro/atualizar/{professorId}
        [HttpPut("atualizar/{professorId}")]
        public async Task<IActionResult> AtualizarProfessor(int professorId, [FromBody] ProfessorNewCadastroDto professor)
        {
            if (professorId != professor.ProfessorId)
                return BadRequest();

            await _professorService.AtualizarProfessorAsync(professor);
            return NoContent();
        }

        // DELETE: api/ProfessorNewCadastro/deletar/{professorId}
        [HttpDelete("deletar/{professorId}")]
        public async Task<IActionResult> DeletarProfessor(int professorId)
        {
            await _professorService.DeletarProfessorAsync(professorId);
            return NoContent();
        }
    }
}
