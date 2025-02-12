using System.Text;

namespace API_HorarioProfessor.Services.CoreServices.FrontEnd.OpcoesProfessor
{
    public class HorarioHtmlGenerator
    {
        private readonly AppState _appState;
        public HorarioHtmlGenerator(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }
        public string GenerateHtml()
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='pt'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Opção de Horário - 2024</title>");
            html.AppendLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container mt-5'>");
            html.AppendLine("        <h1>Opção de Horário - 2024</h1>");
            html.AppendLine("        <div id='professorInfo'>");
            html.AppendLine("            <label for='nomeProfessor'>Nome do Professor:</label>");
            html.AppendLine("            <select class='form-control mb-2' id='nomeProfessor'></select>");
            html.AppendLine("            <label for='disciplinaProfessor'>Disciplina:</label>");
            html.AppendLine("            <select class='form-control mb-2' id='disciplinaProfessor'></select>");
            html.AppendLine("            <label for='numeroAulas'>Número de Aulas:</label>");
            html.AppendLine("            <input type='number' class='form-control mb-2' id='numeroAulas' placeholder='Número de Aulas'>");
            html.AppendLine("            <label for='turmasAtribuidas'>Turmas Atribuídas:</label>");
            html.AppendLine("            <select class='form-control mb-2' id='turmasAtribuidas'></select>");
            html.AppendLine("            <label for='periodoProfessor'>Período:</label>");
            html.AppendLine("            <select class='form-control mb-2' id='periodoProfessor'>");
            html.AppendLine("                <option value=''>Selecione o Período</option>");
            html.AppendLine("                <option value='manha'>Manhã</option>");
            html.AppendLine("                <option value='tarde'>Tarde</option>");
            html.AppendLine("                <option value='noite'>Noite</option>");
            html.AppendLine("            </select>");
            html.AppendLine("        </div>");
            html.AppendLine("        <button class='btn btn-success mb-3' onclick='salvarOpcoes()'>Salvar</button>");
            html.AppendLine("        <button class='btn btn-danger mb-3' onclick='excluirOpcoes()'>Excluir</button>");
            html.AppendLine("        <h2>OPÇÃO 1</h2>");
            html.AppendLine(CreateHorarioGrid("opcao1", 6));
            html.AppendLine("        <h2>OPÇÃO 2</h2>");
            html.AppendLine(CreateHorarioGrid("opcao2", 6));
            html.AppendLine("    </div>");
            AddJavaScript(html);
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private void AddJavaScript(StringBuilder html)
        {
            html.AppendLine("<script src='https://code.jquery.com/jquery-3.6.0.min.js'></script>");
            html.AppendLine("<script>");
            // Adiciona JavaScript para carregar dados nos comboboxes
            html.AppendLine("    $(document).ready(function() {");
            html.AppendLine("        var professores = ['ELENICE', 'SAMIR', 'RANIERE', 'AMANDA', 'LUIS'].sort();");
            html.AppendLine("        professores.unshift(''); // Adiciona uma opção em branco");
            html.AppendLine("        professores.forEach(function(prof) {");
            html.AppendLine("            $('#nomeProfessor').append(`<option value=\"${prof}\">${prof}</option>`);");
            html.AppendLine("        });");
            html.AppendLine("        $('#disciplinaProfessor').append('<option value=\"\"> </option>'); // Opção em branco");
            html.AppendLine("        ['HIS', 'GEO', 'MAT', 'FIS', 'EDU', 'FIL', 'POR'].sort().forEach(function(disciplina) {");
            html.AppendLine("            $('#disciplinaProfessor').append(`<option value=\"${disciplina}\">${disciplina}</option>`);");
            html.AppendLine("        });");
            html.AppendLine("        $('#turmasAtribuidas').append('<option value=\"\"> </option>');  // Opção em branco");
            html.AppendLine("        ['6A', '6B', '7A', '7B', '8A', '8B'].sort().forEach(function(turma) {");
            html.AppendLine("            $('#turmasAtribuidas').append(`<option value=\"${turma}\">${turma}</option>`);");
            html.AppendLine("        });");
            html.AppendLine("    });");
            html.AppendLine("    function excluirOpcoes() {");
            html.AppendLine("        // Implementar lógica de exclusão conforme necessário");
            html.AppendLine("        alert('Opções excluídas!'); // Placeholder");
            html.AppendLine("    }");
            html.AppendLine("</script>");
        }

        private string CreateHorarioGrid(string gridId, int numeroDeLinhas)
        {
            StringBuilder grid = new StringBuilder();
            grid.AppendLine($"<table class='table table-bordered' id='{gridId}'>");
            grid.AppendLine("<thead><tr><th>Aulas</th><th>Segunda</th><th>Terça</th><th>Quarta</th><th>Quinta</th><th>Sexta</th></tr></thead>");
            grid.AppendLine("<tbody>");
            for (int i = 1; i <= numeroDeLinhas; i++)
            {
                grid.AppendLine($"<tr><td>{i}</td>");
                for (int j = 0; j < 5; j++) // Para cada dia da semana
                {
                    grid.AppendLine("<td>");
                    grid.AppendLine("<select class='form-control'>");
                    grid.AppendLine("<option value=''></option>");  // Opção vazia
                    grid.AppendLine("<option value='HL'>HL</option>");
                    grid.AppendLine("<option value='HA'>HA</option>");
                    grid.AppendLine("</select>");
                    grid.AppendLine("</td>");
                }
                grid.AppendLine("</tr>");
            }
            grid.AppendLine("</tbody>");
            grid.AppendLine("</table>");
            return grid.ToString();
        }

    }
}
