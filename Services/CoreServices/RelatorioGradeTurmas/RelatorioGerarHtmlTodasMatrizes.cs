using API_HorarioProfessor.Services.CoreServices;
using System.Diagnostics;

namespace API_HorarioProfessor.Services.CoreServices.RelatorioGradeTurmas
{
    public class RelatorioGerarHtmlTodasMatrizes
    {
        private readonly AppState _appState;
        // Variáveis para ajustes de estilo
        private string fontSize = "8pt"; // Tamanho da fonte para as células
        private string headerFontSize = "9pt"; // Tamanho da fonte para os cabeçalhos dos dias
        private string cellPadding = "1px 3px"; // Padding dentro das células
        private string timeColWidth = "30px"; // Largura da coluna de horários

        public RelatorioGerarHtmlTodasMatrizes(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        public void RelatorioGerarHtmlTodasMatrize(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<html><head>");
                writer.WriteLine("<style>");
                writer.WriteLine($"table {{ width: 100%; border-collapse: collapse; }}");
                writer.WriteLine($"th, td {{ border: 1px solid black; padding: {cellPadding}; font-size: {fontSize}; text-align: left; }}");
                writer.WriteLine($"th {{ font-weight: bold; }}");
                writer.WriteLine($"h2 {{ font-size: {headerFontSize}; font-weight: bold; margin: 5px 0; }}");
                writer.WriteLine($".time-col {{ width: {timeColWidth}; }}");
                writer.WriteLine("</style>");
                writer.WriteLine("</head><body>");

                // Gerar tabelas para cada dia da semana
                GerarTabelaDia(writer, "Segunda-feira", _appState.matrizHorariosSegunda);
                GerarTabelaDia(writer, "Terça-feira", _appState.matrizHorariosTerca);
                GerarTabelaDia(writer, "Quarta-feira", _appState.matrizHorariosQuarta);
                GerarTabelaDia(writer, "Quinta-feira", _appState.matrizHorariosQuinta);
                GerarTabelaDia(writer, "Sexta-feira", _appState.matrizHorariosSexta);

                writer.WriteLine("</body></html>");
            }
        }

        public void AbrirNoNavegador(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true // Necessário para abrir o arquivo no navegador padrão
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Não foi possível abrir o navegador: " + e.Message);
            }
        }

        public void GerarTabelaDia(StreamWriter writer, string nomeDia, string[,] matrizHorarios)
        {
            Console.WriteLine($"Gerando tabela para {nomeDia}"); // Log para depuração
            writer.WriteLine($"<h2>{nomeDia}</h2>");
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th class='time-col'>Hora</th>");
            foreach (var turma in _appState.Turmas)
            {
                writer.Write($"<th>{turma}</th>");
            }
            writer.WriteLine("</tr>");

            for (int i = 0; i < matrizHorarios.GetLength(0); i++)
            {
                writer.WriteLine("<tr>");
                writer.Write($"<td class='time-col'>{i + 1}</td>");
                for (int j = 0; j < matrizHorarios.GetLength(1); j++)
                {
                    string professor = matrizHorarios[i, j];
                    string cor = ObterCorProfessor(professor);
                    writer.Write($"<td style='color:{cor}'>{professor}</td>");
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
        }

        public string ObterCorProfessor(string nomeProfessor)
        {
            if (!string.IsNullOrEmpty(nomeProfessor) && _appState.Professores.TryGetValue(nomeProfessor.ToUpper(), out var dadosProfessor))
            {
                foreach (var materia in dadosProfessor.DistribuicaoTurmas.Keys)
                {
                    switch (materia)
                    {
                        case "INF": return "#FF6347"; // Tomato
                        case "CIE": return "#4682B4"; // SteelBlue
                        case "HIS": return "#6A5ACD"; // SlateBlue
                        case "ING": return "#32CD32"; // LimeGreen
                        case "GEO": return "#FFD700"; // Gold
                        case "EDU": return "#FF4500"; // OrangeRed
                        case "MAT": return "#FF69B4"; // HotPink
                        case "POR": return "#1E90FF"; // DodgerBlue
                        case "ART": return "#8A2BE2"; // BlueViolet
                        case "LEI": return "#20B2AA"; // LightSeaGreen
                        default: return "#000000"; // Default to black if no match
                    }
                }
            }
            return "#000000"; // Return black if professor name is not found
        }
    }

}

