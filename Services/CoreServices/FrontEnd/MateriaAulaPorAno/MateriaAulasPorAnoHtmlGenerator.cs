using System.Text;

namespace API_HorarioProfessor.Services.CoreServices.FrontEnd.MateriaAulaPorAno
{
    public class MateriaAulasPorAnoHtmlGenerator
    {
        private readonly AppState _appState;
        public MateriaAulasPorAnoHtmlGenerator(AppState appState)
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
            html.AppendLine("    <title>Gestão de Matérias e Aulas por Ano</title>");
            html.AppendLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container mt-5'>");
            html.AppendLine("        <h1>Gestão de Matérias e Aulas por Ano</h1>");
            html.AppendLine("        <div class='mb-3'>");
            html.AppendLine("            <label for='materiaSelect' class='form-label'>Matéria:</label>");
            html.AppendLine("            <select class='form-control' id='materiaSelect'>");
            html.AppendLine("                <option value=''>Selecione a Matéria</option>");
            html.AppendLine("                <option value='MAT'>Matemática</option>");
            html.AppendLine("                <option value='INF'>Informática</option>");
            html.AppendLine("                <option value='CIE'>Ciências</option>");
            html.AppendLine("                <option value='HIS'>História</option>");
            html.AppendLine("                <option value='ING'>Inglês</option>");
            html.AppendLine("                <option value='GEO'>Geografia</option>");
            html.AppendLine("                <option value='EDU'>Educação</option>");
            html.AppendLine("                <option value='ART'>Artes</option>");
            html.AppendLine("                <option value='POR'>Português</option>");
            html.AppendLine("                <option value='LEI'>Leitura</option>");
            html.AppendLine("            </select>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='mb-3'>");
            html.AppendLine("            <label for='anoEscolar' class='form-label'>Ano Escolar:</label>");
            html.AppendLine("            <input type='number' class='form-control' id='anoEscolar' placeholder='Ano Escolar (6 a 9)'>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='mb-3'>");
            html.AppendLine("            <label for='quantidadeAulas' class='form-label'>Quantidade de Aulas:</label>");
            html.AppendLine("            <input type='number' class='form-control' id='quantidadeAulas' placeholder='Quantidade de Aulas'>");
            html.AppendLine("        </div>");
            html.AppendLine("        <button type='button' class='btn btn-primary' onclick='adicionarEntrada()'>Adicionar</button>");
            html.AppendLine("        <table class='table mt-4'>");
            html.AppendLine("            <thead>");
            html.AppendLine("                <tr>");
            html.AppendLine("                    <th>Matéria</th>");
            html.AppendLine("                    <th>Ano Escolar</th>");
            html.AppendLine("                    <th>Quantidade de Aulas</th>");
            html.AppendLine("                    <th>Ações</th>");
            html.AppendLine("                </tr>");
            html.AppendLine("            </thead>");
            html.AppendLine("            <tbody id='materiaList'></tbody>");
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
            html.AppendLine("<script src='https://code.jquery.com/jquery-3.6.0.min.js'></script>");
            html.AppendLine("<script>");
            html.AppendLine("    function adicionarEntrada() {");
            html.AppendLine("        const materia = document.getElementById('materiaSelect').value;");
            html.AppendLine("        const ano = document.getElementById('anoEscolar').value;");
            html.AppendLine("        const qtdAulas = document.getElementById('quantidadeAulas').value;");

            html.AppendLine("        if (materia && ano && qtdAulas) {");
            html.AppendLine("            const tabela = document.getElementById('materiaList'");
            html.AppendLine("            const linha = tabela.insertRow(");
            html.AppendLine("           linha.insertCell(0).textContent = materia;");
            html.AppendLine("            linha.insertCell(1).textContent = ano;");
            html.AppendLine("            linha.insertCell(2).textContent = qtdAulas;");
            html.AppendLine("            const acoesCell = linha.insertCell(3");
            html.AppendLine("            const removerBtn = document.createElement('button'");
            html.AppendLine("            removerBtn.textContent = 'Remover';");
            html.AppendLine("            removerBtn.className = 'btn btn-danger';");
            html.AppendLine("            removerBtn.onclick = function() { tabela.deleteRow(linha.rowIndex");
            html.AppendLine("            acoesCell.appendChild(removerBtn");
            html.AppendLine("        } else {");
            html.AppendLine("           alert('Por favor, preencha todos os campos.'");
            html.AppendLine("        }");
            html.AppendLine("        document.getElementById('materiaSelect').value = '';");
            html.AppendLine("        document.getElementById('anoEscolar').value = '';");
            html.AppendLine("        document.getElementById('quantidadeAulas').value = '';");
            html.AppendLine("    }");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}
