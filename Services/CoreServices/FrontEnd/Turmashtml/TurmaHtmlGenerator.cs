using System.Net.NetworkInformation;
using System.Text;

namespace API_HorarioProfessor.Services.CoreServices.FrontEnd.Turmashtml
{


    /* csharp
    Copy code
    string turmas = "6A,6B,6C";
    string script = TurmaHtmlGenerator.GenerateJsSelectCall(turmas);
    // Incluir esse script no HTML que é enviado ao navegador
    Para desselecionar todas as turmas ao carregar a página:
    csharp
    Copy code
    string script = TurmaHtmlGenerator.GenerateJsDeselectCall();
    // Incluir esse script no HTML que é enviado ao navegador*
    */
    public class TurmaHtmlGenerator
    {
        private readonly AppState _appState;
        public TurmaHtmlGenerator(AppState appState)
        {
            _appState = appState;
        }

        public string GerarHtmlTurmas(
            string buttonColor = "white",
            string buttonTextColor = "black",
            string selectedButtonColor = "#4CAF50", // Verde escolar
            string selectedItemColor = "#4CAF50",   // Verde escolar para itens selecionados na lista
            string selectedItemTextColor = "white", // Texto branco para itens selecionados
            string[] grades = null,                 // Adicionando grades como um parâmetro opcional
            string fontSize = "12px"                // Adicionando o tamanho da fonte como parâmetro opcional
        )
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='pt'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <title>Seleção de Turmas</title>");
            html.AppendLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
            html.AppendLine("    <style>");
            html.AppendLine("        .turma-btn {");
            html.AppendLine("            cursor: pointer;");
            html.AppendLine("            margin: 2px;"); // Reduz a margem
            html.AppendLine("            padding: 5px 10px;"); // Reduz o padding
            html.AppendLine("            border: 1px solid #ccc;");
            html.AppendLine("            border-radius: 5px;");
            html.AppendLine("            color: " + buttonTextColor + ";");
            html.AppendLine("            background-color: " + buttonColor + ";");
            html.AppendLine("            display: inline-block;"); // Garante que os botões fiquem na mesma linha
            html.AppendLine("            font-size: " + fontSize + ";"); // Define o tamanho da fonte
            html.AppendLine("        }");
            html.AppendLine("        .selected {");
            html.AppendLine("            background-color: " + selectedButtonColor + ";");
            html.AppendLine("            color: " + selectedItemTextColor + ";");
            html.AppendLine("        }");
            html.AppendLine("        #selectedClasses li {");
            html.AppendLine("            margin: 5px;");
            html.AppendLine("            padding: 10px;");
            html.AppendLine("            border: 1px solid #666;");
            html.AppendLine("            list-style-type: none;");
            html.AppendLine("            cursor: pointer;");
            html.AppendLine("            background-color: " + buttonColor + ";");
            html.AppendLine("            color: " + buttonTextColor + ";");
            html.AppendLine("        }");
            html.AppendLine("        .selected-list-item {");
            html.AppendLine("            background-color: " + selectedItemColor + ";");
            html.AppendLine("            color: " + selectedItemTextColor + ";");
            html.AppendLine("        }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div id='classContainer' class='container'>");

            if (grades == null)
            {
                grades = new string[] { "6", "7", "8", "9" };
            }

            string sections = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            foreach (var grade in grades)
            {
                html.AppendLine($"<div class='grade-row' style='margin-bottom: 10px;'>");
                foreach (var section in sections)
                {
                    html.AppendFormat("<div class='turma-btn' id='{0}{1}' onclick='toggleSelection(this)'>{0}{1}</div>", grade, section);
                }
                html.AppendLine("</div>");
            }

            // Adicione o script JavaScript abaixo do conteúdo HTML dos botões de turma
            html.AppendLine("    <script>");
            html.AppendLine("        function sendSelectedTurmas() {");
            html.AppendLine("            const selectedElements = document.querySelectorAll('.turma-btn.selected');");
            html.AppendLine("            const selectedIds = Array.from(selectedElements).map(element => element.id);");
            html.AppendLine("");
            html.AppendLine("            fetch('/save-selections', {");
            html.AppendLine("                method: 'POST',");
            html.AppendLine("                headers: {");
            html.AppendLine("                    'Content-Type': 'application/json'");
            html.AppendLine("                },");
            html.AppendLine("                body: JSON.stringify({ selectedIds: selectedIds })");
            html.AppendLine("            })");
            html.AppendLine("            .then(response => response.json())");
            html.AppendLine("            .then(data => console.log('Success:', data))");
            html.AppendLine("            .catch((error) => {");
            html.AppendLine("                console.error('Error:', error);");
            html.AppendLine("            });");
            html.AppendLine("        }");
            html.AppendLine("    </script>");

            html.AppendLine("    </div>");
            html.AppendLine("    <div class='container mt-4'>");
            html.AppendLine("        <h2 style='display: inline;'>Turmas Selecionadas:</h2>");
            html.AppendLine("        <button onclick='saveSelections()' class='btn btn-primary' style='margin-left: 20px;'>Gravar</button>");
            html.AppendLine("        <button onclick='clearSelections()' class='btn btn-secondary' style='margin-left: 10px;'>Limpar</button>");
            html.AppendLine("    </div>");
            html.AppendLine("    <ul id='selectedClasses' class='list-unstyled container mt-2'></ul>");
            html.AppendLine("    <script>");
            html.AppendLine("        function toggleSelection(element) {");
            html.AppendLine("            element.classList.toggle('selected');");
            html.AppendLine("            updateSelectedList();");
            html.AppendLine("        }");

            html.AppendLine("        function updateSelectedList() {");
            html.AppendLine("            const selectedItems = document.querySelectorAll('.turma-btn.selected');");
            html.AppendLine("            const list = document.getElementById('selectedClasses');");
            html.AppendLine("            list.innerHTML = ''; // Limpa a lista antes de adicionar os itens selecionados");

            html.AppendLine("            // Converter NodeList para array e ordenar");
            html.AppendLine("            const sortedItems = Array.from(selectedItems).sort((a, b) => a.textContent.localeCompare(b.textContent));");

            html.AppendLine("            // Agora adiciona os itens ordenados à lista");
            html.AppendLine("            sortedItems.forEach(item => {");
            html.AppendLine("                const li = document.createElement('li');");
            html.AppendLine("                li.textContent = item.textContent;");
            html.AppendLine("                li.className = 'p-2 border';");
            html.AppendLine("                li.onclick = function() {");
            html.AppendLine("                    this.classList.toggle('selected-list-item');");
            html.AppendLine("                    if (!this.classList.contains('selected-list-item')) {");
            html.AppendLine("                        this.style.backgroundColor = 'white';");
            html.AppendLine("                        this.style.color = 'black';");
            html.AppendLine("                    }");
            html.AppendLine("                    toggleSelection(document.getElementById(item.textContent));");
            html.AppendLine("                };");
            html.AppendLine("                list.appendChild(li);");
            html.AppendLine("            });");
            html.AppendLine("        }");

            html.AppendLine("        function saveSelections() {");
            html.AppendLine("            const selectedIds = Array.from(document.querySelectorAll('.turma-btn.selected')).map(btn => btn.id);");
            html.AppendLine("            console.log('Seleções salvas:', selectedIds); // Substitua com a lógica de gravação adequada");
            html.AppendLine("        }");
            html.AppendLine("        function clearSelections() {");
            html.AppendLine("            document.querySelectorAll('.turma-btn.selected').forEach(btn => {");
            html.AppendLine("                btn.classList.remove('selected');");
            html.AppendLine("            });");
            html.AppendLine("            updateSelectedList();");
            html.AppendLine("        }");
            html.AppendLine("    </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

    }
}
