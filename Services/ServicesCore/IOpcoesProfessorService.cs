using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services.ServicesCore
{
    public interface IOpcoesProfessorService
    {
        // novo metodo busca as opcoes utilizada no processamento - TABELA opcoesProcessamento

        // este metodo pesquisa a tabela opcoesHorario / este guarda as opcoes do professor
        Task<IEnumerable<OpcaoHorarioDto>> SelecionarOpcoesPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola);
       
        // este metodo pesquisa tabela OpcoesHorarioProcessamento usado no processamento aqui
        Task<IEnumerable<OpcaoHorarioDto>> SelecionarOpcoesProcessamentoPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola);

        
        // Novo método para apagar as opções de acordo com os filtros fornecidos
        Task<int> ApagarOpcoesPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola, int AcumuloCargo);

        // Novo método para copiar dados da tabela OpcaoHorario para OpcaoHorarioProcessamento
        Task<int> CopiarOpcoesParaProcessamentoAsync(int anoLetivo, string periodo, int ativo, int idEscola, int opcao);

        // Novo método para copiar opções excluindo uma lista de professores
        Task<int> CopiarOpcoesParaProcessamentoComExclusaoAsync(int anoLetivo, string periodo, int ativo, int idEscola, int opcao, List<int> listaProfessoresExcluidos, string filtroProfessores);

        // Novo método para apagar opções com base em filtros e lista de professores
        Task<int> ApagarOpcoesPorFiltrosComProfessoresAsync(int anoLetivo, string periodo, int ativo, int idEscola, List<int> listaProfessores);
    }
}
