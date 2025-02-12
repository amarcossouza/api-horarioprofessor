namespace API_HorarioProfessor.Services.CoreServices.Classes
{
    public class ValidadorDeAgendamento
    {
        private readonly AppState _appState;

        public ValidadorDeAgendamento(AppState appState)
        {
            _appState = appState;
        }

        public Dictionary<string, bool> ValidarAgendamento(string[,] matrizDia, int idxDiaSemanaMatriz, string nomeProfessor, int horario, int turmaIndex)
        {
            // Lista de métodos de validação a serem chamados
            var validadores = new Dictionary<string, Func<string[,], int, string, int, int, bool>>
        {
            { "PrioridadeProfessor", ValidarPrioridadeProfessor },
            { "RestricaoHorario", ValidarRestricaoHorario },
            { "DisponibilidadeTurma", ValidarDisponibilidadeTurma }
        };

            // Dicionário para armazenar os resultados das validações
            Dictionary<string, bool> resultados = new Dictionary<string, bool>();

            // Iterar sobre cada validador
            foreach (var validador in validadores)
            {
                bool resultado = validador.Value(matrizDia, idxDiaSemanaMatriz, nomeProfessor, horario, turmaIndex);
                resultados[validador.Key] = resultado;

                if (!resultado)
                {
                    break; // Para o loop se qualquer validador falhar
                }
            }

            return resultados;
        }

        private bool ValidarPrioridadeProfessor(string[,] matrizDia, int idxDiaSemanaMatriz, string nomeProfessor, int horario, int turmaIndex)
        {
            // Simulação de lógica
            // Console.WriteLine("Validando prioridade do professor...");
            return true; // Simula que a prioridade foi atendida
        }

        private bool ValidarRestricaoHorario(string[,] matrizDia, int idxDiaSemanaMatriz, string nomeProfessor, int horario, int turmaIndex)
        {
            // Reutiliza o método de validação de restrição de horário
            RestricaoHorario restricaoHorario = new RestricaoHorario(_appState);
            return restricaoHorario.ValidarHorario(matrizDia, idxDiaSemanaMatriz, nomeProfessor, horario, turmaIndex);
        }

        private bool ValidarDisponibilidadeTurma(string[,] matrizDia, int idxDiaSemanaMatriz, string nomeProfessor, int horario, int turmaIndex)
        {
            // Simulação de lógica
            // Console.WriteLine("Validando disponibilidade da turma...");
            return true; // Simula que a turma está disponível
        }
    }
}
