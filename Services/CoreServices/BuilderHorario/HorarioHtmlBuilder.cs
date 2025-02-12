using System.Text;

namespace API_HorarioProfessor.Services.CoreServices.BuilderHorario
{
    public class HorarioHtmlBuilder
    {
        private readonly AppState _appState;

        public HorarioHtmlBuilder(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }
        public  string BuildHtml()
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Horários dos Professores</title>");
            html.AppendLine("    <link href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css' rel='stylesheet'>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container mt-5'>");
            html.AppendLine("        <h1 class='mb-4'>Horários dos Professores</h1>");

            // Processa cada professor
            foreach (var professor in _appState.Professores)
            {
                html.AppendLine("        <div class='card mb-3'>");
                html.AppendLine($"           <div class='card-header'>Professor: {professor.Key}</div>");
                html.AppendLine("            <div class='card-body'>");
                html.AppendLine("                <table class='table'>");
                html.AppendLine("                    <thead>");
                html.AppendLine("                        <tr>");
                html.AppendLine("                            <th>Dia da Semana</th>");
                html.AppendLine("                            <th>Horários</th>");
                html.AppendLine("                        </tr>");
                html.AppendLine("                    </thead>");
                html.AppendLine("                    <tbody>");

                // Adicionando dados das matrizes de cada dia
                string[][,] dias = { _appState.matrizHorariosSegunda, _appState.matrizHorariosTerca, _appState.matrizHorariosQuarta, _appState.matrizHorariosQuinta, _appState.matrizHorariosSexta };
                string[] nomesDias = { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };

                for (int dia = 0; dia < dias.Length; dia++)
                {
                    var horarios = new List<string>();
                    for (int i = 0; i < dias[dia].GetLength(0); i++)
                    {
                        for (int j = 0; j < dias[dia].GetLength(1); j++)
                        {
                            if (dias[dia][i, j] == professor.Key)
                                horarios.Add(_appState.Turmas[j]);
                        }
                    }

                    html.AppendLine("                        <tr>");
                    html.AppendLine($"                           <td>{nomesDias[dia]}</td>");
                    html.AppendLine($"                           <td>{string.Join(", ", horarios)}</td>");
                    html.AppendLine("                        </tr>");
                }

                html.AppendLine("                    </tbody>");
                html.AppendLine("                </table>");
                html.AppendLine("            </div>");
                html.AppendLine("        </div>");
            }

            // Formulário de opções
            html.AppendLine("        <h2>Opções do Professor</h2>");
            html.AppendLine("        <form>");
            html.AppendLine("            <div class='form-group'>");
            html.AppendLine("                <label for='nomeProfessor'>Nome do Professor</label>");
            html.AppendLine("                <input type='text' class='form-control' id='nomeProfessor' name='nomeProfessor' placeholder='Digite o nome do professor'>");
            html.AppendLine("            </div>");
            html.AppendLine("            <button type='submit' class='btn btn-primary'>Salvar Opções</button>");
            html.AppendLine("        </form>");
            html.AppendLine("    </div>");

            html.AppendLine("    <script src='https://code.jquery.com/jquery-3.5.1.slim.min.js'></script>");
            html.AppendLine("    <script src='https://cdn.jsdelivr.net/npm/@popperjs/core@2.5.2/dist/umd/popper.min.js'></script>");
            html.AppendLine("    <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js'></script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

    }
}
