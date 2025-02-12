using API_HorarioProfessor.Services.CoreServices;
using System.Diagnostics;

namespace API_HorarioProfessor.Services.CoreServices.RelatorioGradeTurmas
{
    public class RelatorioInversoProfessorNoTitulo
    {
        private readonly AppState _appState;
        public RelatorioInversoProfessorNoTitulo(AppState appState)
        {
            _appState = appState; // Injetando o AppState 
        }

        // Mapeamento de matérias para cores
        private Dictionary<string, string> materiaParaCor = new Dictionary<string, string>
        {
            {"INF", "#FF6347"}, // Tomato
            {"CIE", "#4682B4"}, // SteelBlue
            {"HIS", "#6A5ACD"}, // SlateBlue
            {"ING", "#32CD32"}, // LimeGreen
            {"GEO", "#FFD700"}, // Gold
            {"EDU", "#FF4500"}, // OrangeRed
            {"MAT", "#FF69B4"}, // HotPink
            {"POR", "#1E90FF"}, // DodgerBlue
            {"ART", "#8A2BE2"}, // BlueViolet
            {"LEI", "#20B2AA"}  // LightSeaGreen
        };

        public void RelatorioInversoProfessorTitulo(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("<html><head><style>table { width: 100%; border-collapse: collapse; } th, td { border: 1px solid black; padding: 2px; text-align: left; font-size: 10pt; }</style></head><body>");
                writer.WriteLine("<h1>Horário por Professor</h1>");  // Adicionando o título aqui

                // Gerar novo formato de relatório para cada dia da semana
                GerarNovoRelatorioDia(writer, "Segunda-feira", _appState.matrizHorariosSegunda);
                GerarNovoRelatorioDia(writer, "Terça-feira", _appState.matrizHorariosTerca);
                GerarNovoRelatorioDia(writer, "Quarta-feira", _appState.matrizHorariosQuarta);
                GerarNovoRelatorioDia(writer, "Quinta-feira", _appState.matrizHorariosQuinta);
                GerarNovoRelatorioDia(writer, "Sexta-feira", _appState.matrizHorariosSexta);

                writer.WriteLine("</body></html>");
            }
        }

        public void GerarNovoRelatorioDia(StreamWriter writer, string nomeDia, string[,] matrizHorarios)
        {
            var professoresUnicos = new HashSet<string>();
            for (int i = 0; i < matrizHorarios.GetLength(0); i++)
            {
                for (int j = 0; j < matrizHorarios.GetLength(1); j++)
                {
                    professoresUnicos.Add(matrizHorarios[i, j]);
                }
            }
            professoresUnicos.RemoveWhere(p => string.IsNullOrEmpty(p));

            // Ordenar professores alfabeticamente
            var professoresOrdenados = professoresUnicos.OrderBy(p => p).ToList();

            writer.WriteLine($"<h2>{nomeDia}</h2>");
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Horário/Professor</th>");
            foreach (var professor in professoresOrdenados)
            {
                string cor = ObterCorProfessor(professor); // Obter cor baseada na matéria principal
                writer.Write($"<th style='background-color:{cor};'>{professor}</th>");
            }
            writer.WriteLine("</tr>");

            for (int i = 0; i < matrizHorarios.GetLength(0); i++)
            {
                writer.WriteLine("<tr>");
                writer.Write($"<td>{i + 1}</td>");
                foreach (var professor in professoresOrdenados)
                {
                    writer.Write("<td>");
                    for (int j = 0; j < matrizHorarios.GetLength(1); j++)
                    {
                        if (matrizHorarios[i, j] == professor)
                        {
                            writer.Write($"{_appState.Turmas[j]} ");
                        }
                    }
                    writer.Write("</td>");
                }
                writer.WriteLine("</tr>");
            }

            writer.WriteLine("</table>");
        }

        public string ObterCorProfessor(string nomeProfessor)
        {
            if (!string.IsNullOrEmpty(nomeProfessor) && _appState.Professores.TryGetValue(nomeProfessor.ToUpper(), out var dadosProfessor))
            {
                var materias = dadosProfessor.DistribuicaoTurmas.Keys;
                if (materias.Any())
                {
                    string materiaPrincipal = materias.First(); // Assume o primeiro como principal ou ajuste conforme necessário
                    if (materiaParaCor.TryGetValue(materiaPrincipal, out var cor))
                    {
                        return cor;
                    }
                }
            }
            return "#DDDDDD"; // Cor padrão se não encontrar
        }

        public void AbrirNoNavegador(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Não foi possível abrir o navegador: " + e.Message);
            }



        }



    }
}
