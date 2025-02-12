using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

using API_HorarioProfessor.Services.CoreServices;

namespace API_HorarioProfessor.Services.CoreServices.BuilderHorario
{
    public class GerarHtmlMatrizPreenchida
    {
        private readonly AppState _appState;

        // Adicione uma chave secreta para criptografia
        // private static readonly byte[] chaveCriptografia = Encoding.UTF8.GetBytes("SuaChaveSecretaAqui_ela_precisaSerBemGrandePARAfUNCIONARokVamosContinuarAGORApORFAVOR");
        private readonly byte[] chaveCriptografia;

        // Adicionando um campo estático para armazenar o nome do primeiro professor
        private string primeiroProfessor;

        public GerarHtmlMatrizPreenchida(AppState appState)
        {
            _appState = appState; // Injetando o AppState
            chaveCriptografia = GenerateRandomKey(); // Inicializa no construtor
        }

        public string RelatorioGerarHtmlMatriz(string path)
        {

            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html lang='en'>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("    <meta charset='UTF-8'>");
            htmlBuilder.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            htmlBuilder.AppendLine("    <title>Relatório de Matrizes de Horários</title>");
            htmlBuilder.AppendLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
            htmlBuilder.AppendLine("    <style>");
            htmlBuilder.AppendLine("        body { font-size: 10px; }");  // Reduzimos o tamanho da fonte base para 10px
            htmlBuilder.AppendLine("        h3 { font-size: 12px; margin-bottom: 8px; }");  // Ajustamos o tamanho da fonte dos títulos principais
            htmlBuilder.AppendLine("        h5 { font-size: 11px; margin-top: 8px; }");  // Ajustamos o tamanho da fonte dos subtítulos
            htmlBuilder.AppendLine("        .table { font-size: 10px; margin-bottom: 4px; }");  // Reduzimos o tamanho da fonte das tabelas
            htmlBuilder.AppendLine("        .table th, .table td { padding: 1px 1px; margin: 0; }");  // Reduzimos o padding e removemos as margens
            htmlBuilder.AppendLine("        .grid-table { table-layout: fixed; width: 100%; }");
            htmlBuilder.AppendLine("        .grid-table th, .grid-table td { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }");
            htmlBuilder.AppendLine("        .scrollable-grid { overflow-x: auto; }");
            htmlBuilder.AppendLine("        .highlight { background-color: yellow !important; }");  // Classe CSS para destacar células, com !important para garantir a aplicação
            htmlBuilder.AppendLine("    </style>");

            //UM NOVO TRATAMENTO PERA PEGAR O EVENTO DO DOM
            htmlBuilder.AppendLine("<script>");
            htmlBuilder.AppendLine("document.addEventListener('DOMContentLoaded', function() {");
            htmlBuilder.AppendLine("    var primeiroProfessor = '';");
            htmlBuilder.AppendLine("    var tables = document.querySelectorAll('.grid-table');");
            htmlBuilder.AppendLine("    tables.forEach(function(table) {");
            htmlBuilder.AppendLine("        var cells = table.querySelectorAll('td');");
            htmlBuilder.AppendLine("        cells.forEach(function(cell) {");
            htmlBuilder.AppendLine("            var professorName = cell.innerText.trim().toUpperCase();");
            htmlBuilder.AppendLine("            if (professorName !== '' && primeiroProfessor === '') {");
            htmlBuilder.AppendLine("                primeiroProfessor = professorName;");
            htmlBuilder.AppendLine("                document.getElementById('professorName').value = primeiroProfessor;");
            htmlBuilder.AppendLine("                document.getElementById('okButton').click();");
            htmlBuilder.AppendLine("            }");
            htmlBuilder.AppendLine("        });");
            htmlBuilder.AppendLine("    });");
            htmlBuilder.AppendLine("});");
            htmlBuilder.AppendLine("</script>");




            htmlBuilder.AppendLine("    <script>");
            htmlBuilder.AppendLine("        function highlightProfessor() {");
            htmlBuilder.AppendLine("            console.log('highlightProfessor function called');");  // Adiciona log para depuração
            htmlBuilder.AppendLine("            var professorName = document.getElementById('professorName').value.trim().toUpperCase();"); // Convertendo para maiúsculas
            htmlBuilder.AppendLine("            console.log('Professor Name:', professorName);");  // Adiciona log para depuração
            htmlBuilder.AppendLine("            if (professorName !== '') {");
            htmlBuilder.AppendLine("                var tables = document.querySelectorAll('.grid-table');");
            htmlBuilder.AppendLine("                tables.forEach(function(table) {");
            htmlBuilder.AppendLine("                    var cells = table.querySelectorAll('td');");
            htmlBuilder.AppendLine("                    cells.forEach(function(cell) {");
            htmlBuilder.AppendLine("                        if (cell.innerText.trim().toUpperCase() === professorName) {"); // Comparando em maiúsculas
            htmlBuilder.AppendLine("                            cell.classList.add('highlight');");
            htmlBuilder.AppendLine("                            console.log('Highlight added to cell:', cell);");  // Log para verificar células destacadas
            htmlBuilder.AppendLine("                        }");
            htmlBuilder.AppendLine("                    });");
            htmlBuilder.AppendLine("                });");
            htmlBuilder.AppendLine("            }");
            htmlBuilder.AppendLine("        }");
            htmlBuilder.AppendLine("");
            htmlBuilder.AppendLine("        function clearHighlight() {");
            htmlBuilder.AppendLine("            console.log('clearHighlight function called');");  // Adiciona log para depuração
            htmlBuilder.AppendLine("            var highlightedCells = document.querySelectorAll('.highlight');");
            htmlBuilder.AppendLine("            highlightedCells.forEach(function(cell) {");
            htmlBuilder.AppendLine("                cell.classList.remove('highlight');");
            htmlBuilder.AppendLine("                console.log('Highlight removed from cell:', cell);");  // Log para verificar células destacadas
            htmlBuilder.AppendLine("            });");
            htmlBuilder.AppendLine("        }");
            htmlBuilder.AppendLine("");
            htmlBuilder.AppendLine("        function handleOkClick() {");
            htmlBuilder.AppendLine("            clearHighlight();");
            htmlBuilder.AppendLine("            highlightProfessor();");
            htmlBuilder.AppendLine("        }");
            htmlBuilder.AppendLine("    </script>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine("    <div class='container-fluid'>");
            htmlBuilder.AppendLine("        <div class='row'>");
            htmlBuilder.AppendLine("            <div class='col-12'>");
            htmlBuilder.AppendLine("                <h3 class='text-center my-2'>Distribuíção de Horários</h3>");

            htmlBuilder.AppendLine("                <div class='row justify-content-start align-items-center'>"); // Alinhando à esquerda e centralizando verticalmente
            htmlBuilder.AppendLine("                    <div class='col-auto'>"); // Coluna com largura automática
            htmlBuilder.AppendLine("                        <span class='input-group-text' style='background-color: transparent; border: none; font-size: smaller; padding: 2px;'>Nome do Professor</span>"); // Reduzindo o tamanho do texto
            htmlBuilder.AppendLine("                    </div>");
            htmlBuilder.AppendLine("                    <div class='col-auto'>"); // Coluna com largura automática
            htmlBuilder.AppendLine("                        <input type='text' id='professorName' class='form-control form-control-sm' style='border-radius: 0; font-size: smaller; width: 120px;' placeholder='Professor'>"); // Reduzindo o tamanho do texto e do campo de texto
            htmlBuilder.AppendLine("                    </div>");
            htmlBuilder.AppendLine("                    <div class='col-auto' style='margin-left: -5px;'>"); // Coluna com largura automática e ajuste de margem para eliminar o espaço
            htmlBuilder.AppendLine("                        <button id='okButton' class='btn btn-secondary btn-sm' style='border-radius: 0; font-size: smaller; padding: 2px 5px;' onclick='handleOkClick()'>Ok</button>"); // Mudando o texto do botão para "Ok" e reduzindo o tamanho
            htmlBuilder.AppendLine("                    </div>");
            htmlBuilder.AppendLine("                    <div class='col-auto' style='margin-left: -5px;'>"); // Coluna com largura automática e ajuste de margem para eliminar o espaço
            htmlBuilder.AppendLine("                        <button class='btn btn-success btn-sm' style='border-radius: 0; font-size: smaller; padding: 2px 5px;' onclick='clearHighlight()'>Limpar</button>"); // Adicionando botão Limpar
            htmlBuilder.AppendLine("                    </div>");
            htmlBuilder.AppendLine("                </div>");


            // Gerar tabela para cada dia da semana
            GenerateDayTable(htmlBuilder, "Segunda-feira", _appState.matrizHorariosSegunda);
            GenerateDayTable(htmlBuilder, "Terça-feira", _appState.matrizHorariosTerca);
            GenerateDayTable(htmlBuilder, "Quarta-feira", _appState.matrizHorariosQuarta);
            GenerateDayTable(htmlBuilder, "Quinta-feira", _appState.matrizHorariosQuinta);
            GenerateDayTable(htmlBuilder, "Sexta-feira", _appState.matrizHorariosSexta);

            htmlBuilder.AppendLine("            </div>");
            htmlBuilder.AppendLine("        </div>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            ////// Convertendo StringBuilder para string
            ////string htmlContent = htmlBuilder.ToString();

            ////// Salvando o HTML no arquivo especificado
            ////File.WriteAllText(path, htmlContent);
            ///


            // Convertendo StringBuilder para string
            string htmlContent = htmlBuilder.ToString();

            // Minificar o HTML
            string minifiedHtml = MinifyHtml(htmlContent);

            // Salvando o HTML minificado no arquivo especificado
            File.WriteAllText(path, minifiedHtml);

            // Retornando o HTML minificado como string
            return minifiedHtml;


            // Retornando o HTML como string
            return htmlContent;
        }

        private void GenerateDayTable(StringBuilder htmlBuilder, string dayName, string[,] dayMatrix)
        {
            htmlBuilder.AppendLine($"<h6>{dayName}</h6>");
            htmlBuilder.AppendLine("<div class='scrollable-grid'>");
            htmlBuilder.AppendLine("<table class='table table-striped grid-table'>");
            htmlBuilder.AppendLine("<thead>");
            htmlBuilder.AppendLine("<tr>");
            foreach (var turma in _appState.Turmas)
            {
                htmlBuilder.AppendLine($"<th style='font-size: 14px;'>{turma}</th>");
            }
            htmlBuilder.AppendLine("</tr>");
            htmlBuilder.AppendLine("</thead>");
            htmlBuilder.AppendLine("<tbody>");
            for (int i = 0; i < dayMatrix.GetLength(0); i++)
            {
                htmlBuilder.AppendLine("<tr>");
                for (int j = 0; j < dayMatrix.GetLength(1); j++)
                {
                    string professorName = dayMatrix[i, j].Trim();
                    // Verifica se o nome do professor não está vazio e se ainda não capturamos o primeiro professor
                    if (!string.IsNullOrEmpty(professorName) && string.IsNullOrEmpty(primeiroProfessor))
                    {
                        // Captura o nome do primeiro professor
                        primeiroProfessor = professorName;
                        // Define o nome do primeiro professor no TextBox
                        htmlBuilder.AppendLine($"<script>document.getElementById('professorName').value = '{primeiroProfessor}';</script>");
                        // Aciona o evento do botão "Ok"
                        htmlBuilder.AppendLine($"<script>document.getElementById('okButton').click();</script>");
                    }
                    // Ajuste para aumentar o tamanho da fonte do conteúdo da célula
                    htmlBuilder.AppendLine($"<td style='font-size: 11px;'><b>{professorName}</b></td>");
                }
                htmlBuilder.AppendLine("</tr>");
            }
            htmlBuilder.AppendLine("</tbody>");
            htmlBuilder.AppendLine("</table>");
            htmlBuilder.AppendLine("</div>");
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
                Console.WriteLine("Erro ao abrir o navegador: " + e.Message);
            }
        }

        // Método para criptografar uma string
        private byte[] EncryptStringToBytes(string plainText, byte[] key)
        {
            // Verifique se a chave tem o tamanho adequado para o algoritmo AES-256
            if (key.Length != 32)
            {
                throw new ArgumentException("A chave deve ter 32 bytes para o algoritmo AES-256.");
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                byte[] encrypted;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                // Return the encrypted bytes from the memory stream.
                return encrypted;
            }
        }


        private byte[] GenerateRandomKey()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }



        // Método para minificar o HTML
        public string MinifyHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            // Remova todos os nós de comentário do HTML
            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//comment()"))
            {
                node.ParentNode.RemoveChild(node);
            }

            // Remove as quebras de linha
            html = html.Replace("\n", string.Empty);
            html = html.Replace("\r", string.Empty);

            // Remove espaços em branco desnecessários
            html = Regex.Replace(html, @"[^\S\r\n]+", " ");

            // Remova espaços em branco desnecessários
            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                document.Save(writer);
            }

            return stringBuilder.ToString();
        }
    }


}

