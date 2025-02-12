namespace API_HorarioProfessor.Services.CoreServices.FrontEnd.Turmashtml
{
    /*
 Explicação
Posicionamento e Tamanho das Abas: O CSS adicionado (#myTabContent, .nav-tabs) controla a posição das abas (topo ou fundo) e define suas dimensões. O container principal tem position: 
relative e min-height: 100vh para garantir que haja espaço suficiente na página para o posicionamento absoluto das abas.
Parâmetros de Tamanho: tabHeight e tabWidth permitem controlar o tamanho das áreas das abas, com valores padrão de 50vh (50% da altura da viewport) e 100% da largura, respectivamente.
Isso deve garantir que as abas se comportem conforme sua descrição, semelhante ao comportamento das abas no Excel, podendo ser facilmente ajustadas para ocupar diferentes porções do layout da página.
 */

    /*
     ### Explicação das Alterações
    - **Container Flexível**: O container principal agora é definido como um contêiner flexível com a direção da coluna (`flex-direction: column`). Isso ajuda a controlar a disposição do conteúdo 
    principal e das abas de forma mais eficaz.

    - **Conteúdo Principal Rolável**: O `.main-content` é configurado para expandir (`flex: 1`), ocupando todo o espaço disponível exceto o ocupado pelas abas. Além disso, ele é 
    configurado para ter rolagem automática no eixo y (`overflow-y: auto`), o que permite que apenas o conteúdo dentro dele role, mantendo as abas fixas no lugar.

    - **Abas Fixas**: As abas são fixadas na parte inferior da tela (`position: fixed; bottom: 0; left: 0; width: 100%;`). Isso garante que elas permaneçam visíveis e fixas enquanto o usuário rola pelo conteúdo principal. 
    A propriedade `z-index: 1000;` garante que as abas permaneçam sobre o conteúdo rolável.

    - **Padrão de Margem e Padding**: Remoção de margens e preenchimentos padrão com `margin: 0; padding: 0;` no `body` e `html` para evitar espaços indesejados ao redor do container.

    ### Resultado Esperado
    Com essas modificações, as abas devem agora comportar-se como as abas no Excel, ficando fixas na parte inferior da tela (quando `tabsPosition` é "bottom") ou na parte superior (quando "top").
    O conteúdo dentro de `.main-content` será o único que rola, e as abas permanecerão estáticas e acessíveis independentemente da posição da rolagem.

    Essa implementação garante que a interface do usuário seja intuitiva e funcional, especialmente para


     */
    public class FrontEndGenerateHtmlWithTabs
    {
        private readonly AppState _appState;
        public FrontEndGenerateHtmlWithTabs(AppState appState)
        {

            _appState = appState;

        }

        public static List<string> tabNames = new List<string> { "Horário", "Opção Professor", "Turmas", "Cadastro Professor", "Opção Horário", "AulaQtdPorAno" };
        public static List<string> tabIds = new List<string> { "horario", "opcaoProfessor", "turmas", "cadastroProfessor", "opcaoHorario", "aulaQtdPorAno" };

        public static void GenerateHtmlWithTabs(string path, string tabsPosition, string NomeDaEscola, Dictionary<string, string> tabContents, bool generateSingleLine = false, int tabHeight = 50, int tabWidth = 100, int scrollbarWidth = 20, int contentMaxHeight = 90)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                if (generateSingleLine)
                {
                    writer.Write("<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                    writer.Write("<title>Relatório de Horários com Abas</title><link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
                    writer.Write("<script src='https://code.jquery.com/jquery-3.6.0.min.js'></script><script src='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.bundle.min.js'></script>");
                    writer.Write("<style>#myTabContent{min-height:" + tabHeight + "vh;width:" + tabWidth + "%;overflow-y:auto;}.nav-tabs{position:" + (tabsPosition == "bottom" ? "fixed" : "relative") + ";bottom:" + (tabsPosition == "bottom" ? "0" : "auto") + ";width:100%;}.tab-pane{max-height:" + contentMaxHeight + "vh;overflow-y:auto;}::-webkit-scrollbar{width:" + scrollbarWidth + "px;}::-webkit-scrollbar-track{background:#f1f1f1;}::-webkit-scrollbar-thumb{background:#888;}::-webkit-scrollbar-thumb:hover{background:#555;}</style>");
                    writer.Write("</head><body style='padding-bottom:" + tabHeight + "vh;'><div class='container mt-3' style='position:relative;min-height:100vh;'>");
                    writer.Write("<style>.nome-escola{font-size:24px;font-family:Arial,sans-serif;font-weight:bold;text-align:center;}.centro{text-align:center;vertical-align:middle;}</style>");
                    writer.Write("<div style='background-color:#1877F2;height:10px;width:100%;'></div>");
                    writer.Write("<table class='mb-3' style='margin-top:20px;'><tr><td style='padding-right:20px;'><img src='img_horario_professor.jpeg' alt='Logotipo' width='300' /></td><td class='centro'><h1 class='display-6 nome-escola'>" + NomeDaEscola + "</h1></td></tr></table>");
                }
                else
                {
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine("<html lang='en'>");
                    writer.WriteLine("<head>");
                    writer.WriteLine("    <meta charset='UTF-8'>");
                    writer.WriteLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                    writer.WriteLine("    <title>Relatório de Horários com Abas</title>");
                    writer.WriteLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
                    writer.WriteLine("    <script src='https://code.jquery.com/jquery-3.6.0.min.js'></script>");
                    writer.WriteLine("    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.bundle.min.js'></script>");
                    writer.WriteLine("    <style>");
                    writer.WriteLine($"        #myTabContent {{ min-height: {tabHeight}vh; width: {tabWidth}%; overflow-y: auto; }}");
                    writer.WriteLine($"        .nav-tabs {{ position: {(tabsPosition == "bottom" ? "fixed" : "relative")}; bottom: {(tabsPosition == "bottom" ? "0" : "auto")}; width: 100%; }}");
                    writer.WriteLine($"        .tab-pane {{ max-height: {contentMaxHeight}vh; overflow-y: auto; }}");
                    writer.WriteLine("        /* Custom Scrollbar styles */");
                    writer.WriteLine($"        ::-webkit-scrollbar {{ width: {scrollbarWidth}px; }}");
                    writer.WriteLine("        ::-webkit-scrollbar-track { background: #f1f1f1; }");
                    writer.WriteLine("        ::-webkit-scrollbar-thumb { background: #888; }");
                    writer.WriteLine("        ::-webkit-scrollbar-thumb:hover { background: #555; }");
                    writer.WriteLine("    </style>");
                    writer.WriteLine("</head>");
                    writer.WriteLine($"<body style='padding-bottom: {tabHeight}vh;'>");
                    writer.WriteLine("    <div class='container mt-3' style='position: relative; min-height: 100vh;'>");
                    writer.WriteLine("<style>");
                    writer.WriteLine(".nome-escola {");
                    writer.WriteLine("    font-size: 24px;");
                    writer.WriteLine("    font-family: Arial, sans-serif;");
                    writer.WriteLine("    font-weight: bold;");
                    writer.WriteLine("    text-align: center;");
                    writer.WriteLine("}");
                    writer.WriteLine(".centro {");
                    writer.WriteLine("    text-align: center;");
                    writer.WriteLine("    vertical-align: middle;");
                    writer.WriteLine("}");
                    writer.WriteLine("</style>");
                    writer.WriteLine("<div style='background-color: #1877F2; height: 10px; width: 100%;'></div>");
                    writer.WriteLine("<table class='mb-3' style='margin-top: 20px;'>");
                    writer.WriteLine("    <tr>");
                    writer.WriteLine("        <td style='padding-right: 20px;'>");
                    writer.WriteLine("            <img src='img_horario_professor.jpeg' alt='Logotipo' width='300' />");
                    writer.WriteLine("        </td>");
                    writer.WriteLine("        <td class='centro'>");
                    writer.WriteLine("            <h1 class='display-6 nome-escola'>" + NomeDaEscola + "</h1>");
                    writer.WriteLine("        </td>");
                    writer.WriteLine("    </tr>");
                    writer.WriteLine("</table>");
                }

                if (tabsPosition == "top")
                {
                    RenderTabs(writer, tabContents, generateSingleLine);
                }

                writer.WriteLine("<div class='main-content' style='padding-top: 10px;'>");
                writer.WriteLine("    <!-- Placeholder for main content or dynamically generated content -->");
                writer.WriteLine("</div>");

                if (tabsPosition == "bottom")
                {
                    RenderTabs(writer, tabContents, generateSingleLine);
                }

                writer.WriteLine("    </div>");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }




        private static void RenderTabs(StreamWriter writer, Dictionary<string, string> tabContents)
        {
            RenderTabs(writer, tabContents, false);
        }

        private static void RenderTabs(StreamWriter writer, Dictionary<string, string> tabContents, bool generateSingleLine)
        {
            if (generateSingleLine)
            {
                writer.Write("        <ul class='nav nav-tabs' id='myTab' role='tablist'>");
                for (int i = 0; i < tabNames.Count; i++)
                {
                    writer.Write($"        <li class='nav-item' role='presentation'><button class='nav-link {(i == 0 ? "active" : "")}' id='{tabIds[i]}-tab' data-bs-toggle='tab' data-bs-target='#{tabIds[i]}' type='button' role='tab' aria-controls='{tabIds[i]}' aria-selected='{(i == 0 ? "true" : "false")}'>{tabNames[i]}</button></li>");
                }
                writer.Write("        </ul>");
                writer.Write("        <div class='tab-content' id='myTabContent'>");
                for (int i = 0; i < tabNames.Count; i++)
                {
                    writer.Write($"        <div class='tab-pane fade {(i == 0 ? "show active" : "")}' id='{tabIds[i]}' role='tabpanel' aria-labelledby='{tabIds[i]}-tab'>{tabContents.GetValueOrDefault(tabIds[i], "<p>No content available</p>")}</div>");
                }
                writer.Write("        </div>");
            }
            else
            {
                writer.WriteLine("        <ul class='nav nav-tabs' id='myTab' role='tablist'>");
                for (int i = 0; i < tabNames.Count; i++)
                {
                    writer.WriteLine($"        <li class='nav-item' role='presentation'>");
                    writer.WriteLine($"            <button class='nav-link {(i == 0 ? "active" : "")}' id='{tabIds[i]}-tab' data-bs-toggle='tab' data-bs-target='#{tabIds[i]}' type='button' role='tab' aria-controls='{tabIds[i]}' aria-selected='{(i == 0 ? "true" : "false")}'>{tabNames[i]}</button>");
                    writer.WriteLine("        </li>");
                }
                writer.WriteLine("        </ul>");

                writer.WriteLine("        <div class='tab-content' id='myTabContent'>");
                for (int i = 0; i < tabNames.Count; i++)
                {
                    writer.WriteLine($"        <div class='tab-pane fade {(i == 0 ? "show active" : "")}' id='{tabIds[i]}' role='tabpanel' aria-labelledby='{tabIds[i]}-tab'>");
                    writer.WriteLine($"            {tabContents.GetValueOrDefault(tabIds[i], "<p>No content available</p>")}");
                    writer.WriteLine("        </div>");
                }
                writer.WriteLine("        </div>");
            }
        }
    }

}
