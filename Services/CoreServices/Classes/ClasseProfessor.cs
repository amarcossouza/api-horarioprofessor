using System.Net.NetworkInformation;

namespace API_HorarioProfessor.Services.CoreServices.Classes
{
    public class ClasseProfessor
    {
        private readonly AppState _appState;

        public int AulasPorTurma { get; set; }
        public Dictionary<string, List<string>> DistribuicaoTurmas { get; set; }
        public string DiasNaoTrabalha { get; set; }
        public string NomeAtividadeEscolar { get; set; }
        public bool DobrarAula { get; set; }
        public string Restricoes { get; set; }
        public string AtividadeAtpc { get; set; }
        public string DiasAtpc { get; set; }
        public bool Prioridade { get; set; }

        public ClasseProfessor(AppState appState)
        {
            _appState = appState; // Injetando o AppState
            DistribuicaoTurmas = new Dictionary<string, List<string>>();
        }
    }
}
