using API_HorarioProfessor.Services.CoreServices;
using System.Net.NetworkInformation;

namespace API_HorarioProfessor.Services.CoreServices.Relatorios
{
    public class Relatorio
    {
        private readonly AppState _appState;

        public Relatorio(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        public void GerarRelatorioProfessores(string nomeDia, string[,] matriz)
        {
            Dictionary<string, int> contagemProfessores = new Dictionary<string, int>();
            int tamanhoColunaProfessor = 20;  // Tamanho para a coluna do nome do professor
            int tamanhoColunaAulas = 10;  // Tamanho para a coluna do total de aulas

            // Contar as ocorrências de cada professor na matriz
            for (int i = 0; i < matriz.GetLength(0); i++)
            {
                for (int j = 0; j < matriz.GetLength(1); j++)
                {
                    string nomeProfessor = matriz[i, j];
                    if (!string.IsNullOrWhiteSpace(nomeProfessor))
                    {
                        if (contagemProfessores.ContainsKey(nomeProfessor))
                        {
                            contagemProfessores[nomeProfessor]++;
                        }
                        else
                        {
                            contagemProfessores.Add(nomeProfessor, 1);
                        }
                    }
                }
            }

            // Imprimir o cabeçalho do relatório
            Console.WriteLine($"Relatório de Distribuição de Horários - {nomeDia}:");
            Console.WriteLine($"{"Professor".PadRight(tamanhoColunaProfessor)}{"Total de Aulas".PadRight(tamanhoColunaAulas)}");

            // Imprimir o relatório de professores e aulas
            foreach (var item in contagemProfessores)
            {
                Console.WriteLine($"{item.Key.PadRight(tamanhoColunaProfessor)}{item.Value.ToString().PadRight(tamanhoColunaAulas)}");
            }
            Console.WriteLine();
        }

        public void GerarRelatorioConsolidado(string[][,] matrizes, string[] diasDaSemana)
        {
            Dictionary<string, int> contagemProfessores = new Dictionary<string, int>();
            int tamanhoColunaProfessor = 20;  // Tamanho para a coluna do nome do professor
            int tamanhoColunaAulas = 10;      // Tamanho para a coluna do total de aulas

            // Processar cada matriz de horário
            for (int index = 0; index < matrizes.Length; index++)
            {
                string[,] matriz = matrizes[index];
                for (int i = 0; i < matriz.GetLength(0); i++)
                {
                    for (int j = 0; j < matriz.GetLength(1); j++)
                    {
                        string nomeProfessor = matriz[i, j];
                        if (!string.IsNullOrWhiteSpace(nomeProfessor))
                        {
                            if (contagemProfessores.ContainsKey(nomeProfessor))
                            {
                                contagemProfessores[nomeProfessor]++;
                            }
                            else
                            {
                                contagemProfessores.Add(nomeProfessor, 1);
                            }
                        }
                    }
                }
            }

            // Imprimir o cabeçalho do relatório consolidado
            Console.WriteLine("Relatório Consolidado de Distribuição de Horários:");
            Console.WriteLine($"{"Professor".PadRight(tamanhoColunaProfessor)}{"Total de Aulas".PadRight(tamanhoColunaAulas)}");

            // Imprimir o



        }

        public void GerarRelatorioDetalhado(string[][,] matrizes, string[] diasDaSemana)
        {
            Dictionary<string, int> contagemTotalProfessores = new Dictionary<string, int>();
            int tamanhoColunaProfessor = 20;  // Tamanho para a coluna do nome do professor
            int tamanhoColunaAulas = 10;      // Tamanho para a coluna do total de aulas

            // Processar cada matriz de horário correspondendo a cada dia da semana
            for (int index = 0; index < matrizes.Length; index++)
            {
                string[,] matriz = matrizes[index];
                Dictionary<string, int> contagemDiariaProfessores = new Dictionary<string, int>();

                // Contar as ocorrências de cada professor na matriz do dia específico
                for (int i = 0; i < matriz.GetLength(0); i++)
                {
                    for (int j = 0; j < matriz.GetLength(1); j++)
                    {
                        string nomeProfessor = matriz[i, j];
                        if (!string.IsNullOrWhiteSpace(nomeProfessor))
                        {
                            if (contagemDiariaProfessores.ContainsKey(nomeProfessor))
                            {
                                contagemDiariaProfessores[nomeProfessor]++;
                            }
                            else
                            {
                                contagemDiariaProfessores.Add(nomeProfessor, 1);
                            }

                            if (contagemTotalProfessores.ContainsKey(nomeProfessor))
                            {
                                contagemTotalProfessores[nomeProfessor]++;
                            }
                            else
                            {
                                contagemTotalProfessores.Add(nomeProfessor, 1);
                            }
                        }
                    }
                }

                // Imprimir o relatório diário
                Console.WriteLine($"Relatório de Distribuição de Horários - {diasDaSemana[index]}:");
                Console.WriteLine($"{"Professor".PadRight(tamanhoColunaProfessor)}{"Total de Aulas".PadRight(tamanhoColunaAulas)}");
                foreach (var item in contagemDiariaProfessores.OrderBy(x => x.Key))
                {
                    Console.WriteLine($"{item.Key.PadRight(tamanhoColunaProfessor)}{item.Value.ToString().PadRight(tamanhoColunaAulas)}");
                }
                Console.WriteLine();
            }

            // Imprimir o relatório consolidado
            Console.WriteLine("Relatório Consolidado de Distribuição de Horários para Toda a Semana:");
            Console.WriteLine($"{"Professor".PadRight(tamanhoColunaProfessor)}{"Total de Aulas".PadRight(tamanhoColunaAulas)}");
            int totalAulas = 0; // Variável para calcular o total de aulas
            foreach (var item in contagemTotalProfessores.OrderBy(x => x.Key))
            {
                Console.WriteLine($"{item.Key.PadRight(tamanhoColunaProfessor)}{item.Value.ToString().PadRight(tamanhoColunaAulas)}");
                totalAulas += item.Value;
            }
            // Mostrando o total de aulas
            Console.WriteLine($"{"Total Aulas".PadRight(tamanhoColunaProfessor)}{totalAulas.ToString().PadRight(tamanhoColunaAulas)}");
            Console.WriteLine();
        }

        public void GerarDistribuicaoHorariosCSV(string[] diasDaSemana, string[][,] matrizes, List<TurmaProf> turmas)
        {
            string filePath = "DistribuicaoHorarios.csv";
            using (StreamWriter file = new StreamWriter(filePath))
            {
                for (int dia = 0; dia < diasDaSemana.Length; dia++)
                {
                    string nomeDia = diasDaSemana[dia];
                    string[,] matriz = matrizes[dia];
                    file.WriteLine($"Matriz de Distribuição de Horários - {nomeDia}:");
                    file.Write(",");

                    // Usar o nome da turma da classe TurmaProf
                    foreach (TurmaProf turma in turmas)
                    {
                        file.Write($"\"{turma.Nome}\",");
                    }
                    file.WriteLine();

                    for (int i = 0; i < matriz.GetLength(0); i++)
                    {
                        file.Write($"{i + 1},");
                        for (int j = 0; j < matriz.GetLength(1); j++)
                        {
                            string nomeProfessor = matriz[i, j];
                            file.Write($"\"{nomeProfessor}\",");
                        }
                        file.WriteLine();
                    }
                    file.WriteLine();
                }
            }

            Console.WriteLine($"Arquivo '{filePath}' gerado com sucesso.");
        }

    }
}
