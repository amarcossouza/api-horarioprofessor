using System.Text;

namespace API_HorarioProfessor.Services.CoreServices.FrontEnd.CadastroProfessor
{
    public class HtmlGenerator
    {
        private readonly AppState _appState;

        public HtmlGenerator(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        public string CadastrarProfessor(string[] disciplinas, string[] disciplinasDescricoes, string[] disciplinasValores, List<TurmaProf> ListTurmasProf)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='pt'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Cadastrar Professores</title>");
            html.AppendLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container mt-5'>");
            html.AppendLine("        <h2 class='text-center'>Cadastrar Professores</h2>");

            // Tabela com as turmas
            html.AppendLine("        <table class='table table-bordered'>");
            html.AppendLine("            <tr>");
            //html.AppendLine("                <td class='text-left'>Selecione as turmas do professor</td>");
            html.AppendLine("                <td class='text-left fs-5'>Selecione as turmas do professor</td>");
            html.AppendLine("            </tr>");
            html.AppendLine("            <tr>");
            html.AppendLine("                <td class='text-left'>");
            html.AppendLine("                    <button class='btn btn-sm btn-primary me-2' onclick='marcarTodos(event)'>Marcar Todos</button>");
            html.AppendLine("                    <button class='btn btn-sm btn-outline-secondary' onclick='desmarcarTodos(event)'>Desmarcar Todos</button>");
            html.AppendLine("                </td>");
            html.AppendLine("            </tr>");
            html.AppendLine("            <tr>");
            html.AppendLine("                <td class='text-left'>");
            html.AppendLine("                    <div class='btn-group'>");
            foreach (var turma in ListTurmasProf)
            {
                html.AppendLine($"                        <button class='btn btn-sm turma-btn border rounded' data-id='{turma.TurmaIndex}' onclick='toggleButton(this)'>{turma.Nome}</button>");
            }
            html.AppendLine("                    </div>");
            html.AppendLine("                </td>");
            html.AppendLine("            </tr>");
            html.AppendLine("        </table>");


            // Formulário para adicionar professores
            html.AppendLine("        <h6>Digite nome, selecione matéria e quantidade de aulas.</h6>");
            html.AppendLine("        <div class='input-group mb-3'>");
            html.AppendLine("            <input type='text' class='form-control' id='nomeProfessor' placeholder='Nome do Professor' oninput='this.value = this.value.toUpperCase()' required>");
            html.AppendLine($"            <select class='form-control' id='disciplina' required>{GetOptionsHtml(disciplinas, disciplinasDescricoes, disciplinasValores)}</select>");
            html.AppendLine("            <input type='number' class='form-control' id='quantidadeAula' placeholder='Quantidade de Aula' min='1' required>");
            html.AppendLine("            <button class='btn btn-success' type='button' onclick='adicionarProfessor()'>Adicionar</button>");
            html.AppendLine("        </div>");
            html.AppendLine("        <ul class='list-group' id='listaProfessores'></ul>");
            html.AppendLine("    </div>");
            html.AppendLine("    <script>");
            html.AppendLine("        var professores = [];");
            html.AppendLine("        function adicionarProfessor() {");
            html.AppendLine("            var nome = document.getElementById('nomeProfessor').value;");
            html.AppendLine("            var disciplina = document.getElementById('disciplina').value;");
            html.AppendLine("            var quantidadeAula = document.getElementById('quantidadeAula').value;");
            html.AppendLine("            if (nome !== '' && disciplina !== '' && quantidadeAula > 0) {");
            html.AppendLine("                var professor = { nome: nome, disciplina: disciplina, quantidadeAula: parseInt(quantidadeAula) };");
            html.AppendLine("                professores.push(professor);");
            html.AppendLine("                professores.sort((a, b) => a.nome.localeCompare(b.nome));");
            html.AppendLine("                document.getElementById('nomeProfessor').value = '';");
            html.AppendLine("                document.getElementById('disciplina').selectedIndex = 0;");
            html.AppendLine("                document.getElementById('quantidadeAula').value = '';");
            html.AppendLine("                atualizarLista();");
            html.AppendLine("                document.getElementById('nomeProfessor').focus();");
            html.AppendLine("            } else {");
            html.AppendLine("                alert('Por favor, preencha todos os campos corretamente e a quantidade de aulas deve ser maior que zero.');");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("        function removerProfessor(index) {");
            html.AppendLine("            professores.splice(index, 1);");
            html.AppendLine("            atualizarLista();");
            html.AppendLine("        }");
            html.AppendLine("        function atualizarLista() {");
            html.AppendLine("            var lista = document.getElementById('listaProfessores');");
            html.AppendLine("            lista.innerHTML = '';"); // Limpa a lista atual
            html.AppendLine("            professores.forEach(function(prof, index) {");
            html.AppendLine("                var li = document.createElement('li');");
            html.AppendLine("                li.className = 'list-group-item d-flex justify-content-between align-items-center';");
            html.AppendLine("                li.innerHTML = prof.nome + ' - ' + prof.disciplina + ' - ' + prof.quantidadeAula + ' aulas <button class=\"btn btn-warning btn-sm\" onclick=\"removerProfessor(' + index + ')\">Remover</button>';");  // Adiciona botão de remover
            html.AppendLine("                lista.appendChild(li);");
            html.AppendLine("            });");
            html.AppendLine("        }");

            // Script para marcar todos os botões
            html.AppendLine("        function marcarTodos(event) {");
            html.AppendLine("            var panel = event.target.closest('.container');");
            html.AppendLine("            var botoes = panel.querySelectorAll('.turma-btn');");
            html.AppendLine("            botoes.forEach(function(botao) {");
            html.AppendLine("                botao.classList.add('selected');");
            html.AppendLine("                botao.style.backgroundColor = 'green';");
            html.AppendLine("                botao.style.color = 'white';");
            html.AppendLine("            });");
            html.AppendLine("        }");

            // Script para desmarcar todos os botões
            html.AppendLine("        function desmarcarTodos(event) {");
            html.AppendLine("            var panel = event.target.closest('.container');");
            html.AppendLine("            var botoes = panel.querySelectorAll('.turma-btn');");
            html.AppendLine("            botoes.forEach(function(botao) {");
            html.AppendLine("                botao.classList.remove('selected');");
            html.AppendLine("                botao.style.backgroundColor = '';");
            html.AppendLine("                botao.style.color = '';");
            html.AppendLine("            });");
            html.AppendLine("        }");



            // Script para alternar a seleção de botões
            html.AppendLine("        function toggleButton(button) {");
            html.AppendLine("            button.classList.toggle('selected');");
            html.AppendLine("            if (button.classList.contains('selected')) {");
            html.AppendLine("                button.style.backgroundColor = 'green';");
            html.AppendLine("                button.style.color = 'white';");
            html.AppendLine("            } else {");
            html.AppendLine("                button.style.backgroundColor = '';");
            html.AppendLine("                button.style.color = '';");
            html.AppendLine("            }");
            html.AppendLine("        }");


            html.AppendLine("        function adicionarProfessor() {");
            html.AppendLine("            var nome = document.getElementById('nomeProfessor').value;");
            html.AppendLine("            var disciplina = document.getElementById('disciplina').value;");
            html.AppendLine("            var quantidadeAula = document.getElementById('quantidadeAula').value;");
            html.AppendLine("            var turmasSelecionadas = document.querySelectorAll('.turma-btn.selected').length;");
            html.AppendLine("            if (nome !== '' && disciplina !== '' && quantidadeAula > 0 && turmasSelecionadas > 0) {");
            html.AppendLine("                var professor = { nome: nome, disciplina: disciplina, quantidadeAula: parseInt(quantidadeAula) };");
            html.AppendLine("                professores.push(professor);");
            html.AppendLine("                professores.sort((a, b) => a.nome.localeCompare(b.nome));");
            html.AppendLine("                document.getElementById('nomeProfessor').value = '';");
            html.AppendLine("                document.getElementById('disciplina').selectedIndex = 0;");
            html.AppendLine("                document.getElementById('quantidadeAula').value = '';");
            html.AppendLine("                atualizarLista();");
            html.AppendLine("                document.getElementById('nomeProfessor').focus();");
            html.AppendLine("            } else {");
            html.AppendLine("                if (turmasSelecionadas === 0) {");
            html.AppendLine("                    alert('Por favor, selecione as turmas do professor.');");
            html.AppendLine("                } else {");
            html.AppendLine("                    alert('Por favor, preencha todos os campos corretamente e a quantidade de aulas deve ser maior que zero.');");
            html.AppendLine("                }");
            html.AppendLine("                return false;"); // Retorna false caso falte algum dado
            html.AppendLine("            }");
            html.AppendLine("            return true;"); // Retorna true se tudo estiver preenchido corretamente
            html.AppendLine("        }");


            html.AppendLine("    </script>");









            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();


            return html.ToString();
        }

        public string GetOptionsHtml(string[] disciplinas, string[] disciplinasDescricoes, string[] disciplinasValores)
        {
            StringBuilder optionsHtml = new StringBuilder();
            for (int i = 0; i < disciplinas.Length; i++)
            {
                optionsHtml.Append($"<option value=\"{disciplinasValores[i]}\">{disciplinas[i]} - {disciplinasDescricoes[i]}</option>");
            }
            return optionsHtml.ToString();
        }

    }
}
