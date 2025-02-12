using API_HorarioProfessor.Services.CoreServices;

namespace API_HorarioProfessor.Services.CoreServices.TrocaTresProfessores
{
    public class ProfessorProcessor
    {
        private readonly AppState _appState;
        public ProfessorProcessor(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }
        // Método para verificar se um professor pode ser alocado
        public bool PodeAlocarProfessor(string professor, string[,] matriz)
        {
            // Implementar a lógica de verificação se o professor pode ser alocado na matriz
            // Baseado nas regras que você mencionou
            // Exemplo simples de verificação:
            for (int i = 0; i < matriz.GetLength(0); i++)
            {
                for (int j = 0; j < matriz.GetLength(1); j++)
                {
                    if (matriz[i, j] == professor)
                    {
                        return false; // Professor já alocado na matriz
                    }
                }
            }
            return true;
        }

    }
}
