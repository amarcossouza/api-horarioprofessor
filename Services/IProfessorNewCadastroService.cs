using API_HorarioProfessor.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services
{
    public interface IProfessorNewCadastroService
    {
        Task<List<ProfessorNewCadastroDto>> ObterTodosProfessoresAsync(int idEscola, int Anoletivo, string Periodo, int OrdenarMaiorNumeroOpcao1OuOpcao2);
        Task<ProfessorNewCadastroDto> ObterProfessorPorIdAsync(int professorId);
        Task AdicionarProfessorAsync(ProfessorNewCadastroDto professor);
        Task AtualizarProfessorAsync(ProfessorNewCadastroDto professor);
        Task DeletarProfessorAsync(int professorId);
    }
}
