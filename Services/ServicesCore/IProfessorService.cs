using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services
{
    public interface IProfessorService
    {
        // Método para obter a lista de professores
        Task<List<Professor>> GetProfessoresAsync(int idEscola, string periodo, int ativo, int anoLetivo, int OrdenarMaiorNumeroOpcao1OuOpcao2);

        Task<List<OpcaoHorarioDto>> SelecionarOpcoesPorFiltros(int anoLetivo, string periodo, bool ativo, int idEscola, int idProfessor);

        Task<List<OpcaoHorarioDto>> SelecionarTodasOpcoesProfessor(int anoLetivo, string periodo, int ativo, int idEscola);

        //// Método para selecionar opções de horários por filtros
        //Task<List<OpcaoHorarioDto>> SelecionarOpcoesPorFiltros(int anoLetivo, string periodo, bool ativo, int idEscola, int IdProfessor);
    }

}
