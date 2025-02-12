namespace API_HorarioProfessor.Services.CoreServices.Classes
{
    public class RestricaoHorario
    {
        private readonly AppState _appState;

        public RestricaoHorario(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        public  bool ValidarHorario(string[,] matrizDia, int idxDiaSemanaMatriz, string nomeProfessor, int horario, int turmaIndex)
        {
            if (_appState.Professores.TryGetValue(nomeProfessor, out var infoProfessor))
            {
                string diasNaoTrabalha = infoProfessor.DiasNaoTrabalha;
                if (string.IsNullOrEmpty(diasNaoTrabalha))
                {
                    return true; // Sem restrições, pode alocar
                }

                if (nomeProfessor == "LUIS")
                {
                    string achei = "LUIS";
                }
                // Lista de restrições no formato DIA#HORÁRIO, por exemplo, "SEG2" para Segunda-feira, 2ª aula
                var restricoes = diasNaoTrabalha.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string diaDaSemana = _appState.diasDaSemana[idxDiaSemanaMatriz];
                string aula = $"{diaDaSemana.Substring(0, 3).ToUpper()}{horario + 1}";

                if (restricoes.Contains(aula))
                {
                    Console.WriteLine($"Professor {nomeProfessor} não trabalha na aula {horario + 1} em {diaDaSemana}.");
                    return false; // Restrição encontrada, não pode alocar
                }
                return true; // Nenhuma restrição encontrada, pode alocar
            }

            Console.WriteLine($"Professor {nomeProfessor} não encontrado.");
            return false; // Professor não encontrado, não pode alocar
        }

    }
}
