using API_HorarioProfessor.Controllers;
using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services.CoreServices.BuilderHorario;
using API_HorarioProfessor.Services.CoreServices.ClasseDiversas;
using API_HorarioProfessor.Services.CoreServices.Classes;
using API_HorarioProfessor.Services.CoreServices.FrontEnd.CadastroProfessor;
using API_HorarioProfessor.Services.CoreServices.FrontEnd.MateriaAulaPorAno;
using API_HorarioProfessor.Services.CoreServices.FrontEnd.OpcoesProfessor;
using API_HorarioProfessor.Services.CoreServices.FrontEnd.Turmashtml;
using API_HorarioProfessor.Services.CoreServices.RelatorioGradeTurmas;
using API_HorarioProfessor.Services.CoreServices.Relatorios;
using API_HorarioProfessor.Services.CoreServices.Sources.Gerenciamento;
using API_HorarioProfessor.Services.CoreServices.TrocaTresProfessores;
using API_HorarioProfessor.Services.ServicesCore;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using API_HorarioProfessor.Data;

using System;
using System.Linq;

using System.Threading.Tasks;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Net.NetworkInformation;
using API_HorarioProfessor.Services.CoreServices.MetodosAuxiliares;
using API_HorarioProfessor.Classes;
using API_HorarioProfessor.DTO;
using System.Diagnostics;
using static API_HorarioProfessor.Models.ProfessorInsercaoPossibilidade;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using projeto_escola_2024_Matriz.Services;

namespace API_HorarioProfessor.Services.CoreServices
{
    public class AppState
    {

        // *********************************************************************
        //              Propriedade de Classes
        // *********************************************************************
        public WorkflowManager WorkflowManager { get; }
        public List<TaskStep> TaskSteps { get; }
        public Relatorio Relatorio { get; set; }
        public RelatorioGerarHtmlTodasMatrizes RelatorioGerarHtmlTodasMatrizes { get; set; }
        public RelatorioInversoProfessorNoTitulo RelatorioInversoProfessorNoTitulo { get; set; }

        // *********************************************************************
        //              matrizes
        // *********************************************************************
        public string[,] matrizHorariosSegunda { get; set; }
        public string[,] matrizHorariosTerca { get; set; }
        public string[,] matrizHorariosQuarta { get; set; }
        public string[,] matrizHorariosQuinta { get; set; }
        public string[,] matrizHorariosSexta { get; set; }

        public int tamanhoColunaTurmas { get; set; }

        public int[,] SaldoAulasProfessores { get; set; }

        // *********************************************************************
        //              Listas
        // *********************************************************************
        public List<string> Turmas { get; set; }
        public List<TurmaProf> ListTurmasProf { get; set; }


        public int Controla_Numero_Item_Opcao_Troca { get; set; }
        public bool dobradinha { get; set; }

        public int QuantidadeHorarios { get; set; }

        public int Fechamento { get; set; } = 0;


        public List<MovimentacaoProfessorFechamento> ListaMovimentacaoProfessorFechamento { get; set; }

        public List<ProfessorNecessidadeSaiVolta> ListaProfessorNecessidadeSaiVolta { get; set; }
        
        // *********************************************************************
        //              propriedade professor
        // *********************************************************************
        //private readonly IProfessorService _professorService;
        //public Dictionary<string, (int AulasPorTurma, Dictionary<string,
        //    List<string>> DistribuicaoTurmas, string DiasNaoTrabalha,
        //    string NomeAtividadeDescolar, bool DobrarAula, string Restricoes,
        //    string AtividadeAtpc, string DiasAtpc, bool Prioridade)> Professores
        //{ get; set; }



        public Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> Professores { get; private set; }
        public Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> COPIA { get; private set; }

        //public Dictionary<string, List<(int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade, OpcoesProfessor opcoesProfessor)>> Professores { get; private set; }

        //public List<Professor> Professores { get; set; }

        // Lista de movimentações precisa ser estática
        public List<MovimentacaoTurma> movimentacoes;

        public Dictionary<string, Dictionary<int, int>> materiaAulasPorAno = new Dictionary<string, Dictionary<int, int>>();

        public List<RegistroDeTroca> RegistrosDeTrocas = new List<RegistroDeTroca>();

        public Dictionary<string, NecessidadeProfessor> necessidadesProfessores = new Dictionary<string, NecessidadeProfessor>();

        public Dictionary<string, NecessidadeProfessor> localNecessidadeProfessor = new Dictionary<string, NecessidadeProfessor>();


        public Dictionary<string, NecessidadeProfessor> necessidadesProfessoresFechamento = new Dictionary<string, NecessidadeProfessor>();

        public string[] diasDaSemana = {
        "Segunda-feira",
        "Terça-feira",
        "Quarta-feira",
        "Quinta-feira",
        "Sexta-feira"
        };

        // *********************************************************************
        //              propriedade da escola
        // *********************************************************************

        public int idEscola { get; set; }
        public string periodo { get; set; }
        public int ativo { get; set; }
        public int anoLetivo { get; set; }

        public int OrdenarMaiorNumeroOpcao1OuOpcao2 { get; set; }

        private readonly TurmaPadraoEscolaService _turmaService;
        private readonly GetMaterialAulasPorAno _materialAula;
        private readonly HorarioDistribuicaoService _horarioDistribuicaoService;


        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppState _appState;
        public AppState( IServiceScopeFactory scopeFactory, 
                        TurmaPadraoEscolaService turmaService,
                        GetMaterialAulasPorAno materialAula,
            HorarioDistribuicaoService horarioDistribuicaoService
                       )
        {
            _scopeFactory = scopeFactory;
            _turmaService = turmaService;
            _materialAula = materialAula;
            _horarioDistribuicaoService = horarioDistribuicaoService;
            // _context = context;
            //_appState = appState;



            /*********************************/
            /*      Executa o codigo         */
            /*********************************/
            idEscola = 1; periodo = "Manha"; ativo = 1;  anoLetivo = 2024;
            if (Professores == null)
            {
                Professores = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();
            }

           // TurmaPadraoEscolaController tpdrao = new TurmaPadraoEscolaController();
            


            //** ATENCAO QDO FOR CRIA INTERFACE DO USUARIO 
            //    ** TRATAR as DUAS VARIAVEIS  CLASSE E turmas

            // Lista de instâncias da classe TurmaProf

            ListTurmasProf = new List<TurmaProf>()
            {
                new TurmaProf(0, "6W","NAO",_appState),
                new TurmaProf(1, "6X","NAO",_appState),
                new TurmaProf(2, "6Y","NAO",_appState),
                new TurmaProf(3, "7W","NAO",_appState),
                new TurmaProf(4, "7X","NAO",_appState),
                new TurmaProf(5, "7Y","NAO",_appState),
                new TurmaProf(6, "8W","NAO",_appState),
                new TurmaProf(7, "8X","NAO",_appState),
                new TurmaProf(8, "8Y","NAO",_appState),
                new TurmaProf(9, "9W","NAO",_appState),
                new TurmaProf(10, "9X","NAO",_appState),
                new TurmaProf(11, "9Y","NAO",_appState),
            };


            // Inicializar a lista Turmas
            Turmas = new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" };


            // Inicializar as matrizes com valores vazios
            matrizHorariosSegunda = InicializarMatriz(6, Turmas.Count);
            matrizHorariosTerca = InicializarMatriz(6, Turmas.Count);
            matrizHorariosQuarta = InicializarMatriz(6, Turmas.Count);
            matrizHorariosQuinta = InicializarMatriz(6, Turmas.Count);
            matrizHorariosSexta = InicializarMatriz(6, Turmas.Count);

            Main(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);


            // *********************************************************************
            // Inicializa o WorkflowManager e a lista de TaskSteps
            // *********************************************************************
            this.WorkflowManager = new WorkflowManager(this);
            TaskSteps = new List<TaskStep>();
        }


        public async Task Main(int idEscola, string periodo, int ativo, int anoLetivo, int OrdenarMaiorNumeroOpcao1OuOpcao2,  int QuantidadeHorarios)
        {
            this.idEscola = idEscola;
            this.periodo = periodo;
            this.ativo = ativo;
            this.anoLetivo = anoLetivo;
            this.OrdenarMaiorNumeroOpcao1OuOpcao2 = OrdenarMaiorNumeroOpcao1OuOpcao2;

            //** vamos implementar o codigo para subtituir o codigo acima hardcode 
            // ---------------------------------------------------------------------------------------------
            // Carrega turmas da escola
            this.ListTurmasProf = new List<TurmaProf>();
            this.Turmas = new List<string>();

            this.ListaMovimentacaoProfessorFechamento = new List<MovimentacaoProfessorFechamento>();

            CarregarTurmasAsync(anoLetivo, periodo, idEscola).Wait(); // Método assíncrono chamado dentro de um construtor

            // -------- inicio
            // ** preciso processar as materias juntos com as escolhas de aulas dos professores 
            this.materiaAulasPorAno = new Dictionary<string, Dictionary<int, int>>();
            ObterMateriaAulasPorAno(anoLetivo, periodo, idEscola).Wait();

            
            // -------- fim 

            this.QuantidadeHorarios = QuantidadeHorarios;

            if (Fechamento == 0)
            {
                // Inicializar as matrizes com valores vazios
                this.matrizHorariosSegunda = InicializarMatriz(this.QuantidadeHorarios, this.Turmas.Count);
                this.matrizHorariosTerca = InicializarMatriz(this.QuantidadeHorarios, this.Turmas.Count);
                this.matrizHorariosQuarta = InicializarMatriz(this.QuantidadeHorarios, this.Turmas.Count);
                this.matrizHorariosQuinta = InicializarMatriz(this.QuantidadeHorarios, this.Turmas.Count);
                this.matrizHorariosSexta = InicializarMatriz(this.QuantidadeHorarios, this.Turmas.Count);

            }

            //---------------------------------------------------------------------------------







            Relatorio = new Relatorio(this);
            RelatorioGerarHtmlTodasMatrizes = new RelatorioGerarHtmlTodasMatrizes(this);
            RelatorioInversoProfessorNoTitulo = new RelatorioInversoProfessorNoTitulo(this);


            // importante quantidade das colunas
            tamanhoColunaTurmas = 20 ; 

            Controla_Numero_Item_Opcao_Troca = 2;


          

            dobradinha = true;



            SaldoAulasProfessores = new int[0, 0]; // Ajuste os tamanhos conforme necessário


            //Professores = new Dictionary<string, List<(int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool, OpcoesProfessor)>>();




            // Inicialização e chamadas de métodos, garantindo que o sistema respeite o MaxReallocationAttempts
            Scheduler sched = new Scheduler();
            sched.MaxReallocationAttempts = 100;  // Você pode ajustar esse valor conforme necessário

            //****************
            //atencao seguier a ordem das inicializacao das variaveis PARA NAO DAR ERRO 
            this.movimentacoes = new List<MovimentacaoTurma>();






            //// Inicializa o dicionário de professores antes de usar
            //materiaAulasPorAno = new Dictionary<string, Dictionary<int, int>>();
            //materiaAulasPorAno.Add("MAT", new Dictionary<int, int> { { 0, 5 } });
            //materiaAulasPorAno.Add("INF", new Dictionary<int, int> { { 0, 1 } });
            //materiaAulasPorAno.Add("CIE", new Dictionary<int, int> { { 0, 4 } });
            //materiaAulasPorAno.Add("HIS", new Dictionary<int, int> { { 6, 4 }, { 7, 4 }, { 8, 3 }, { 9, 3 } });
            //materiaAulasPorAno.Add("ING", new Dictionary<int, int> { { 0, 2 } });
            //materiaAulasPorAno.Add("GEO", new Dictionary<int, int> { { 6, 3 }, { 7, 3 }, { 8, 4 }, { 9, 4 } });
            //materiaAulasPorAno.Add("EDU", new Dictionary<int, int> { { 0, 3 } });
            //materiaAulasPorAno.Add("ART", new Dictionary<int, int> { { 0, 2 } });
            //materiaAulasPorAno.Add("POR", new Dictionary<int, int> { { 0, 5 } });
            //materiaAulasPorAno.Add("LEI", new Dictionary<int, int> { { 0, 1 } });

            // this.Professores =  InicializarProfessores(idEscola, periodo, ativo, anoLetivo); // primeiro professor para depois seguir com a movimentacao 

            this.Professores = await InicializarProfessores(idEscola, periodo, ativo, anoLetivo);

            InicializarMovimentacoes();
            // =========---------------------------------------------------------------------------------------------------------------
            // 25-01-2025
            //PRECISO FAZER PROCESSAMENTO POR TURMAS E RETIRAR OS 
            // PROFESSORES QUE SAO POLIVALENTES
            // ELES SERAO INSERIDOS NA MAO

            // Lista temporária para armazenar os itens que serão removidos    
            // Cria uma nova lista para armazenar os itens que devem ser removidos

            // Percorre a lista de movimentações de trás para frente para evitar problemas ao remover itens
            for (int i = movimentacoes.Count - 1; i >= 0; i--)
            {
                if (new[] { "REG", "POL" }.Contains(movimentacoes[i].Sigla))
                {
                    movimentacoes.RemoveAt(i);
                }
            }

 
            // Cria uma lista temporária para armazenar as chaves a serem removidas
            var chavesParaRemover = new List<string>();

            foreach (var kvp in Professores)
            {
                // Acessando o primeiro valor da chave dentro do dicionário DistribuicaoTurmas (Item2)
                if (kvp.Value.Item2.Keys.FirstOrDefault() != null &&
                    new[] { "REG", "POL" }.Contains(kvp.Value.Item2.Keys.First()))
                {
                    chavesParaRemover.Add(kvp.Key);
                }
            }

            // Remove os professores da lista original usando as chaves armazenadas
            foreach (var chave in chavesParaRemover)
            {
                Professores.Remove(chave);
            }

            // -----------====================------------------------=====================-----------------------


            // Exemplo de chamada de método passando a lista de turmas como argumento
            InicializarMatriz(QuantidadeHorarios, Turmas.Count);



            string[,] InicializarMatriz(int linhas, int colunas)
            {
                string[,] matriz = new string[linhas, colunas];
                for (int i = 0; i < linhas; i++)
                {
                    for (int j = 0; j < colunas; j++)
                    {
                        matriz[i, j] = "";
                    }
                }
                return matriz;
            }



            //INICIO
            var workflow = new WorkflowManager(this);

            // QUER DIZER FECHAMENTO NORMAL
            if (Fechamento == 0)
            {



                // PROCESSAMENTO 2 QUE RESOLVEU A QUESTAO DA DISTIRBUICAO SEM TER QUE FAZER
                // A TROCA ENTRE COLUNAS 3 PESSOAS 

                // Adicionando tarefas ao fluxo de trabalho

                //fiz isto pois nao descobri porque o codigo nao carrega o professor certinho

                //////Professores = COPIA;
                workflow.AddTask(new TaskStep(this, "Distribuir Horários", () => DistribuirHorarios(Professores, movimentacoes)));
                
             
                workflow.AddTask(new TaskStep(this, "Reagendar Horários Não Distribuídos", ReagendarHorariosNaoDistribuidos));      
               
                workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));              

                workflow.AddTask(new TaskStep(this, "Processar Realocações por Coluna", () => ProcessarRealoacoesColuna(Professores))); // //() => SomeCondition())); // Exemplo com condição


                workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));
                workflow.AddTask(new TaskStep(this, "Processar Realocações", ProcessarRealoacoes));
                

                workflow.AddTask(new TaskStep(this, "Processar Realocações por Coluna", () => ProcessarRealoacoesColuna(Professores))); // //() => SomeCondition())); // Exemplo com condição
    
//parece que este causou problema mistorou as quantidades na hora da distribuicao
// workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));
     
                //gerou problemas na distribuicao quando adicionei novamente
                workflow.AddTask(new TaskStep(this, "Reagendar Horários Não Distribuídos", ReagendarHorariosNaoDistribuidos));




                TrocasEntreTresProfessores trocaTresProf = new TrocasEntreTresProfessores(this);
                  /////8 codigo novo TROCA ENTRE TRES PROFESSORES
                string[][,] _matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };
                workflow.AddTask(new TaskStep(this, "Processar Troca entre Três Professores", () => trocaTresProf.RealizarTrocasTresProfessoresExecutar(_matrizes)));

                workflow.AddTask(new TaskStep(this, "Processar Realocações", ProcessarRealoacoes));

                workflow.AddTask(new TaskStep(this, "Inserir Professor Vertical e Horizontal", IncluirProfessorVertical_MoverHorizontal));


                workflow.AddTask(new TaskStep(this, "Processar Realocações por Coluna", () => ProcessarRealoacoesColuna(Professores))); // //() => SomeCondition())); // Exemplo com condição


                // workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));

                // workflow.AddTask(new TaskStep(this, "Processar Realocações por Coluna", () => ProcessarRealoacoesColuna(Professores))); // //() => SomeCondition())); // Exemplo com condição

                // workflow.AddTask(new TaskStep(this, "Processar Realocações", ProcessarRealoacoes));

                //workflow.AddTask(new TaskStep(this, "Processar Troca entre Três Professores", () => trocaTresProf.RealizarTrocasTresProfessoresExecutar(_matrizes)));

                // novo metodo para 
                //workflow.AddTask(new TaskStep(this, "juntar professores na mesma coluna - dobradinha", UnirProfessoresAgendadoDeHorarios));


                // trocaTresProf = new TrocasEntreTresProfessores(this);
                //  /////8 codigo novo TROCA ENTRE TRES PROFESSORES
                // _matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };
                // workflow.AddTask(new TaskStep(this, "Processar Troca entre Três Professores", () => trocaTresProf.RealizarTrocasTresProfessoresExecutar(_matrizes)));


                //workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));

                // workflow.AddTask(new TaskStep(this, "Processar Realocações", ProcessarRealoacoes));

                // FIM 
                //workflow.AddTask(new TaskStep(this, "Processar Troca entre Três Professores", () => trocaTresProf.RealizarTrocasTresProfessoresExecutar(_matrizes)));

                //workflow.AddTask(new TaskStep(this, "Processar Realocações", ProcessarRealoacoes));



                // workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));

                //// novo metodo para 
                ////workflow.AddTask(new TaskStep(this, "juntar professores na mesma coluna - dobradinha", UnirProfessoresAgendadoDeHorarios));


                //workflow.AddTask(new TaskStep(this, "Inserir Professor Vertical e Horizontal", IncluirProfessorVertical_MoverHorizontal));

                // workflow.AddTask(new TaskStep(this, "Resolve sai Volta Aula", ResolverSairVoltaAula_MoverHorizontal));







                /*

                // ***********************
                // ESTO FAZENDO ESTE TESTE PENSANDO EM TER DUAS DISTRIBUICAO AUTOMATICA COM SUGESTOES 
                // PROCESSAMENTO 1 SEQUENCIA HORIGINAL 
                // A TROCA ENTRE COLUNAS 3 PESSOAS 

                // Adicionando tarefas ao fluxo de trabalho

                ////1
                //workflow.AddTask(new TaskStep("Distribuir Horários", () => DistribuirHorarios(professores, movimentacoes)));

                //////3
                //workflow.AddTask(new TaskStep("Reagendar Horários Não Distribuídos", ReagendarHorariosNaoDistribuidos));
                //////4
                //workflow.AddTask(new TaskStep("Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));
                //////5
                //workflow.AddTask(new TaskStep("Processar Realocações por Coluna", () => ProcessarRealoacoesColuna(professores))); // //() => SomeCondition())); // Exemplo com condição

                ////2
                //workflow.AddTask(new TaskStep("Iniciar Alocação", IniciarProcessoDeAlocacao));

                //////7       
                //workflow.AddTask(new TaskStep("Processar Realocações", ProcessarRealoacoes));
                ////workflow.AddTask(new TaskStep("Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));
                //workflow.AddTask(new TaskStep("Processar Realocações por Coluna", () => ProcessarRealoacoesColuna(professores))); // //() => SomeCondition())); // Exemplo com condição


                /////8 codigo novo TROCA ENTRE TRES PROFESSORES
                //string[][,] _matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };
                //workflow.AddTask(new TaskStep("Processar Troca entre Três Professores", () => TrocasEntreTresProfessores.RealizarTrocasTresProfessoresExecutar(_matrizes)));
                */
                //////3
                //workflow.AddTask(new TaskStep("Reagendar Horários Não Distribuídos", ReagendarHorariosNaoDistribuidos));

                //workflow.AddTask(new TaskStep(this, "Resolve sai Volta Aula", ResolverSairVoltaAula_MoverHorizontal));

                // Executando o fluxo de trabalho
                workflow.Execute();

            }

            // FECHAMENTO ESPECIAL COM FALHAS AINDA NA DISTRIBUICAO
            if(Fechamento == 1) 
            {

                this.ListaMovimentacaoProfessorFechamento = new List<MovimentacaoProfessorFechamento>();
                necessidadesProfessores = necessidadesProfessoresFechamento;

                // workflow.AddTask(new TaskStep(this, "Reagendar Horários Não Distribuídos", ReagendarHorariosNaoDistribuidos));
                workflow.AddTask(new TaskStep(this, "Inserir Professor Vertical e Horizontal", IncluirProfessorVertical_MoverHorizontal));

               // workflow.AddTask(new TaskStep(this, "Identificar Turmas Sem Agendamento", IdentificarTurmasSemAgendamentoCompleto));

                //workflow.AddTask(new TaskStep(this, "juntar professores na mesma coluna - dobradinha", UnirProfessoresAgendadoDeHorarios));

                //workflow.AddTask(new TaskStep(this, "Resolve sai Volta Aula", ResolverSairVoltaAula_MoverHorizontal));




                workflow.Execute();
            }



            // Distribuir horários para os professores conforme as regras e restrições
            // Chamada correta ao método
            //---1
            //DistribuirHorarios(professores, movimentacoes);
            // ReprocessarTurmas(professores, movimentacoes);
            //ReprocessarTurmas(Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas)> professores, List<MovimentacaoTurma> movimentacoes)


            // ---2
            // IniciarProcessoDeAlocacao();



            //var dadosSegunda  = matrizHorariosSegunda;
            // Opcional: Imprimir as matrizes para verificar a distribuição dos horários
            await MostrarDistribuicaoHorarios("Segunda-feira", matrizHorariosSegunda, Turmas);
            await MostrarDistribuicaoHorarios("Terca-feira", matrizHorariosTerca, Turmas);
            await MostrarDistribuicaoHorarios("Quarta-feira", matrizHorariosQuarta, Turmas);
            await MostrarDistribuicaoHorarios("Quinta-feira", matrizHorariosQuinta, Turmas);
            await MostrarDistribuicaoHorarios("Sexta-feira", matrizHorariosSexta, Turmas);
            // Repetir para os outros dias conforme necessário



            //TodosRelatorios.Relatorios.GerarRelatorioProfessores("Segunda-feira", matrizHorariosSegunda);
            // Matrizes de horários por dia da semana
            string[][,] matrizes = {
        matrizHorariosSegunda,
        matrizHorariosTerca,
        matrizHorariosQuarta,
        matrizHorariosQuinta,
        matrizHorariosSexta
        };
            // Nomes dos dias da semana correspondentes às matrizes
            string[] diasDaSemana = {
            "Segunda-feira",
            "Terça-feira",
            "Quarta-feira",
            "Quinta-feira",
            "Sexta-feira"
            };


            var relatorio = new Relatorio(_appState);
            relatorio.GerarRelatorioDetalhado(matrizes, diasDaSemana);

            // Chamada do método para imprimir as necessidades pendentes
            string TituloRelatorio = "Necessidades Professores:";
            relatorio.GerarRelatorioConsolidado(matrizes, diasDaSemana);

            // aqui é a impressao do ultimo estado da grade.
            ImprimirNecessidadesPendentes("Necessidades Professores true:", localNecessidadeProfessor, false);
            ImprimirNecessidadesPendentes("Necessidades Professores false:", necessidadesProfessores, false);



            #region fazer uma analise das contagem aqui vou colocar o codigo


            Contabilizar_Aulas contabAulas = new Contabilizar_Aulas(this);

            // VERIFICAR QUANTAS TURMAS AINDA FALTA SER DISTRIBUIDAS 
            // #####################################################
            List<ContagemAulasProfessor> LerMatrizLocalizarAgendamentoFaltantes = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> GerarListaTodasAulasProfessor = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> agendamentosFaltantes = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);



            //ImprimirRelatorioDeContagemDeAulas(resultados);

            LerMatrizLocalizarAgendamentoFaltantes = contabAulas.ProcessarMatrizesEIdentificarTurmasFaltantes(matrizes).OrderBy(x => x.NomeProfessor).ToList();
            //ImprimirRelatorioDeContagemDeAulas(resultados);

            GerarListaTodasAulasProfessor = contabAulas.PreencherTabelaContagemAulasProfessor().OrderBy(x => x.NomeProfessor).ToList();
            // ImprimirRelatorioDeContagemDeAulas(resultados);

            agendamentosFaltantes = contabAulas.IdentificarAgendamentosFaltantes(GerarListaTodasAulasProfessor, LerMatrizLocalizarAgendamentoFaltantes);


            var GerandoNecessidadeProfessor = contabAulas.PreencherNecessidadesProfessores(agendamentosFaltantes);

            this.necessidadesProfessores = GerandoNecessidadeProfessor;

            #endregion 



            //********************************************************************
            //***** antes de imprimir as necessidades vamos fazer uma atualizacao 
            // lendo o gride para contar e ve verificar se esta tudo certo.

            // VERIFICAR QUANTAS TURMAS AINDA FALTA SER DISTRIBUIDAS 
            // ######################################################################

            #region atualiza as variaveis para fazer a impressãodos dados do GRIDE

            //List<ContagemAulasProfessor> todasAulasProfessor;
            //List<ContagemAulasProfessor> agendamentosFaltantes;

            //trocaTresProf.ContaProfessoresDistribuidoNaGrade(matrizes, out todasAulasProfessor, out agendamentosFaltantes);


            int contar = this.necessidadesProfessores.Count();
            //porque deu tudo certo
            if (agendamentosFaltantes.Count == 0)
            {

                List<CelulaVaziaInfo> celulasVazias = await ObterCelulasVaziasAsync(matrizes);
                int professoresNaoEntraram = celulasVazias.Count; // Contagem de professores que não entraram
                if (professoresNaoEntraram == 0)
                {
                    var retorno = await _horarioDistribuicaoService.SalvarMatrizComoJson(matrizes, this.Turmas, idEscola, periodo, anoLetivo);
                }
            }


            if (Fechamento == 1)
            {
                var retorno = await _horarioDistribuicaoService.SalvarMatrizComoJson(matrizes, this.Turmas, idEscola, periodo, anoLetivo);
            }


            #endregion

            sched.ImprimirRegistrosDeTroca();
            relatorio.GerarRelatorioConsolidado(matrizes, diasDaSemana);
            ImprimirMovimentacoes(movimentacoes, true);
            ImprimirMovimentacoes(movimentacoes, false);




/*

            string caminho = "C:\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\bin\\Debug\\net6.0";
            string path = "\\" + "Horarios.html";
            var relatorioGerar = new RelatorioGerarHtmlTodasMatrizes(this);
            relatorioGerar.RelatorioGerarHtmlTodasMatrize(caminho + path);
            relatorioGerar.AbrirNoNavegador(path);

            path = "\\" + "NovoRelHorariosTitulo.html";
            var relatorioTitulo = new RelatorioInversoProfessorNoTitulo(this);
            relatorioTitulo.RelatorioInversoProfessorTitulo(caminho + path);
            relatorioTitulo.AbrirNoNavegador(path);


            HorarioHtmlBuilder horaBuild = new HorarioHtmlBuilder(this);
            //teste de tela
            string formhtml = horaBuild.BuildHtml();
            string deucerto = "";



            //### RELATORIO QUE É COLOCADO NO PAINEL HORARIO PROFESSOR - DISTRIBUICAO DE HORARIO.
            GerarHtmlMatrizPreenchida gerarMatrizPrench = new GerarHtmlMatrizPreenchida(this);
            caminho = "C:\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\bin\\Debug\\net6.0";
            string Arquivomatrizpreenchida = caminho + "\\" + "matriz_preenchida.html";
            string horariohml = gerarMatrizPrench.RelatorioGerarHtmlMatriz(caminho + path);

            //criacao das tabs
            //caminho = "C:\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\bin\\Debug\\net6.0";
            //path = "\\" + "FrontEndRelatorioTabs.html";
            //FrontEndGenerateHtmlWithTabs.GenerateHtmlWithTabs(caminho + path, "bottom");


            caminho = "C:\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\bin\\Debug\\net6.0";
            path = "RelatorioHorarios.html";
            string horarioHtml = gerarMatrizPrench.RelatorioGerarHtmlMatriz(caminho + path);




            path = "C:\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\bin\\Debug\\net6.0\\FrontEndRelatorioTabs.html";


            // Chamar o método para gerar as abas na parte inferior da página
            path = "FrontEndRelatorioTabs.html";
            // PRECISA ARRUMAR NAO ESTA FUNCIONANDO ***;
            //FrontEndGenerateHtmlWithTabs.GenerateHtmlWithTabsBottom(caminho + path, tabContents, 50, 100, 20); // altura e largura das abas, e margem esquerda


            //######## %%%%%% CADASTRAR PROFESSOR FORMULÁRIO.
            // gerar codigo do formulario
            //Classes.professor.Professor _prof = new Classes.professor.Professor();
            ClasseProfessor _prof = new ClasseProfessor(this);
            //string HTML_GERADO_CADASTRO_PROFESSOR = ProfessorHtmlGenerator.GenerateHtml();

            // Definindo os arrays de disciplinas, descrições e valores
            string[] disciplinas = { "Matemática", "Informática", "Ciências", "História", "Inglês", "Geografia", "Educação", "Artes", "Português", "Leitura" };
            string[] descricoes = { "MAT", "INF", "CIE", "HIS", "ING", "GEO", "EDU", "ART", "POR", "LEI" };
            string[] valores = { "MAT", "INF", "CIE", "HIS", "ING", "GEO", "EDU", "ART", "POR", "LEI" }; // Os valores podem ser os mesmos que as descrições neste exemplo

            HtmlGenerator htmlGener = new HtmlGenerator(this);

            //// Chamando o método GenerateHtml() passando os arrays como parâmetros
            string HTML_GERADO_CADASTRO_PROFESSOR = htmlGener.CadastrarProfessor(disciplinas, descricoes, valores, ListTurmasProf);


            HorarioHtmlGenerator horahtmlGener = new HorarioHtmlGenerator(this);
            /// gerar codigo opcoes do professor
            string HTML_OPCOES_PROFESSOR = horahtmlGener.GenerateHtml();



            MateriaAulasPorAnoHtmlGenerator matAulaPorAno = new MateriaAulasPorAnoHtmlGenerator(this);
            string HTML_MATERIA_AULA_POR_ANO = matAulaPorAno.GenerateHtml();

            TurmaHtmlGenerator TurmaHtml = new TurmaHtmlGenerator(this);


           
            //******* ###### criando turmas 
            string[] series = { "6", "7", "8", "9" };
            string turmashtml = TurmaHtml.GerarHtmlTurmas(
                buttonColor: "white",
                buttonTextColor: "black",
                selectedButtonColor: "#4CAF50",
                selectedItemColor: "#4CAF50",
                selectedItemTextColor: "white",
                grades: series,
                fontSize: "12px"
            );
            path = "C:\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\projeto_escola_2024-Matriz\\bin\\Debug\\net6.0\\FrontEndRelatorioTabs.html";

            // Preparando o conteúdo das abas
            Dictionary<string, string> tabContents = new Dictionary<string, string>
            {
                { "horario", horariohml}, //o proprio html gerado nao é lido do disco
                { "opcaoProfessor", "<p>Opções para configuração do professor.</p>" },
                { "turmas", turmashtml },
                { "cadastroProfessor", HTML_GERADO_CADASTRO_PROFESSOR  },
                { "opcaoHorario", HTML_OPCOES_PROFESSOR  },
                { "aulaQtdPorAno", HTML_MATERIA_AULA_POR_ANO  },
            };


    
            // ##### GERADOR DOS ABAS - PAINEL 
            // Chamar o método para gerar as abas, configurando para estar no 'bottom' ou 'top'

            string NOME_DA_ESCOLA = "Nome da Escola"; // ler do banco de dados o nome da escola GenerateHtmlWithTabs
            bool ofuscar_codigo_numa_linha = false;
            FrontEndGenerateHtmlWithTabs.GenerateHtmlWithTabs(path, "top", NOME_DA_ESCOLA, tabContents, ofuscar_codigo_numa_linha, 50, 100);

            */


          
        }


        public async Task<List<CelulaVaziaInfo>> ObterCelulasVaziasAsync(string[][,] matrizes)
        {
            List<CelulaVaziaInfo> celulasVazias = new List<CelulaVaziaInfo>();

            // Função para converter uma matriz bidimensional em uma lista de listas
            List<List<string>> ConverterMatrizParaLista(string[,] matriz)
            {
                var listaConvertida = new List<List<string>>();

                for (int i = 0; i < matriz.GetLength(0); i++)
                {
                    var linha = new List<string>();
                    for (int j = 0; j < matriz.GetLength(1); j++)
                    {
                        linha.Add(matriz[i, j]);
                    }
                    listaConvertida.Add(linha);
                }

                return listaConvertida;
            }

            // Função para verificar se uma matriz está completamente vazia
            bool IsMatrizVazia(List<List<string>> matriz)
            {
                return matriz.All(linha => linha.All(string.IsNullOrEmpty));
            }

            bool todasMatrizesVazias = true;

            // Nomes das matrizes para exibição
            var nomesMatrizes = new[] { "MatrizHorariosSegunda", "MatrizHorariosTerca", "MatrizHorariosQuarta", "MatrizHorariosQuinta", "MatrizHorariosSexta" };

            for (int i = 0; i < matrizes.Length; i++)
            {
                var matrizConvertida = ConverterMatrizParaLista(matrizes[i]);

                if (IsMatrizVazia(matrizConvertida))
                {
                    continue; // Pula para a próxima matriz se esta estiver completamente vazia
                }

                // Se ao menos uma matriz não estiver vazia, desmarcamos `todasMatrizesVazias`
                todasMatrizesVazias = false;

                // Verifique células individuais vazias
                await VerificarMatrizVaziaAsync(matrizConvertida, nomesMatrizes[i], Turmas, celulasVazias);
            }

            // Se todas as matrizes estiverem vazias, adiciona uma entrada especial
            if (todasMatrizesVazias)
            {
                celulasVazias.Add(new CelulaVaziaInfo
                {
                    NomeMatriz = "Todas as Matrizes",
                    Linha = -1,
                    Coluna = -1,
                    NomeColuna = "Todas as matrizes estão completamente vazias"
                });
            }

            return celulasVazias;
        }

        // Função assíncrona para verificar e registrar células vazias em uma matriz específica
        private async Task VerificarMatrizVaziaAsync(List<List<string>> matriz, string nomeMatriz, List<string> nomesColunas, List<CelulaVaziaInfo> celulasVazias)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < matriz.Count; i++)
                {
                    for (int j = 0; j < matriz[i].Count; j++)
                    {
                        if (string.IsNullOrEmpty(matriz[i][j])) // Verifica se a célula está vazia
                        {
                            // Adiciona as informações da célula vazia à lista, incluindo o nome da coluna
                            celulasVazias.Add(new CelulaVaziaInfo
                            {
                                NomeMatriz = nomeMatriz,
                                Linha = i,
                                Coluna = j,
                                NomeColuna = nomesColunas[j] // Nome da coluna baseado na lista `Turmas`
                            });
                        }
                    }
                }
            });
        }





        // novas implementacoes gerando os dos lendo do banco 
        // tirando o codigo hardcode
        // 09/10/2024

        private async Task CarregarTurmasAsync(int AnoLetivo, string Periodo, int idEscola)
        {
            var turmas = await _turmaService.ObterTurmasAsync(AnoLetivo, Periodo, idEscola);

            int indice = 0;
            foreach (var turma in turmas)
            {
                this.ListTurmasProf.Add(new TurmaProf(indice, turma.Turmas,turma.Polivalente, _appState)); // Supondo que this seja o _appState
                indice++;
            }

            this.Turmas = turmas.Select(t => t.Turmas).ToList();

        }


        //private async Task ObterMateriaAulasPorAno(int AnoLetivo, string Periodo, int idEscola)
        //{

        //    var ListMaterias = await _materialAula.ObterMateriaAulasPorAnoAsync(AnoLetivo, idEscola);

        //    // Inicializa o dicionário de professores antes de usar
        //    materiaAulasPorAno = new Dictionary<string, Dictionary<int, int>>();
        //    materiaAulasPorAno.Add("MAT", new Dictionary<int, int> { { 0, 5 } });
        //    materiaAulasPorAno.Add("INF", new Dictionary<int, int> { { 0, 1 } });
        //    materiaAulasPorAno.Add("CIE", new Dictionary<int, int> { { 0, 4 } });
        //    materiaAulasPorAno.Add("HIS", new Dictionary<int, int> { { 6, 4 }, { 7, 4 }, { 8, 3 }, { 9, 3 } });
        //    materiaAulasPorAno.Add("ING", new Dictionary<int, int> { { 0, 2 } });
        //    materiaAulasPorAno.Add("GEO", new Dictionary<int, int> { { 6, 3 }, { 7, 3 }, { 8, 4 }, { 9, 4 } });
        //    materiaAulasPorAno.Add("EDU", new Dictionary<int, int> { { 0, 3 } });
        //    materiaAulasPorAno.Add("ART", new Dictionary<int, int> { { 0, 2 } });
        //    materiaAulasPorAno.Add("POR", new Dictionary<int, int> { { 0, 5 } });
        //    materiaAulasPorAno.Add("LEI", new Dictionary<int, int> { { 0, 1 } });
        //}

        private async Task ObterMateriaAulasPorAno(int AnoLetivo, string Periodo, int idEscola)
        {
            // Carregar as matérias do banco de dados 04-11-2024
            var ListMaterias = await _materialAula.ObterMateriaAulasPorAnoAsync(AnoLetivo, Periodo, idEscola);


            if (ListMaterias.Count() > 0)
            {


                // Inicializar o dicionário de professores antes de usar
                //materiaAulasPorAno = new Dictionary<string, Dictionary<int, int>>();

                // Loop para preencher a estrutura a partir da lista carregada do banco
                foreach (var materia in ListMaterias)
                {
                    string sigla = materia.Sigla; // Exemplo: "MAT", "HIS"
                    int todosOsAnos = materia.TodosOsAnos; // Exemplo: 0, 6, 7, 8, 9
                    int quantidadeAulas = materia.QuantidadeAulas; // Exemplo: 5, 4, 3

                    // Verificar se a sigla já existe no dicionário
                    if (!materiaAulasPorAno.ContainsKey(sigla))
                    {
                        // Se não existir, adicionar uma nova entrada com um dicionário vazio
                        this.materiaAulasPorAno[sigla] = new Dictionary<int, int>();
                    }

                    // Adicionar o "TodosOsAnos" e "QuantidadeAulas" ao dicionário interno
                    this.materiaAulasPorAno[sigla][todosOsAnos] = quantidadeAulas;
                }



            }
            else
            {
                //// Inicializa o dicionário de professores antes de usar
                this.materiaAulasPorAno = new Dictionary<string, Dictionary<int, int>>();
                this.materiaAulasPorAno.Add("MAT", new Dictionary<int, int> { { 0, 5 } });
                this.materiaAulasPorAno.Add("INF", new Dictionary<int, int> { { 0, 1 } });
                this.materiaAulasPorAno.Add("CIE", new Dictionary<int, int> { { 0, 4 } });
                this.materiaAulasPorAno.Add("HIS", new Dictionary<int, int> { { 6, 4 }, { 7, 4 }, { 8, 3 }, { 9, 3 } });
                this.materiaAulasPorAno.Add("ING", new Dictionary<int, int> { { 0, 2 } });
                this.materiaAulasPorAno.Add("GEO", new Dictionary<int, int> { { 6, 3 }, { 7, 3 }, { 8, 4 }, { 9, 4 } });
                this.materiaAulasPorAno.Add("EDU", new Dictionary<int, int> { { 0, 3 } });
                this.materiaAulasPorAno.Add("ART", new Dictionary<int, int> { { 0, 2 } });
                this.materiaAulasPorAno.Add("POR", new Dictionary<int, int> { { 0, 5 } });
                this.materiaAulasPorAno.Add("LEI", new Dictionary<int, int> { { 0, 1 } });

            }

        }

        public void InicializarSaldoAulasProfessores(Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> professores, List<string> turmas)
        {
            SaldoAulasProfessores = new int[professores.Count, turmas.Count];

            int professorIndex = 0;
            foreach (var professor in professores)
            {
                string nomeProfessor = professor.Key;
                int aulasPorTurma = professor.Value.AulasPorTurma;
                Dictionary<string, List<string>> distribuicaoTurmas = professor.Value.DistribuicaoTurmas;

                int turmaIndex = 0;
                foreach (var materiaTurmas in distribuicaoTurmas)
                {
                    List<string> turmasProfessor = materiaTurmas.Value;
                    foreach (string turma in turmasProfessor)
                    {
                        int aulasDistribuidas = turmasProfessor.Count * aulasPorTurma;
                        SaldoAulasProfessores[professorIndex, turmas.IndexOf(turma)] = aulasDistribuidas;
                    }
                    turmaIndex++;
                }

                professorIndex++;
            }
        }


        public string[,] InicializarMatriz(int linhas, int colunas)
        {
            string[,] matriz = new string[linhas, colunas];
            for (int i = 0; i < linhas; i++)
            {
                for (int j = 0; j < colunas; j++)
                {
                    matriz[i, j] = "";
                }
            }
            return matriz;
        }


        public void AdicionarAulasProfessores(Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> professores)
        {
            foreach (var professor in professores)
            {
                string nomeProfessor = professor.Key;
                int aulasPorTurma = professor.Value.AulasPorTurma;
                Dictionary<string, List<string>> distribuicaoTurmas = professor.Value.DistribuicaoTurmas;

                // Criar uma lista de turmas baseada na distribuição de turmas do professor
                List<string> turmas = new List<string>();
                foreach (var turma in distribuicaoTurmas.Keys)
                {
                    turmas.Add(turma);
                }

                // Adicionar o nome do professor na posição correta nas matrizes de horários para cada dia da semana
                // Adicionar horários dos professores nas matrizes
                AdicionarHorariosProfessor(professores, matrizHorariosSegunda);
                AdicionarHorariosProfessor(professores, matrizHorariosTerca);
                AdicionarHorariosProfessor(professores, matrizHorariosQuarta);
                AdicionarHorariosProfessor(professores, matrizHorariosQuinta);
                AdicionarHorariosProfessor(professores, matrizHorariosSexta);


            }
        }

        public void AdicionarHorariosProfessor(Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> professores, string[,] matrizHorarios)
        {
            foreach (var professor in professores)
            {
                string nomeProfessor = professor.Key;
                int aulasPorTurma = professor.Value.AulasPorTurma;
                Dictionary<string, List<string>> distribuicaoTurmas = professor.Value.DistribuicaoTurmas;

                foreach (var diasTurmas in distribuicaoTurmas)
                {
                    string materia = diasTurmas.Key;
                    List<string> turmas = diasTurmas.Value;

                    foreach (var turma in turmas)
                    {
                        // Distribui as aulas do professor para a turma de acordo com a quantidade de aulas por turma
                        for (int i = 0; i < aulasPorTurma; i++)
                        {
                            AdicionarNome(matrizHorarios, 1, turma, nomeProfessor);
                        }
                    }
                }
            }
        }

        public void AdicionarNome(string[,] matriz, int linha, string turma, string nomeProfessor)
        {
            if (linha >= 1 && linha <= matriz.GetLength(0))
            {
                int coluna = matrizHorariosSegunda.GetLength(1) - Turmas.IndexOf(turma) - 1;
                matriz[linha - 1, coluna] = nomeProfessor;
                Console.WriteLine($"Nome '{nomeProfessor}' adicionado na linha {linha}, coluna {coluna + 1} da matriz.");
            }
            else
            {
                Console.WriteLine("Linha inválida.");
            }
        }

        public async Task MostrarDistribuicaoHorarios(string nomeDia, string[,] matriz, List<string> turmas)
        {

            Console.WriteLine($"Matriz de Distribuição de Horários - {nomeDia}:");
            Console.Write("   ");
            foreach (string turma in turmas)
            {
                Console.Write(turma.PadRight(tamanhoColunaTurmas));
            }
            Console.WriteLine();

            for (int i = 0; i < matriz.GetLength(0); i++)
            {
                Console.Write($"{i + 1,-2} ");
                for (int j = 0; j < matriz.GetLength(1); j++)
                {
                    Console.Write(matriz[i, j].PadRight(tamanhoColunaTurmas));
                }
                Console.WriteLine();
            }
            Console.WriteLine();

        }


        public void AtualizarSaldoAulasProfessores(string nomeProfessor, int aulasDistribuidas)
        {
            int indiceProfessor = Array.IndexOf(Professores.Keys.ToArray(), nomeProfessor);
            if (indiceProfessor >= 0)
            {
                for (int i = 0; i < SaldoAulasProfessores.GetLength(1); i++)
                {
                    SaldoAulasProfessores[indiceProfessor, i] += aulasDistribuidas;
                }
            }
        }

        public int ContarNomeNaLinha(string[,] matriz, int linha, string nomeProfessor)
        {
            if (linha >= 1 && linha <= matriz.GetLength(0))
            {
                int vezesEncontrado = 0;
                for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                {
                    if (matriz[linha - 1, coluna] == nomeProfessor)
                    {
                        vezesEncontrado++;
                    }
                }
                return vezesEncontrado;
            }
            else
            {
                Console.WriteLine("Linha inválida.");
                return -1;
            }
        }

        public int ContarNomeNaColuna(string[,] matriz, int coluna, string nomeProfessor)
        {
            if (coluna >= 1 && coluna <= matriz.GetLength(1))
            {
                int vezesEncontrado = 0;
                for (int linha = 0; linha < matriz.GetLength(0); linha++)
                {
                    if (matriz[linha, coluna - 1] == nomeProfessor)
                    {
                        vezesEncontrado++;
                    }
                }
                return vezesEncontrado;
            }
            else
            {
                Console.WriteLine("Coluna inválida.");
                return -1;
            }
        }

        public void ImprimirMovimentacoes(List<MovimentacaoTurma> movimentacoes, bool imprimirApenasComAulasRestantes)
        {
            if (movimentacoes == null || movimentacoes.Count == 0)
            {
                Console.WriteLine("A lista de movimentações está vazia ou é nula.");
                return;
            }

            Console.WriteLine("Movimentações de Turmas:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("| NomeProfessor | Turma | QtdAula | QtdAulaAgendada | QtdAulaCalculada |");
            Console.WriteLine("-------------------------------------------------------------");

            foreach (var movimentacao in movimentacoes)
            {
                int aulasRestantes = movimentacao.QtdAula - movimentacao.QtdAulaAgendada;

                // Imprime se não for necessário filtrar ou se há aulas restantes para agendar
                if (!imprimirApenasComAulasRestantes || aulasRestantes > 0)
                {
                    Console.WriteLine($"| {movimentacao.NomeProfessor,-14} | {movimentacao.Turma,-5} | {movimentacao.QtdAula,-7} | {movimentacao.QtdAulaAgendada,-15} | {movimentacao.QtdAulaCalculada,-17} |");
                }
            }

            Console.WriteLine("-------------------------------------------------------------");
        }


        public void ImprimirNecessidadesPendentes(string TituloRelatorio, Dictionary<string, NecessidadeProfessor> necessidades, bool imprimirApenasComAulasRestantes)
        {
            if (necessidades == null || necessidades.Count == 0)
            {
                Console.WriteLine("O dicionário de necessidades está vazio ou é nulo.");
                return;
            }

            Console.WriteLine(TituloRelatorio);
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("| Chave          | NomeProfessor  | Turma | QtdAula | DiaDaSemana  | Horario | Restantes |");
            Console.WriteLine("------------------------------------------------------------------------------------------");

            foreach (var item in necessidades)
            {
                var necessidade = item.Value;
                int aulasRestantes = necessidade.QtdAula;
                //int aulasRestantes = 0;

                if (!imprimirApenasComAulasRestantes || aulasRestantes > 0)
                {
                    Console.WriteLine($"| {item.Key,-14} | {necessidade.NomeProfessor,-14} | {necessidade.Turma,-5} | {necessidade.QtdAula,-7} | {necessidade.DiaDaSemana,-11} | {necessidade.Horario,-7} | {aulasRestantes,-9} |");
                }
            }

            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        public void AtualizarMovimentacao(List<MovimentacaoTurma> movimentacoes, string nomeProfessor, string nomeTurma, int aulasAdicionadas, int totalAulasPorTurma, string Sigla, int AnoEscolar)
        {

            // Verificar se já existe uma entrada para esse professor e turma
            MovimentacaoTurma movimentar = movimentacoes.Find(m => m.NomeProfessor == nomeProfessor && m.Turma == nomeTurma);

            if (movimentar != null)
            {
                // Atualizar os valores
                movimentar.QtdAulaAgendada += aulasAdicionadas;
                movimentar.QtdAulaCalculada = movimentar.QtdAula - movimentar.QtdAulaAgendada;
            }
            else
            {
                // Criar uma nova entrada se não existir
                MovimentacaoTurma novaMovimentacao = new MovimentacaoTurma(
                    nomeProfessor,
                    nomeTurma,
                    totalAulasPorTurma,  // Defina o valor correto se for conhecido
                    aulasAdicionadas,
                    0,   // Deve ser calculado após definir QtdAula
                    Sigla, //sigla
                    AnoEscolar, //ano escolar
                    _appState

                );

                movimentacoes.Add(novaMovimentacao);
            }
        }

        public void AdicionarMovimentacao(List<MovimentacaoTurma> movimentacoes, string nomeProfessor, string turma, int qtdAula, int aulasAdicionadas, int qtdAulaCalculada, string Sigla, int AnoEscolar)
        {
            MovimentacaoTurma novaMovimentacao = new MovimentacaoTurma(
                nomeProfessor,
                turma,
                qtdAula,
                aulasAdicionadas,
                qtdAulaCalculada,
                Sigla,
                AnoEscolar,
                 _appState
            );

            movimentacoes.Add(novaMovimentacao);
        }




        public void DistribuirHorarios(
           Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> professores,
           List<MovimentacaoTurma> movimentacoes)
        {
            string[][,] matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };
            Dictionary<string, int> dobradinhasPorProfessor = new Dictionary<string, int>();
            //************@@@@@@@@@@@@@@@@@@@@@@@@@@
            //atual da  escola ELENICE  var _professores = professores.OrderByDescending(x => x.Key);//.ToArray().OrderByDescending(y => y.Value.AulasPorTurma).ToArray();
           
            if (professores  == null)
            {
                professores = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();

            }
            var _professores = professores
        .OrderBy(p => p.Key) // Ordena alfabeticamente pelo nome do professor
        .ThenBy(p => p.Value.AulasPorTurma % 2 != 0) // Ordena com números ímpares no final
        .ThenBy(p => p.Value.AulasPorTurma); // Ordena pelo número de aulas por turma


            foreach (var professor in _professores)
            {
                if (!dobradinhasPorProfessor.ContainsKey(professor.Key))
                {
                    dobradinhasPorProfessor[professor.Key] = 0;
                }

                // Verifica se dobradinhas estão permitidas globalmente e para o professor específico
                if (dobradinha && professor.Value.DobrarAula && dobradinhasPorProfessor[professor.Key] < 2)
                {
                    // Processa o professor duas vezes para dobradinha
                    ProcessarTurmasDoProfessor(matrizes, professor.Key, professor.Value, movimentacoes);
                    ProcessarTurmasDoProfessor(matrizes, professor.Key, professor.Value, movimentacoes);

                    dobradinhasPorProfessor[professor.Key]++;  // Incrementa o contador de dobradinhas para esse professor
                }

                // Processa normalmente se não houver dobradinha ou se já atingiu o limite
                ProcessarTurmasDoProfessor(matrizes, professor.Key, professor.Value, movimentacoes);
            }
        }




        private void ProcessarTurmasDoProfessor(string[][,] matrizes, string nomeProfessor, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade) dadosProfessor, List<MovimentacaoTurma> movimentacoes)
        {
            foreach (var distribuicao in dadosProfessor.DistribuicaoTurmas)
            {
                foreach (var turma in distribuicao.Value)
                {
                    int turmaIndex = Turmas.IndexOf(turma.Trim());
                    ProcessarTurma(matrizes, nomeProfessor, turmaIndex, dadosProfessor.AulasPorTurma, movimentacoes, turma);
                }
            }
        }

        private void ProcessarTurma(string[][,] matrizes, string nomeProfessor, int turmaIndex, int totalAulasPorTurma, List<MovimentacaoTurma> movimentacoes, string turma)
        {
            /*
             * ################# D O B R A D I N H A ###################3
             */


            TentarAlocarProfessorParaTurma(matrizes, nomeProfessor, turmaIndex, totalAulasPorTurma, movimentacoes, turma);
        }


        public bool ValidarAlocacaoProfessorParaReaReagendar(string[,] matriz, int idxDiaSemanaMatriz, int horario, int turmaIndex, string nomeProfessor, out string Sigla, out int AnoEscolar, string turma)
        {
            Sigla = string.Empty;
            AnoEscolar = 0;
            // Verifica se o professor já está agendado no mesmo horário em qualquer turma
            if (IsProfessorAgendadoNoHorario(matriz, horario, nomeProfessor))
            {
                return false;  // Falha na validação devido a conflito
            }

            // Verifica se o professor já está agendado na mesma turma em qualquer horário
            if (IsProfessorAgendadoNaTurma(matriz, turmaIndex, nomeProfessor, turma))
            {
                return false;  // Falha na validação devido a conflito
            }

            // Verifica se o horário na matriz está disponível
            if (!IsHorarioDisponivel(matriz, horario, turmaIndex))
            {
                return false;  // Falha na validação porque o horário já está ocupado
            }

            // Verifica se o professor ainda tem aulas a serem distribuídas
            if (!ProfessorTemAulasRestantes(nomeProfessor, turma))
            {
                return false;  // Falha na validação porque o professor não tem mais aulas a serem distribuídas
            }


            // Passou todas as validações
            return true;
        }


        public bool ValidarAlocacaoProfessorParaReaReagendarSemValidarCelulaVazia(string[,] matriz, int idxDiaSemanaMatriz, int horario, int turmaIndex, string nomeProfessor, out string Sigla, out int AnoEscolar, string turma)
        {
            Sigla = string.Empty;
            AnoEscolar = 0;
            // Verifica se o professor já está agendado no mesmo horário em qualquer turma
            if (IsProfessorAgendadoNoHorario(matriz, horario, nomeProfessor))
            {
                return false;  // Falha na validação devido a conflito
            }

            // Verifica se o professor já está agendado na mesma turma em qualquer horário
            if (IsProfessorAgendadoNaTurma(matriz, turmaIndex, nomeProfessor, turma))
            {
                return false;  // Falha na validação devido a conflito
            }

            //// Verifica se o horário na matriz está disponível
            //if (!IsHorarioDisponivel(matriz, horario, turmaIndex))
            //{
            //    return false;  // Falha na validação porque o horário já está ocupado
            //}

            // Verifica se o professor ainda tem aulas a serem distribuídas
            //if (!ProfessorTemAulasRestantes(nomeProfessor, turma))
            //{
            //    return false;  // Falha na validação porque o professor não tem mais aulas a serem distribuídas
            //}


            // Passou todas as validações
            return true;
        }


        public void AlocarProfessor(string[,] matriz, int idxDiaSemanaMatriz, int horario, int turmaIndex, string nomeProfessor, List<MovimentacaoTurma> movimentacoes, int totalAulasPorTurma, string Sigla, int AnoEscolar)
        {
            if (horario < 0 || horario >= 6)
            {
                Console.WriteLine("Erro: Valor de 'horario' fora dos limites.");
                return; // ou tratamento adequado
            }

            if (turmaIndex < 0 || turmaIndex >= Turmas.Count)
            {
                Console.WriteLine("Erro: Valor de 'turmaIndex' fora dos limites ou turma não encontrada.");
                return; // ou tratamento adequado
            }

            // Alocar o professor na matriz


            // ################Antes de alocar verificar quantas vezes o professor aparece na mesma na mesma turma em diversos horarios #################
            //bool validado = PodeAgendarProfessorNaColuna(matriz, turmaIndex, nomeProfessor);
            //if (validado)
            //{
            //    if (dobradinha == false)
            //    {
            //        validado = PodeAgendarProfessorSeInexistenteNaColuna(matriz, turmaIndex, nomeProfessor);

            //        matriz[horario, turmaIndex] = nomeProfessor;

            //        // Atualizar movimentação (este passo pressupõe que o método de atualização está implementado corretamente)
            //        AtualizarMovimentacao(movimentacoes, nomeProfessor, turmas[turmaIndex], 1, totalAulasPorTurma, Sigla, AnoEscolar);
            //    }

            //    matriz[horario, turmaIndex] = nomeProfessor;

            //    // Atualizar movimentação (este passo pressupõe que o método de atualização está implementado corretamente)
            //    AtualizarMovimentacao(movimentacoes, nomeProfessor, turmas[turmaIndex], 1, totalAulasPorTurma, Sigla, AnoEscolar);
            //}



            // ################Antes de alocar verificar quantas vezes o professor aparece na mesma na mesma turma em diversos horarios #################
            // Verifica se é possível agendar o professor na coluna especificada
            bool podeAlocar = PodeAgendarProfessorNaColuna(matriz, turmaIndex, nomeProfessor);
            if (podeAlocar)
            {
                // Se dobradinha não está ativada, verificar se o professor já está presente na coluna
                if (!dobradinha)
                {
                    podeAlocar = PodeAgendarProfessorSeInexistenteNaColuna(matriz, turmaIndex, nomeProfessor);
                }

                // Procede com a alocação se ainda é possível
                if (podeAlocar)
                {

                    //// ####  fazer validacoes de opcoes do professor   #####
                    //matriz[horario, turmaIndex] = nomeProfessor; // Aloca o professor na matriz

                    //// Atualiza as movimentações correspondentes
                    //AtualizarMovimentacao(movimentacoes, nomeProfessor, turmas[turmaIndex], 1, totalAulasPorTurma, Sigla, AnoEscolar);

                    ValidadorDeAgendamento validadorDeAgendamento = new ValidadorDeAgendamento(this);

                    //// Chama a validação
                    var resultados = validadorDeAgendamento.ValidarAgendamento(matriz, idxDiaSemanaMatriz, nomeProfessor, horario, turmaIndex);

                    bool podeAlocar2 = true;
                    foreach (var resultado in resultados.Values)
                    {
                        if (!resultado)
                        {
                            podeAlocar2 = false;
                            break;
                        }
                    }

                    if (podeAlocar2)
                    {
                        if (idxDiaSemanaMatriz == 4)
                        {
                            if (nomeProfessor == "LUIS")
                            {
                                if ((horario == 5) || (horario == 6))
                                {
                                    string E_SEXTA = "AQUI";
                                }
                            }
                        }
                        // Aloca o professor na matriz
                        matriz[horario, turmaIndex] = nomeProfessor;

                        AtualizarMovimentacao(movimentacoes, nomeProfessor, Turmas[turmaIndex], 1, totalAulasPorTurma, Sigla, AnoEscolar);
                        //Console.WriteLine("Alocado com sucesso!");
                    }
                    else
                    {
                        Console.WriteLine("Falha na alocação devido a uma restrição.");
                    }




                }
            }








        }



        public int ContarVezesDoProfessorNaColunaNaMatriz(string[,] matriz, int coluna, string nomeProfessor)
        {
            int count = 0; // Contador para o número de vezes que o nome do professor é encontrado na coluna

            // Iterar por todas as linhas na coluna especificada para contar as ocorrências do nome do professor
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                // 3AULAS
                if (matriz[linha, coluna] == nomeProfessor)
                {
                    count++; // Incrementar o contador se o nome do professor for encontrado
                }
                
                //////if (ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]) ==
                //////    ProfessorHelper.ExtrairNomeProfessor(nomeProfessor))
                //////{
                //////    count++; // Incrementar o contador se o nome do professor for encontrado
                //////}

            }

            return count;
        }


        public bool VerificarCapacidadeAulasPorTurma(string[][,] matrizes, int coluna, string nomeProfessor)
        {

            //    // true: Indica que o número total de aulas que o professor já está agendado para dar é menor que o máximo permitido.Isso significa que ainda há espaço para agendar
            //    //   mais aulas para esse professor nessa turma, em conformidade com as restrições curriculares.

            //    // false: Indica que o número de aulas já agendadas atingiu ou excedeu o limite permitido. Neste caso, não seria possível agendar mais aulas para esse professor nessa turma sem violar
            //    //     as restrições de carga horária máxima estabelecidas.


            int count = 0; // Contador para o número de vezes que o nome do professor é encontrado na coluna
            string siglaMateria = ""; // Para armazenar a sigla da matéria do professor

            // Encontrar a sigla da matéria do professor dentro do dicionário de professores
            if (Professores.TryGetValue(nomeProfessor, out var infoProfessor))
            {
                siglaMateria = infoProfessor.DistribuicaoTurmas.Keys.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(siglaMateria))
            {
                Console.WriteLine("Sigla da matéria não encontrada para o professor.");
                return false; // Se não encontrar a sigla, retorna false
            }

            // Extrair o ano escolar da turma
            int anoEscolar = ExtrairAnoEscolarDeTurmaIndex(coluna);

            // Obter a quantidade máxima de aulas permitidas para essa matéria e ano escolar
            if (materiaAulasPorAno.TryGetValue(siglaMateria, out var aulasPorAno))
            {
                int maxAulasPermitidas = aulasPorAno.GetValueOrDefault(anoEscolar, aulasPorAno.GetValueOrDefault(0));

                // Iterar sobre cada matriz para contar as ocorrências
                foreach (string[,] matrizDia in matrizes)
                {
                    for (int linha = 0; linha < matrizDia.GetLength(0); linha++)
                    {
                        if (matrizDia[linha, coluna].Equals(nomeProfessor))
                        {
                            count++;
                        }
                        
                        //////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matrizDia[linha, coluna]);
                        //////string nomeExtraidoParametro = ProfessorHelper.ExtrairNomeProfessor(nomeProfessor);

                        //////if (nomeExtraidoMatriz.Equals(nomeExtraidoParametro))
                        //////{
                        //////    count++;
                        //////}


                    }
                }

                // Validar se o número de aulas já agendadas é menor que o máximo permitido
                return count < maxAulasPermitidas;
            }
            else
            {
                Console.WriteLine("Informações sobre aulas por ano não encontradas para a sigla da matéria.");
                return false;
            }
        }

        public int ExtrairAnoEscolarDeTurmaIndex(int turmaIndex)
        {
            // Supõe que ListTurmasProf é acessível e mapeia índices para nomes de turma
            string nomeTurma = ListTurmasProf[turmaIndex].Nome;

            // Extrai o primeiro caractere numérico do nome da turma como o ano escolar
            int anoEscolar = 0;
            foreach (char c in nomeTurma)
            {
                if (char.IsDigit(c))
                {
                    anoEscolar = int.Parse(c.ToString());
                    break; // Interrompe o loop assim que o primeiro dígito é encontrado
                }
            }

            return anoEscolar;
        }


        public bool PodeAgendarProfessorNaColuna(string[,] matriz, int coluna, string nomeProfessor)
        {
            int count = 0; // Contador para o número de vezes que o nome do professor é encontrado na coluna

            int _turmaIndex = coluna;

            string E_Polivalente = ListTurmasProf.First(x => x.TurmaIndex == _turmaIndex).Polivalente;

            // Iterar por todas as linhas na coluna especificada para contar as ocorrências do nome do professor
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                if (matriz[linha, coluna] == nomeProfessor)
                {
                    count++; // Incrementar o contador se o nome do professor for encontrado
                }

                //if (ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]) ==
                //    ProfessorHelper.ExtrairNomeProfessor(nomeProfessor))
                //{
                //    count++;
                //}

            }

            //if (E_Polivalente == "SIM")
            //{
            //    // retornar zero para permitir muitos lancamentos na mesmo coluna
            //    // porque é turma polivalente.
            //    count = 0;
            //}

            // Retorna true se o professor foi agendado 0 ou 1 vezes, permitindo mais uma alocação se necessário
            // Retorna false se o professor já foi agendado 2 vezes ou mais, indicando que não há mais espaço para alocação
            return count < 2;
        }


        public bool PodeAgendarProfessorSeInexistenteNaColuna(string[,] matriz, int coluna, string nomeProfessor)
        {
            // Verifica todas as linhas na coluna especificada
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                // 3AULAS
                if (matriz[linha, coluna] == nomeProfessor)
                {
                    // Retorna false se encontrar o professor na coluna, indicando que não deve ser alocado novamente
                    return false;
                }
                

                ////////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
                ////////string nomeExtraidoParametro = ProfessorHelper.ExtrairNomeProfessor(nomeProfessor);

                ////////if (nomeExtraidoMatriz == nomeExtraidoParametro)
                ////////{
                ////////    // Retorna false se encontrar o professor na coluna, indicando que não deve ser alocado novamente
                ////////    return false;
                ////////}

            }

            // Retorna true se o professor não for encontrado na coluna, indicando que é possível alocá-lo
            return true;
        }


        public bool IsProfessorAgendadoNoHorario(string[,] matriz, int horario, string nomeProfessor)
        {
            // Verifica se o índice do horário está dentro dos limites da matriz
            if (horario < 0 || horario >= matriz.GetLength(0))
                return false;  // Horário inválido

            // Itera sobre todas as colunas válidas na linha especificada (horário)
            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
            {
                //if (matriz[horario, coluna] != null && matriz[horario, coluna].Equals(nomeProfessor))
                //    return true;  // Encontrou o professor no horário especificado

                if (ProfessorHelper.ExtrairNomeProfessor(matriz[horario, coluna]) != null &&
                    ProfessorHelper.ExtrairNomeProfessor(matriz[horario, coluna])
                        .Equals(ProfessorHelper.ExtrairNomeProfessor(nomeProfessor)))
                {
                    return true; // Encontrou o professor no horário especificado
                }

            }

            return false;  // Não encontrou o professor no horário especificado
        }


        public bool IsProfessorAgendadoNaColunaEspecifica(string[,] matriz, int coluna, string nomeProfessor)
        {
            int count = 0; // Contador para o número de vezes que o nome do professor é encontrado

            // Iterar por todas as linhas (horários) na coluna (turma) especificada
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                if (matriz[linha, coluna] == nomeProfessor)
                {
                    count++;
                }
            }

            // Retorna true se o professor foi agendado 0 ou 1 vezes, permitindo mais uma alocação se necessário
            // Retorna false se o professor já foi agendado 2 vezes, indicando que não há mais espaço para alocação
            return count < 2;
        }






        //****************************************************8 
        //ATENCAO CONVESI COM ELENICE BAKCUP
        //backpup nao apagar 
        public bool IsProfessorAgendadoNaTurma(string[,] matriz, int coluna, string nomeProfessor, string turma)
        {
            bool validacao = false;
            int count = 0; // Contador para o número de vezes que o nome do professor é encontrado
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                // 3AULAS
                if (matriz[linha, coluna] == nomeProfessor)
                {
                    count++;
                }
              

                ////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
                ////string nomeExtraidoParametro = ProfessorHelper.ExtrairNomeProfessor(nomeProfessor);

                ////if (nomeExtraidoMatriz == nomeExtraidoParametro)
                ////{
                ////    count++;
                ////}


            }

            //string E_Polivalente = ListTurmasProf.First(x => x.Nome == turma).Polivalente;
            var E_Polivalente = ListTurmasProf.FirstOrDefault(x => x.Nome == turma)?.Polivalente ?? "NAO";


            if (nomeProfessor == "ELENY")
            {
                string ACHEI = "ELENY";
            }

            if (dobradinha == true)
            {
                if (dobradinha)
                    if (count == 2)
                    {
                        validacao = true;
                    }
                if (count <= 1)
                {
                    validacao = false;
                }
            }
            else
            if (dobradinha == false)
            {
                if (count <= 2)
                {
                    validacao = false;
                }

            }

            // PERMITE FAZER DOBRADINHA SEMPRE QUE FOR POLIVALENTE
            //if (E_Polivalente == "SIM")
            //{
            //    // passei para false para ignorar a validacao ok 
            //    // desta forma vai poder repetir o professor na coluna
            //    validacao = false;
            //}

            return validacao;
        }

        public bool IsProfessorAgendadoNaCelulaEMesmoDaPesquisa(string[,] matriz, int horario, int turmaIndex, string nomeProfessor)
        {
            bool validacao = false;
            string professorCelula = matriz[horario, turmaIndex];
            if (professorCelula == nomeProfessor)
            {
                validacao = true;
            }
            return validacao;
        }


        public bool VerificaProfessorMesmoHorarioQualquerTurma(string[,] matriz, int horario, string nomeProfessor)
        {
            bool validacao = false;
            // Iterar sobre todas as colunas (turmas) na linha (horário) especificada
            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
            {
                string professorNaCelula = matriz[horario, coluna];
                ////if (matriz[horario, coluna] == nomeProfessor)
                ////{
                ////    validacao = true;  // Encontrou o professor neste horário em alguma turma
                ////    break;
                ////}
                ///

                if (ProfessorHelper.ExtrairNomeProfessor(matriz[horario, coluna]) ==
                  ProfessorHelper.ExtrairNomeProfessor(nomeProfessor))
                {
                    validacao = true;  // Encontrou o professor neste horário em alguma turma
                    break;
                }


            }

            return validacao;  // Professor não está agendado neste horário em nenhuma turma
        }


        public bool VerificaProfessorMesmoHorarioQualquerTurmaDestino(string[,] matriz, int horario, string nomeProfessor)
        {
            bool validacao = false;
            // Iterar sobre todas as colunas (turmas) na linha (horário) especificada
            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
            {
                string professorNaCelula = matriz[horario, coluna];
                if (matriz[horario, coluna] == nomeProfessor)
                {
                    validacao = true;  // Encontrou o professor neste horário em alguma turma
                    break;
                }
            }
            return validacao;  // Professor não está agendado neste horário em nenhuma turma
        }





        private bool AdicionarAula(string[,] matriz, string turma, string nomeProfessor, bool dobrarAula)
        {
            // Obtenção do índice da turma baseado na lista dinâmica de turmas
            int turmaIndex = Turmas.IndexOf(turma);
            if (turmaIndex == -1) return false; // Se a turma não for encontrada, retorna falso.

            for (int i = 0; i < matriz.GetLength(0); i++)
            {
                if (matriz[i, turmaIndex] == "")
                {
                    matriz[i, turmaIndex] = nomeProfessor; // Alocar professor na primeira linha disponível
                    if (dobrarAula && i + 1 < matriz.GetLength(0) && matriz[i + 1, turmaIndex] == "")
                    {
                        matriz[i + 1, turmaIndex] = nomeProfessor; // Dobrar a aula, se possível
                        return true;
                    }
                    else if (!dobrarAula)
                    {
                        return true;
                    }
                }
            }
            return false; // Se não houver espaço disponível ou não for possível dobrar a aula
        }


        public bool IsHorarioDisponivel(string[,] matriz, int horario, int turmaIndex)
        {
            // Verifica se a célula está vazia
            return string.IsNullOrEmpty(matriz[horario, turmaIndex]);
        }


        private bool ProfessorTemAulasRestantes(string nomeProfessor, string turma)
        {
            // Verifica se o professor tem aulas restantes para distribuir na turma especificada
            MovimentacaoTurma movimentacaoProfessor = movimentacoes.Find(m => m.NomeProfessor == nomeProfessor && m.Turma == turma);
            if (movimentacaoProfessor != null)
            {
                int aulasRestantes = movimentacaoProfessor.QtdAula - movimentacaoProfessor.QtdAulaAgendada;
                return aulasRestantes > 0;
            }
            return false;
        }



        //-------------------------RE   PROCESSAMENTO ------------------------------------------


        public void ReprocessarTurmas(Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> professores, List<MovimentacaoTurma> movimentacoes)
        {
            string[][,] matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };
            bool progresso;

            do
            {
                progresso = false;
                foreach (var movimentacao in movimentacoes.Where(m => m.QtdAula > m.QtdAulaAgendada).ToList())
                {
                    string turma = movimentacao.Turma;
                    string nomeProfessor = movimentacao.NomeProfessor;
                    var professorInfo = professores[nomeProfessor]; // Acesso às informações detalhadas do professor
                    int turmaIndex = Turmas.IndexOf(movimentacao.Turma); // Índice da turma na lista de turmas
                    int aulasNecessarias = movimentacao.QtdAula - movimentacao.QtdAulaAgendada; // Aulas que ainda precisam ser agendadas

                    int aulasAlocadas = TentarAlocarProfessorParaTurma(matrizes, nomeProfessor, turmaIndex, aulasNecessarias, movimentacoes, turma);

                    if (aulasAlocadas > 0)
                    {
                        progresso = true; // Indica que houve progresso no reprocessamento
                    }
                }
            } while (progresso); // Continua o processo enquanto estiver
        }

        private int TentarAlocarProfessorParaTurma(string[][,] matrizes, string nomeProfessor, int turmaIndex, int aulasNecessarias, List<MovimentacaoTurma> movimentacoes, string turma)
        {
            int aulasAlocadas = 0;
            int idxDiaSemanaMatriz = 0;

            ValidadorDeAgendamento validadorDeAgendamento = new ValidadorDeAgendamento(this);
            foreach (var matriz in matrizes)
            {

                for (int horario = 0; horario < matriz.GetLength(0); horario++) // Correção aqui: deve usar GetLength(0) para horários
                {
                    if (aulasAlocadas >= aulasNecessarias) break;
                    string Sigla;
                    int AnoEscolar;
                    if (ValidarAlocacaoProfessorParaReaReagendar(matriz, idxDiaSemanaMatriz, horario, turmaIndex, nomeProfessor, out Sigla, out AnoEscolar, turma))
                    {
                        if (idxDiaSemanaMatriz != 0)
                        { string naoigual = "zero"; }
                        ///novos tipos de validacao.
                        var resultados = validadorDeAgendamento.ValidarAgendamento(matriz, idxDiaSemanaMatriz, nomeProfessor, horario, turmaIndex);

                        bool podeAlocar = true;
                        foreach (var resultado in resultados.Values)
                        {
                            if (!resultado)
                            {
                                podeAlocar = false;
                                break;
                            }
                        }

                        if (podeAlocar)
                        {
                            AlocarProfessor(matriz, idxDiaSemanaMatriz, horario, turmaIndex, nomeProfessor, movimentacoes, aulasNecessarias, Sigla, AnoEscolar);
                            aulasAlocadas++;
                        }
                    }
                }
                if (aulasAlocadas >= aulasNecessarias) break;

                idxDiaSemanaMatriz++;
                if (idxDiaSemanaMatriz == 3)
                {
                    //*** nao vou deixar processar as outras turmas. //**
                    break;
                }
            }
            return aulasAlocadas;
        }

        private bool TentarAlocarNoHorario(string[,] matriz, int idxDiaSemanaMatriz, int horario, int turmaIndex, string nomeProfessor, string turma, List<MovimentacaoTurma> movimentacoes, int aulasNecessarias, ref int aulasAlocadas)
        {
            string Sigla;
            int AnoEscolar;
            if (ValidarAlocacaoProfessorParaReaReagendar(matriz, idxDiaSemanaMatriz, horario, turmaIndex, nomeProfessor, out Sigla, out AnoEscolar, turma))
            {
                AlocarProfessor(matriz, idxDiaSemanaMatriz, horario, turmaIndex, nomeProfessor, movimentacoes, aulasNecessarias, Sigla, AnoEscolar);
                aulasAlocadas++;
                return true;
            }
            return false;
        }




        // popular a classe de movimentacoes 
        public async void InicializarMovimentacoes()
        {
            // Limpa a lista de movimentações existente ou inicializa uma nova
            movimentacoes = new List<MovimentacaoTurma>();

            // Iterar sobre cada professor no dicionário de professores
            foreach (var professorEntry in Professores)
            {
                string nomeProfessor = professorEntry.Key;
                var (aulasPorTurma, distribuicaoTurmas, _, _, _, _, _, _, _) = professorEntry.Value;

                // Iterar sobre cada matéria e as turmas associadas a essa matéria
                 foreach (var materiaTurmas in distribuicaoTurmas)
                {
                    string siglaMateria = materiaTurmas.Key;
                    List<string> turmas = materiaTurmas.Value;

                    // Obter as configurações de aulas por ano para a matéria atual
                    if (materiaAulasPorAno.TryGetValue(siglaMateria, out var aulasPorAno))
                    {
                        // Iterar sobre cada turma
                        foreach (string turma in turmas)
                        {
                            int anoEscolar = ExtrairAnoEscolar(turma); // Supõe-se uma função que extraia o ano escolar baseado no nome da turma
                            int qtdAula = aulasPorAno.TryGetValue(anoEscolar, out var qtd) ? qtd : (aulasPorAno.TryGetValue(0, out var qtdDefault) ? qtdDefault : 0);

                            // Criar e adicionar a nova movimentação para a turma
                            MovimentacaoTurma movimentacao = new MovimentacaoTurma(nomeProfessor, turma, qtdAula, 0, 0, siglaMateria, anoEscolar, _appState);
                            movimentacoes.Add(movimentacao);
                         
                        }
                    }
                }
            }
        }

        // Função auxiliar para extrair o ano escolar do nome da turma
        private int ExtrairAnoEscolar(string turma)
        {
            // Implementação básica, assumindo que o nome da turma começa com o ano (e.g., "6W")
            if (int.TryParse(turma.Substring(0, 1), out int anoEscolar))
            {
                return anoEscolar;
            }
            return 0; // Retorna 0 se não for possível extrair um ano válido
        }


        //public async Task ProcessarCodigo(string codigoProfessores)
        //{
        //    var referenciasValidas = AppDomain.CurrentDomain.GetAssemblies()
        //        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));

        //    var options = ScriptOptions.Default
        //        .WithReferences(referenciasValidas)
        //        .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text");

        //    // Executar o script e obter o resultado
        //    var resultado = await CSharpScript.EvaluateAsync<Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>>(codigoProfessores, options);

        //    // Substituir a lista de professores atual pela nova lista processada
        //    Professores = resultado;

        //    Console.WriteLine($"Número de professores: {Professores.Count}");
        //}


        //public async Task GerarCodigoProfessores(List<Professor> listaProfessores)
        //{
        //    // Inicializa a string que armazenará o código gerado
        //    var codigoProfessores = new StringBuilder();

        //    // Adiciona a linha inicial que cria o dicionário Professores
        //    codigoProfessores.AppendLine("Professores = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();");

        //    // Itera sobre cada professor na lista e gera o código correspondente
        //    foreach (var prof in listaProfessores)
        //    {
        //        // Monta a string da DistribuicaoTurmas
        //        var distribuicaoTurmas = new StringBuilder();
        //        foreach (var turma in prof.DistribuicaoTurmas)
        //        {
        //            var turmasList = string.Join("\", \"", turma.Value);
        //            distribuicaoTurmas.Append($"{{ \"{turma.Key}\", new List<string> {{ \"{turmasList}\" }} }}, ");
        //        }

        //        // Remove a última vírgula e espaço adicionados
        //        if (distribuicaoTurmas.Length > 0)
        //        {
        //            distribuicaoTurmas.Length -= 2;
        //        }

        //        // Adiciona o código para adicionar o professor ao dicionário Professores
        //        codigoProfessores.AppendLine($@"
        //        Professores.Add(
        //            ""{prof.Nome.ToUpper()}"", 
        //            ({prof.AulasPorTurma}, 
        //            new Dictionary<string, List<string>> {{ {distribuicaoTurmas} }},
        //            ""{prof.DiasNaoTrabalha}"", 
        //            ""{prof.NomeAtividadeDescolar}"", 
        //            {prof.DobrarAula.ToString().ToLower()}, 
        //            ""{prof.Restricoes}"", 
        //            ""{prof.AtividadeAtpc}"", 
        //            ""{prof.DiasAtpc}"", 
        //            {prof.Prioridade.ToString().ToLower()})
        //        );
        //    ");
        //    }

        //    // Exibe o código gerado (para depuração)
        //    Console.WriteLine(codigoProfessores.ToString());

        //    // Configurações do ScriptOptions
        //    var referenciasValidas = AppDomain.CurrentDomain.GetAssemblies()
        //        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));

        //    var options = ScriptOptions.Default
        //        .WithReferences(referenciasValidas)
        //        .WithImports("System", "System.Collections.Generic", "System.Linq");

        //    // Executar o código gerado dinamicamente
        //    await CSharpScript.RunAsync(codigoProfessores.ToString(), options, globals: this);
        //}

        public async Task<Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)>> InicializarProfessores(int idEscola, string periodo, int ativo, int anoLetivo)
        // public async Task InicializarProfessores(int idEscola, string periodo, int ativo ,int anoLetivo )
        {
            /*
             Tipo de Chave (string): O dicionário é indexado por uma string, que, no contexto educacional, provavelmente representa o nome do professor.
            Tipo de Valor (Tupla): Cada valor no dicionário é uma tupla com vários campos de tipos diferentes:
            int AulasPorTurma: Um inteiro que representa o número de aulas que o professor ministra por turma.
            Dictionary<string, List<string>> DistribuicaoTurmas: Um dicionário aninhado onde a chave é uma string (provavelmente o nome da disciplina) e o valor é uma lista de strings (provavelmente os códigos ou nomes das turmas que o professor leciona).
            string DiasNaoTrabalha: Uma string que descreve os dias em que o professor não trabalha.
            string NomeAtividadeDescolar: Uma string que pode representar alguma atividade escolar especial associada ao professor, fora das aulas regulares.
            bool DobrarAula: Um booleano que indica se o professor tem permissão ou a prática de dobrar aulas.
            string Restricoes: Uma string que detalha restrições adicionais que podem aplicar-se ao professor, como restrições de horário ou de sala.
            string AtividadeAtpc: Uma string que descreve as atividades de ATPC (Atribuição de Tempo de Preparação e Coordenação) que o professor realiza.
            string DiasAtpc: Uma string que lista os dias nos quais o professor realiza atividades de ATPC.
            bool Prioridade: Um booleano que indica se o professor tem alguma prioridade especial na distribuição de horários ou recursos.
            OpcoesProfessor: objeto que guarda as opcoes do professor 
                     */
            // professores = new Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)>();

            /* GRADE DE HORARIO ATUAL 
            // Adiciona dados dos professores
            professores.Add("AMANDA", (12, new Dictionary<string, List<string>> { { "INF", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,TER5,TER6,QUA1,QUI5,QUI6", "", true, "", "", "", false));
            professores.Add("ANDERSON", (24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "QUI6,SEX6", "", true, "", "", "", false));
            professores.Add("DANIELE", (24, new Dictionary<string, List<string>> { { "HIS", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "SEG1,SEX1", "", true, "", "", "", false));
            professores.Add("DIEGO", (24, new Dictionary<string, List<string>> { { "ING", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEX6", "", true, "", "", "", false));
            professores.Add("ELENICE", (18, new Dictionary<string, List<string>> { { "GEO", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "SEX5,SEX6", "", true, "", "", "", false));
            professores.Add("UISSA", (24, new Dictionary<string, List<string>> { { "EDU", new List<string> { "7W", "7X", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG6,QUI6", "", true, "", "", "", false));
            // MUDANCA professores.Add("HERICA", (25, new Dictionary<string, List<string>> { { "MAT", new List<string> { "8W", "8X", "9W", "9X", "9Y" } } }, "SEG1,SEX6", "", true, "", "", "", false));
            professores.Add("MARGARETH", (15, new Dictionary<string, List<string>> { { "MAT", new List<string> { "9W", "9X", "9Y" } } }, "QUA1,SEX1", "", true, "", "", "", false));
            professores.Add("IDELSON", (25, new Dictionary<string, List<string>> { { "MAT", new List<string> { "6W", "6X", "6Y", "7W", "7X" } } }, "TER1,QUI1", "", true, "", "", "", false));
            professores.Add("JUDENIR", (12, new Dictionary<string, List<string>> { { "EDU", new List<string> { "6W", "6X", "6Y", "7Y" } } }, "", "", true, "", "", "", false));
            // MUDANCA professores.Add("JULIANA", (25, new Dictionary<string, List<string>> { { "POR", new List<string> { "7Y", "8W", "8X", "8Y", "9W" } } }, "TER6", "QUI6", true, "", "", "", false));
            professores.Add("JULIANA", (10, new Dictionary<string, List<string>> { { "POR", new List<string> { "7Y", "8Y" } } }, "TER6,SEX6", "", true, "", "", "", false));
            professores.Add("KARINA", (24, new Dictionary<string, List<string>> { { "ART", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "TER1,QUI1", "", true, "", "", "", false));
            professores.Add("LUIS", (25, new Dictionary<string, List<string>> { { "POR", new List<string> { "6W", "6X", "6Y", "7W", "7X" } } }, "QUI1,QUI2", "", true, "", "", "", false));
            professores.Add("RAQUEL", (20, new Dictionary<string, List<string>> { { "MAT", new List<string> { "7Y", "8Y", "8W", "8X" } } }, "SEG5,SEG6,TER5,TER6,QUI5,QUI6,", "", true, "", "", "", false));
            professores.Add("RANIERE", (12, new Dictionary<string, List<string>> { { "LEI", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "", "", false, "", "", "", false));
            //MUDANCA SAIU DA ESCOLA        professores.Add("RENATA", (18, new Dictionary<string, List<string>> { { "HIS", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "TER1,QUI1", "", true, "", "", "", false));
            professores.Add("VINICIUS", (18, new Dictionary<string, List<string>> { { "HIS", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEX1", "", true, "", "", "", false));
            professores.Add("SAMIR", (25, new Dictionary<string, List<string>> { { "POR", new List<string> { "8W", "8X", "9Y", "9X", "9W" } } }, "SEX5,SEX6", "", true, "", "", "", false));
            // MUDANCA professores.Add("VIVIAN", (24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEG2", "", true, "", "", "", false));
            professores.Add("PARISOTO", (24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEG2", "", true, "", "", "", false));
            professores.Add("ZILRENE", (24, new Dictionary<string, List<string>> { { "GEO", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "QUA1,QUA2", "", true, "", "", "", false));
            */

            //#### GRUPO DE NOME QUE IREMOS DIVULGAR NO SITE

            //Professores = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();
            //Professores.Add("ALESSANDRA", (12, new Dictionary<string, List<string>> { { "INF", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,TER5,TER6,QUA1,QUI5,QUI6", "", true, "", "", "", false));
            //Professores.Add("ANDRE", (24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "QUI6,SEX6", "", true, "", "", "", false));
            //Professores.Add("DENISE", (24, new Dictionary<string, List<string>> { { "HIS", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "SEG1,SEX1", "", true, "", "", "", false));
            //Professores.Add("DIOGO", (24, new Dictionary<string, List<string>> { { "ING", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEX6", "", true, "", "", "", false));
            //Professores.Add("ALICE", (18, new Dictionary<string, List<string>> { { "GEO", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "SEX5,SEX6", "", true, "", "", "", false));
            //Professores.Add("LARISSA", (24, new Dictionary<string, List<string>> { { "EDU", new List<string> { "7W", "7X", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG6,QUI6", "", true, "", "", "", false));
            //Professores.Add("MARISTELA", (15, new Dictionary<string, List<string>> { { "MAT", new List<string> { "9W", "9X", "9Y" } } }, "QUA1,SEX1", "", true, "", "", "", false));
            //Professores.Add("IVAN", (25, new Dictionary<string, List<string>> { { "MAT", new List<string> { "6W", "6X", "6Y", "7W", "7X" } } }, "TER1,QUI1", "", true, "", "", "", false));
            //Professores.Add("JOAO", (12, new Dictionary<string, List<string>> { { "EDU", new List<string> { "6W", "6X", "6Y", "7Y" } } }, "", "", true, "", "", "", false));
            //Professores.Add("JULIA", (10, new Dictionary<string, List<string>> { { "POR", new List<string> { "7Y", "8Y" } } }, "TER6,SEX6", "", true, "", "", "", false));
            //Professores.Add("KATIA", (24, new Dictionary<string, List<string>> { { "ART", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "TER1,QUI1", "", true, "", "", "", false));
            //Professores.Add("LUIS", (25, new Dictionary<string, List<string>> { { "POR", new List<string> { "6W", "6X", "6Y", "7W", "7X" } } }, "QUI1,QUI2", "", true, "", "", "", false));
            //Professores.Add("RAQUEL", (20, new Dictionary<string, List<string>> { { "MAT", new List<string> { "7Y", "8Y", "8W", "8X" } } }, "SEG5,SEG6,TER5,TER6,QUI5,QUI6,", "", true, "", "", "", false));
            //Professores.Add("ROBERTO", (12, new Dictionary<string, List<string>> { { "LEI", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "", "", false, "", "", "", false));
            //Professores.Add("VITOR", (18, new Dictionary<string, List<string>> { { "HIS", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEX1", "", true, "", "", "", false));
            //Professores.Add("SANDRO", (25, new Dictionary<string, List<string>> { { "POR", new List<string> { "8W", "8X", "9Y", "9X", "9W" } } }, "SEX5,SEX6", "", true, "", "", "", false));
            //Professores.Add("PAULO", (24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEG2", "", true, "", "", "", false));
            //Professores.Add("ZILDA", (24, new Dictionary<string, List<string>> { { "GEO", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "QUA1,QUA2", "", true, "", "", "", false));
            //////Este método pode ser expandido para adicionar mais professores ou modificar a estrutura de como os professores são adicionados.



            /// ********** ANO 2025
            // IMPORTANTE, COPIA DE SEGURNCA
            // AJUSTANDO O CODIGO 02 / 01/ 2025 PROBLEMAS DE DUPLICIDADES NO CHAVES DOS NOME 
          
            using (var scope = _scopeFactory.CreateScope())
            {

                var professorService = scope.ServiceProvider.GetRequiredService<IProfessorService>();
                var opcaoHorarioService = scope.ServiceProvider.GetRequiredService<IOpcoesProfessorService>();
                var cadastroProfessor = scope.ServiceProvider.GetRequiredService<IProfessorNewCadastroService>();


                var listaProfessores = await professorService.GetProfessoresAsync(idEscola, periodo, ativo, anoLetivo, this.OrdenarMaiorNumeroOpcao1OuOpcao2);


                //int anoLetivo = 2024;
                //string periodo = "Manha";

                //int idEscola = 1;
                //var listaOpcaoHorario = await opcaoHorarioService.SelecionarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola);
                //var listCadastroProfessor = await cadastroProfessor.ObterTodosProfessoresAsync(idEscola, anoLetivo, periodo);
                //var context = scope.ServiceProvider.GetRequiredService<HorarioProfessorDbContext>();



                // Inicializa a string que armazenará o código gerado
                var codigoProfessores = new System.Text.StringBuilder();

                // Adiciona a linha inicial que cria o dicionário Professores
                codigoProfessores.AppendLine("var Professores = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();");

                // Limita a quantidade de professores a 3 registros
                var topProfessores = listaProfessores.ToList();

                // Itera sobre cada professor na lista e gera o código correspondente
                foreach (var prof in topProfessores)
                {
                    // Monta a string da DistribuicaoTurmas
                    var distribuicaoTurmas = new System.Text.StringBuilder();
                    foreach (var turma in prof.DistribuicaoTurmas)
                    {
                        var turmasList = string.Join("\", \"", turma.Value);
                        distribuicaoTurmas.Append($"{{ \"{turma.Key}\", new List<string> {{ \"{turmasList}\" }} }}, ");
                    }

                    // Remove a última vírgula e espaço adicionados
                    if (distribuicaoTurmas.Length > 0)
                    {
                        distribuicaoTurmas.Length -= 2;
                    }

                    // Adiciona o código para adicionar o professor ao dicionário Professores
                    codigoProfessores.AppendLine($@"
                        Professores.Add(
                            ""{prof.Nome.ToUpper()}"", 
                            ({prof.AulasPorTurma}, 
                            new Dictionary<string, List<string>> {{ {distribuicaoTurmas} }},
                            ""{prof.DiasNaoTrabalha}"", 
                            ""{prof.NomeAtividadeDescolar}"", 
                            {prof.DobrarAula.ToString().ToLower()}, 
                            ""{prof.Restricoes}"", 
                            ""{prof.AtividadeAtpc}"", 
                            ""{prof.DiasAtpc}"", 
                            {prof.Prioridade.ToString().ToLower()})
                        );
                    ");
                }

                // Imprime o código gerado para depuração
                //Console.WriteLine(codigoProfessores.ToString());

                // Executar o script gerado
                var referenciasValidas = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => MetadataReference.CreateFromFile(a.Location));

                var options = ScriptOptions.Default
                    .WithReferences(referenciasValidas)
                    .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text");

                // Executar o código gerado dinamicamente
                var script = CSharpScript.RunAsync(codigoProfessores.ToString(), options).Result;

                // Substituir a lista de professores atual pela nova lista processada
                Professores = script.GetVariable("Professores").Value as Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>;


                //Console.WriteLine($"Número de professores: {Professores.Count}");


                return Professores;
            }
            

        ////////////////    //VOU DESCONTINUAR ESTE CODIGO PORQUE VOU RESOLVER O PROBLEMA DOS NOMES COLOCANDO ELENICE GEO E ELENICE HIS
        ////////////////    using (var scope = _scopeFactory.CreateScope())
        ////////////////    {
        ////////////////        var professorService = scope.ServiceProvider.GetRequiredService<IProfessorService>();
        ////////////////        var listaProfessores = await professorService.GetProfessoresAsync(idEscola, periodo, ativo, anoLetivo, this.OrdenarMaiorNumeroOpcao1OuOpcao2);

        ////////////////        // Inicializa a string que armazenará o código gerado
        ////////////////        var codigoProfessores = new System.Text.StringBuilder();

        ////////////////        // Adiciona a linha inicial que cria o dicionário Professores
        ////////////////        codigoProfessores.AppendLine("var Professores = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();");

        ////////////////        // Dicionário auxiliar para rastrear nomes duplicados
        ////////////////        var contadorChaves = new Dictionary<string, int>();

        ////////////////        // Itera sobre cada professor na lista e gera o código correspondente
        ////////////////        foreach (var prof in listaProfessores)
        ////////////////        {
        ////////////////            // Monta a string da DistribuicaoTurmas
        ////////////////            var distribuicaoTurmas = new System.Text.StringBuilder();
        ////////////////            foreach (var turma in prof.DistribuicaoTurmas)
        ////////////////            {
        ////////////////                var turmasList = string.Join("\", \"", turma.Value);
        ////////////////                distribuicaoTurmas.Append($"{{ \"{turma.Key}\", new List<string> {{ \"{turmasList}\" }} }}, ");
        ////////////////            }

        ////////////////            // Remove a última vírgula e espaço adicionados
        ////////////////            if (distribuicaoTurmas.Length > 0)
        ////////////////            {
        ////////////////                distribuicaoTurmas.Length -= 2;
        ////////////////            }

        ////////////////            // Gera uma chave única temporária se necessário
        ////////////////            string nomeOriginal = prof.Nome.ToUpper();
        ////////////////            string chave = nomeOriginal;

        ////////////////            if (contadorChaves.ContainsKey(nomeOriginal))
        ////////////////            {
        ////////////////                contadorChaves[nomeOriginal]++;
        ////////////////                chave = $"{nomeOriginal}_{contadorChaves[nomeOriginal]}";
        ////////////////            }
        ////////////////            else
        ////////////////            {
        ////////////////                contadorChaves[nomeOriginal] = 0;
        ////////////////            }

        ////////////////            // Adiciona o código para adicionar o professor ao dicionário Professores
        ////////////////            codigoProfessores.AppendLine($@"
        ////////////////    Professores.Add(
        ////////////////        ""{chave}"", 
        ////////////////        ({prof.AulasPorTurma}, 
        ////////////////        new Dictionary<string, List<string>> {{ {distribuicaoTurmas} }},
        ////////////////        ""{prof.DiasNaoTrabalha}"", 
        ////////////////        ""{prof.NomeAtividadeDescolar}"", 
        ////////////////        {prof.DobrarAula.ToString().ToLower()}, 
        ////////////////        ""{prof.Restricoes}"", 
        ////////////////        ""{prof.AtividadeAtpc}"", 
        ////////////////        ""{prof.DiasAtpc}"", 
        ////////////////        {prof.Prioridade.ToString().ToLower()})
        ////////////////    );
        ////////////////");
        ////////////////        }

        ////////////////        // Imprime o código gerado para depuração
        ////////////////        // Console.WriteLine(codigoProfessores.ToString());

        ////////////////        // Executar o script gerado dinamicamente
        ////////////////        var referenciasValidas = AppDomain.CurrentDomain.GetAssemblies()
        ////////////////            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
        ////////////////            .Select(a => MetadataReference.CreateFromFile(a.Location));

        ////////////////        var options = ScriptOptions.Default
        ////////////////            .WithReferences(referenciasValidas)
        ////////////////            .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text");

        ////////////////        // Executar o script gerado dinamicamente
        ////////////////        var script = CSharpScript.RunAsync(codigoProfessores.ToString(), options).Result;

        ////////////////        // Substituir a lista de professores atual pela nova lista processada
        ////////////////        var dicionarioComChaves = script.GetVariable("Professores").Value as Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>;

        ////////////////        // Criar o dicionário final com os nomes originais (removendo os sufixos)
        ////////////////        var dicionarioFinal = new Dictionary<string, (int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool)>();

        ////////////////        foreach (var kvp in dicionarioComChaves)
        ////////////////        {
        ////////////////            // Remove o sufixo, se houver, e mantém os valores
        ////////////////            var nomeOriginal = kvp.Key; //.Contains('_') ? kvp.Key.Split('_')[0] : kvp.Key;

        ////////////////            // Adiciona ao dicionário final com o nome original
        ////////////////            dicionarioFinal.Add(nomeOriginal, kvp.Value);
        ////////////////        }

        ////////////////        // Substituir pelo dicionário final
        ////////////////        Professores = dicionarioFinal;

        ////////////////        return Professores;
        ////////////////    }





        }



        // passo 3 ------=========== / ==========// REMANEJAMENTO DOS PROFESSORES
        public bool ValidarAlocacaoProfessorParaRealocacao(string[,] matriz, int horario, int turmaIndex, string nomeProfessor, out string Sigla, out int AnoEscolar, string turma)
        {
            Sigla = string.Empty;
            AnoEscolar = 0;
            // Verifica se o professor já está agendado no mesmo horário em qualquer turma
            //if (IsProfessorAgendadoNoHorario(matriz, horario, nomeProfessor))
            //{
            //    return false;  // Falha na validação devido a conflito
            //}

            // Verifica se o professor já está agendado na mesma turma em qualquer horário
            if (IsProfessorAgendadoNaTurma(matriz, turmaIndex, nomeProfessor, turma))
            {
                return false;  // Falha na validação devido a conflito
            }

            //// Verifica se o horário na matriz está disponível
            //if (!IsHorarioDisponivel(matriz, horario, turmaIndex))
            //{
            //    return false;  // Falha na validação porque o horário já está ocupado
            //}

            //// Verifica se o professor ainda tem aulas a serem distribuídas
            //if (!ProfessorTemAulasRestantes(nomeProfessor, turma))
            //{
            //    return false;  // Falha na validação porque o professor não tem mais aulas a serem distribuídas
            //}


            // Passou todas as validações
            return true;
        }



        public bool TentarRemoverERealocar(Dictionary<string, List<(int hora, int turma)>> horariosVagos)
        {
            bool alteracoes = false;
            foreach (var professor in Professores.Keys)
            {
                foreach (var dia in new string[] { "Segunda-feira", "Terca-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    string[,] matriz = GetMatrizPorDia(dia);
                    for (int hora = 0; hora < matriz.GetLength(0); hora++)
                    {
                        for (int turma = 0; turma < matriz.GetLength(1); turma++)
                        {
                            if (matriz[hora, turma] == professor)
                            {
                                // Verifica se o professor pode ser realocado em algum horário vago
                                if (horariosVagos.ContainsKey(dia) && horariosVagos[dia].Any())
                                {
                                    var novoHorario = horariosVagos[dia].First();
                                    if (TentarAlocarHorario(professor, novoHorario, matriz))
                                    {
                                        matriz[hora, turma] = ""; // Remove o professor do horário antigo
                                        horariosVagos[dia].Remove(novoHorario);
                                        horariosVagos[dia].Add((hora, turma)); // Adiciona o horário antigo aos vagos
                                        alteracoes = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return alteracoes;
        }


        private string[,] GetMatrizPorDia(string dia)
        {
            switch (dia)
            {
                case "Segunda-feira":
                    return matrizHorariosSegunda;
                case "Terca-feira":
                    return matrizHorariosTerca;
                case "Quarta-feira":
                    return matrizHorariosQuarta;
                case "Quinta-feira":
                    return matrizHorariosQuinta;
                case "Sexta-feira":
                    return matrizHorariosSexta;
                default:
                    throw new ArgumentException("Dia inválido");
            }
        }




        public bool RevalidarMatrizes()
        {
            foreach (var dia in new string[] { "Segunda-feira", "Terca-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
            {
                string[,] matriz = GetMatrizPorDia(dia);
                for (int hora = 0; hora < matriz.GetLength(0); hora++)
                {
                    var professoresNaHora = new HashSet<string>();
                    for (int turma = 0; turma < matriz.GetLength(1); turma++)
                    {
                        string professor = matriz[hora, turma];
                        if (!string.IsNullOrEmpty(professor))
                        {
                            if (professoresNaHora.Contains(professor))
                            {
                                return false; // Conflito encontrado
                            }
                            professoresNaHora.Add(professor);
                        }
                    }
                }
            }
            return true;
        }




        public void IniciarProcessoDeAlocacao()
        {
            //---3
            // Primeiro, analisamos as necessidades com base nas movimentações atuais.
            IdentificarTurmasSemAgendamentoCompleto();

            // Tenta realizar as trocas usando os horários vagos disponíveis inicialmente.
            ReagendarHorariosNaoDistribuidos();



            // Agora tentamos remover e realocar horários globalmente onde possível para otimizar a alocação.
            // Chamada sem parâmetros
            //if (TentarRemoverERealocarGlobal())
            //{
            //    ReagendarHorariosNaoDistribuidos();
            //}

            //---4  
            ProcessarRealoacoes();

            ///5
            ProcessarRealoacoesColuna(Professores);


            //// 6 Criado para resolver o problema quando o gride é dobradinha
            ProcessarRealoacoes();



            ////7 criado para resolver o problema do gride dobradinha
            ProcessarRealoacoesColuna(Professores);


            ////9 criado para resolver o problema do gride dobradinha
            ProcessarRealoacoesColuna(Professores);

            ////8 criado para resolver o problema do gride dobradinha
            ProcessarRealoacoes();

            ProcessarRealoacoesColuna(Professores);

            ProcessarRealoacoes();


            ProcessarRealoacoesColuna(Professores);


            // resolve a questao de professores que sai de uma coluna para outra 
            UnirProfessoresAgendadoDeHorarios();



            //TentarRemoverERealocarGlobal();

            // Finalmente, verificamos se as matrizes de horários estão consistentes após todas as alterações.
            if (RevalidarMatrizes())
            {
                Console.WriteLine("Todas as alocações foram realizadas com sucesso e sem conflitos.");
            }
            else
            {
                Console.WriteLine("Foram detectados conflitos ou problemas nas alocações de horários.");
            }


        }




        public void ProcessarRealoacoes()
        {
            string[][,] matrizes = {
        matrizHorariosSegunda,
        matrizHorariosTerca,
        matrizHorariosQuarta,
        matrizHorariosQuinta,
        matrizHorariosSexta
        };
            Contabilizar_Aulas contabAulas = new Contabilizar_Aulas(this);

            // VERIFICAR QUANTAS TURMAS AINDA FALTA SER DISTRIBUIDAS 
            // ######################################################################
            List<ContagemAulasProfessor> LerMatrizLocalizarAgendamentoFaltantes = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> GerarListaTodasAulasProfessor = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> agendamentosFaltantes = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);



            //ImprimirRelatorioDeContagemDeAulas(resultados);

            LerMatrizLocalizarAgendamentoFaltantes = contabAulas.ProcessarMatrizesEIdentificarTurmasFaltantes(matrizes).OrderBy(x => x.NomeProfessor).ToList();
            //ImprimirRelatorioDeContagemDeAulas(resultados);

            GerarListaTodasAulasProfessor = contabAulas.PreencherTabelaContagemAulasProfessor().OrderBy(x => x.NomeProfessor).ToList();
            // ImprimirRelatorioDeContagemDeAulas(resultados);

            agendamentosFaltantes = contabAulas.IdentificarAgendamentosFaltantes(GerarListaTodasAulasProfessor, LerMatrizLocalizarAgendamentoFaltantes);


            var GerandoNecessidadeProfessor = contabAulas.PreencherNecessidadesProfessores(agendamentosFaltantes);


            necessidadesProfessores = GerandoNecessidadeProfessor;





            int maxIteracoes = 100; // Máximo de iterações para evitar loops infinitos
            int iteracaoAtual = 0;

            bool houveMudanca;

            localNecessidadeProfessor = necessidadesProfessores
                                        .Where(kvp => kvp.Value.QtdAula > 0)
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            //limpa a variavel necessidadesProfessores
            necessidadesProfessores = new Dictionary<string, NecessidadeProfessor>();
            do
            {
                int count = localNecessidadeProfessor.Values.Count(n => n.QtdAula > 0);
                if (count == 0)
                {

                    if (localNecessidadeProfessor.Count == 0)
                    {
                        string chegou = string.Empty;
                        localNecessidadeProfessor = necessidadesProfessores;
                        //.OrderBy(kvp => kvp.Value.necessidadesPendentes) // Ordena pelo campo Turma de NecessidadeProfessor
                        //.ToDictionary(item => item.Key, item => item.Value);
                        necessidadesProfessores = new Dictionary<string, NecessidadeProfessor>();
                        break;
                    }
                }

                houveMudanca = false;

                if (TentarRemoverERealocarGlobal(localNecessidadeProfessor))
                    houveMudanca = true;
                iteracaoAtual++;
            } while (houveMudanca && iteracaoAtual < maxIteracoes);

            if (iteracaoAtual >= maxIteracoes)
            {
                Console.WriteLine("O processo de realocação atingiu o limite de iterações sem resolver todas as necessidades.");
            }
            else
            {

                Console.WriteLine("O processo de realocação foi concluído com sucesso.");
            }


            necessidadesProfessores = localNecessidadeProfessor
             .Where(kvp => kvp.Value.QtdAula > 0)
             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


        }



        public async Task ProcessarRealoacoesColuna(Dictionary<string, (int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)> professores)
        {
            // ******     atencao a      O r d e m    foi i n v e r t i d a   para melhorar o processamento 
            //    string[][,] matrizes = {
            //matrizHorariosSexta,
            //matrizHorariosQuinta,
            //matrizHorariosQuarta,
            //matrizHorariosTerca,
            //matrizHorariosSegunda
            //    };

            string[][,] matrizes = {
        matrizHorariosSegunda,
        matrizHorariosTerca,
        matrizHorariosQuarta,
        matrizHorariosQuinta,
        matrizHorariosSexta
        };

            // Nomes dos dias da semana correspondentes às matrizes, também invertidos
            //        string[] diasDaSemana = {
            //    "Sexta-feira",
            //    "Quinta-feira",
            //    "Quarta-feira",
            //    "Terça-feira",
            //    "Segunda-feira"
            //};

            //string[] diasDaSemana = {
            //"Segunda-feira",
            //"Terça-feira",
            //"Quarta-feira",
            //"Quinta-feira",
            //"Sexta-feira"
            //};

            string[] diasDaSemana = {
        "Segunda-feira",
        "Terça-feira",
        "Quarta-feira",
        "Quinta-feira",
        "Sexta-feira"
        };

            Contabilizar_Aulas contabAulas = new Contabilizar_Aulas(this);

            // VERIFICAR QUANTAS TURMAS AINDA FALTA SER DISTRIBUIDAS 
            // ######################################################################
            List<ContagemAulasProfessor> LerMatrizLocalizarAgendamentoFaltantes = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> GerarListaTodasAulasProfessor = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> agendamentosFaltantes = contabAulas.ProcessarMatrizesEContabilizarAulas(matrizes);



            //ImprimirRelatorioDeContagemDeAulas(resultados);

            LerMatrizLocalizarAgendamentoFaltantes = contabAulas.ProcessarMatrizesEIdentificarTurmasFaltantes(matrizes).OrderBy(x => x.NomeProfessor).ToList();
            //ImprimirRelatorioDeContagemDeAulas(resultados);

            GerarListaTodasAulasProfessor = contabAulas.PreencherTabelaContagemAulasProfessor().OrderBy(x => x.NomeProfessor).ToList();
            // ImprimirRelatorioDeContagemDeAulas(resultados);

            agendamentosFaltantes = contabAulas.IdentificarAgendamentosFaltantes(GerarListaTodasAulasProfessor, LerMatrizLocalizarAgendamentoFaltantes);


            var GerandoNecessidadeProfessor = contabAulas.PreencherNecessidadesProfessores(agendamentosFaltantes);


            necessidadesProfessores = GerandoNecessidadeProfessor;


            int maxIteracoes = 1; // Máximo de iterações para evitar loops infinitos
            int iteracaoAtual = 0;
            bool houveMudanca;

            localNecessidadeProfessor = necessidadesProfessores;

            do
            {
                int count = necessidadesProfessores.Values.Count(n => n.QtdAula > 0);
                if (count == 0)
                {

                    if (localNecessidadeProfessor.Count == 0) break;
                    string chegou = string.Empty;
                    localNecessidadeProfessor = necessidadesProfessores;
                    //.OrderBy(kvp => kvp.Value.Turma) // Ordena pelo campo Turma de NecessidadeProfessor
                    //.ToDictionary(item => item.Key, item => item.Value);

                    necessidadesProfessores = new Dictionary<string, NecessidadeProfessor>();
                }

                houveMudanca = false;

                HorarioRealocacaoPorTurma horarioRealocacaoPorTurma = new HorarioRealocacaoPorTurma(_appState);
                localNecessidadeProfessor = await horarioRealocacaoPorTurma.ProcessarRealoacoesPorColuna(matrizes, localNecessidadeProfessor);
                houveMudanca = true;

                iteracaoAtual++;
            } while (houveMudanca && iteracaoAtual < maxIteracoes);

            if (iteracaoAtual >= maxIteracoes)
            {
                Console.WriteLine("O processo de realocação atingiu o limite de iterações sem resolver todas as necessidades.");
            }
            else
            {
                Console.WriteLine("O processo de realocação foi concluído com sucesso.");
            }
            necessidadesProfessores = localNecessidadeProfessor
            .Where(kvp => kvp.Value.QtdAula > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }


        public bool TentarRemoverERealocarGlobal(Dictionary<string, NecessidadeProfessor> LocalnecessidadesProfessores)
        {
            bool alteracoes = false; // Variável para rastrear se houve alterações

            // Matrizes de horários por dia da semana
            string[][,] matrizes = {
        matrizHorariosSegunda,
        matrizHorariosTerca,
        matrizHorariosQuarta,
        matrizHorariosQuinta,
        matrizHorariosSexta
    };

            // Nomes dos dias da semana correspondentes às matrizes
            string[] diasDaSemana = {
        "Segunda-feira",
        "Terça-feira",
        "Quarta-feira",
        "Quinta-feira",
        "Sexta-feira"
    };

            // Iterar sobre todas as necessidades dos professores
            //   foreach (var necessidade in LocalnecessidadesProfessores.Values.Where(x => x.QtdAula > 0).OrderByDescending(m => m.NomeProfessor))
            foreach (var necessidade in LocalnecessidadesProfessores.Values.Where(x => x.QtdAula > 0).OrderBy(m => m.NomeProfessor))
            {



                // Encontra o TurmaIndex correspondente ao nome da turma da necessidade
                var turmaProf = ListTurmasProf.FirstOrDefault(t => t.Nome == necessidade.Turma);
                if (turmaProf == null)
                    continue; // Se não encontrar a turma, continua para o próximo professor



                while (necessidade.QtdAula > 0)
                {
                    bool realizouAlteracaoNoDia = false; // Controla se uma alteração foi feita no dia atual

                    int idxDiaSemanaMatriz = 0;
                    // Iterar sobre todos os dias da semana
                    for (int i = 0; i < diasDaSemana.Length; i++)
                    {
                        string[,] matrizDia = matrizes[i];
                        for (int hora = 0; hora < matrizDia.GetLength(0); hora++)
                        {
                            for (int turmaIndex = 0; turmaIndex < matrizDia.GetLength(1); turmaIndex++)
                            {

                                // Verifica se o índice da coluna corresponde ao TurmaIndex do professor
                                if (turmaIndex == turmaProf.TurmaIndex)
                                {

                                    //fazer uma validação aqui na sequencia da matriz//
                                    //##############################################
                                    //caso o professor ja tenha dois agendamentos na mesma
                                    //coluna na mesma matriz nao proceguir com o processamento
                                    //########################################################

                                    //////// Verifica se é possível agendar o professor na coluna especificada
                                    int contarVezesProfessorNaColuna = ContarVezesDoProfessorNaColunaNaMatriz(matrizDia, turmaIndex, necessidade.NomeProfessor);
                                    if (contarVezesProfessorNaColuna < 2)
                                    {

                                        if (necessidade.NomeProfessor == "LUIS" && necessidade.Turma == "7X" && necessidade.DiaDaSemana == 0 && hora == 2)
                                        {
                                            string prof = "testar";
                                        }


                                        string Sigla;
                                        int AnoEscolar;
                                        if (PodeDesalocar(matrizDia, hora, turmaIndex, necessidade.NomeProfessor, out Sigla, out AnoEscolar, necessidade.Turma))
                                        {
                                            //*** atencao tem que ser a primeira variavel
                                            // ** tem que pegar o valor da celula antes de fazer a troca
                                            // nao mudar ok 
                                            // Coloca a necessidade do professor no lugar do professor que cedeu o horário

                                            // ###### faz aqui o backup da celula antes de trocar  ####
                                            // Tenta realocar o professor desalocado


                                            // Obtem o nome da turma desalocada
                                            string turmaDesalocada = ListTurmasProf.First(t => t.TurmaIndex == turmaIndex).Nome;
                                            string NomeProfessorNaCelula = matrizDia[hora, turmaIndex];
                                            int qtdAulaDesalocada = 1;
                                            int HorarioDesalocado = hora;
                                            int DiaDaSemanaDesalocado = i; //i do for

                                            string disciplinasDeAmandaDesalocado = GetDisciplinaDeProfessor(NomeProfessorNaCelula);




                                            NecessidadeProfessor necessidadeProfessorRetirado = new NecessidadeProfessor(
                                                NomeProfessorNaCelula,
                                                turmaDesalocada,
                                                disciplinasDeAmandaDesalocado,
                                                AnoEscolar,
                                                qtdAulaDesalocada, // Quantidade de aulas a ser realocada
                                                i, // Dia da semana
                                                HorarioDesalocado,
                                                DiaDaSemanaDesalocado, // Tentativas de realocação
                                                _appState
                                            );

                                            int aulasAgendadas = GetQtdAulaAgendada(necessidade.NomeProfessor, turmaDesalocada, disciplinasDeAmandaDesalocado);

                                            //////////////// Desaloca o professor do horário atual
                                            //////////////matrizDia[hora, turmaIndex] = "";
                                            //////////////matrizDia[hora, turmaIndex] = necessidade.NomeProfessor;




                                            /// ATENCAO NESTE MOMENTO VOU GRAVAR A NECESSIDADE DO PROFESSOR NA CELULA
                                            ///  SE NAO PASSAR NA VALIDACAO NAO PODE DEIXAR O SISTEMA 
                                            ///  TENTAR GRAVAR A NECESSIDADE DO PROFESSOR RETIRADO CODIGO LOGO ABAIXO.
                                            ///  O SISTEMA DEVE SEGUIR SEM FAZER A REALOCACAO OK
                                            ///  
                                            ValidadorDeAgendamento validadorDeAgendamento = new ValidadorDeAgendamento(this);

                                            var resultados = validadorDeAgendamento.ValidarAgendamento(matrizDia, idxDiaSemanaMatriz, necessidade.NomeProfessor, hora, turmaIndex);
                                            bool podeAlocar = true;
                                            foreach (var resultado in resultados.Values)
                                            {
                                                if (!resultado)
                                                {
                                                    podeAlocar = false;
                                                    break;
                                                }
                                            }

                                            if (necessidade.NomeProfessor == "LUIS" && necessidade.Turma == "7X" && idxDiaSemanaMatriz == 3 && hora == 1)
                                            {
                                                string prof = "testar";
                                            }
                                            if (necessidade.NomeProfessor == "LUIS" && necessidade.Turma == "7X" && idxDiaSemanaMatriz == 3 && hora == 2)
                                            {
                                                string prof = "testar";
                                            }
                                            if (necessidade.NomeProfessor == "LUIS" && necessidade.Turma == "7X" && idxDiaSemanaMatriz == 3 && hora == 3)
                                            {
                                                string prof = "testar";
                                            }

                                            /*
                                             bool validaLocacaoTodasMatrizesOrigem = VerificarCapacidadeAulasPorTurma(matrizes, oportunidadesOrigem.TurmaIndex, necessidade.NomeProfessor);
                                             */


                                            contarVezesProfessorNaColuna = ContarVezesDoProfessorNaColunaNaMatriz(matrizDia, turmaIndex, necessidade.NomeProfessor);
                                            bool validaLocacaoTodasMatrizesOrigem = VerificarCapacidadeAulasPorTurma(matrizes, turmaIndex, necessidade.NomeProfessor);
                                            if (podeAlocar)
                                            {
                                                if ((contarVezesProfessorNaColuna < 2) && (validaLocacaoTodasMatrizesOrigem == true))
                                                {

                                                    if (idxDiaSemanaMatriz == 4)
                                                    {
                                                        string naoigualzero = "";
                                                    }

                                                    if (idxDiaSemanaMatriz == 4)
                                                    {
                                                        if (necessidade.NomeProfessor == "SAMIR")
                                                        {
                                                            if ((hora == 5) || (hora == 6))
                                                            {
                                                                {
                                                                    string E_SEXTA = "AQUI";
                                                                }

                                                            }
                                                        }
                                                    }

                                                    if (necessidade.NomeProfessor == "ELENICE")
                                                    {
                                                        string achei = "";
                                                    }


                                                    // Desaloca o professor do horário atual
                                                    matrizDia[hora, turmaIndex] = "";
                                                    matrizDia[hora, turmaIndex] = necessidade.NomeProfessor;

                                                    // Atualiza a quantidade de aulas restantes do professor
                                                    necessidade.QtdAula = --necessidade.QtdAula;


                                                    bool tentaGravar = TentaColocarOProfessorDesalocadoNumaVagaVazia(necessidadeProfessorRetirado);
                                                    if (!tentaGravar)
                                                    {
                                                        //string uniqueKey = $"{Scheduler.sequencial_Professor_desalocado++} {necessidade.NomeProfessor}";
                                                        //necessidadesPendentes.Add(uniqueKey, necessidadeProfessorRetirado);
                                                        var gerenciador = new GerenciadorNecessidades(this);
                                                        gerenciador.AdicionarOuAtualizarNecessidade(necessidadeProfessorRetirado.NomeProfessor, necessidadeProfessorRetirado.Turma, necessidadeProfessorRetirado.Sigla,
                                                                                                    necessidadeProfessorRetirado.AnoEscolar, necessidadeProfessorRetirado.QtdAula, necessidadeProfessorRetirado.DiaDaSemana,
                                                                                                    necessidadeProfessorRetirado.Horario, necessidadeProfessorRetirado.TentativasDeRealocacao);
                                                        //var verDentroOBJ = necessidadesPendentes;
                                                    }



                                                    alteracoes = true;
                                                    realizouAlteracaoNoDia = true;
                                                    break; // Sai do loop interno se a realocação for bem-sucedida
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (realizouAlteracaoNoDia) break; // Sai do loop de horas se uma alteração foi feita
                        }
                        if (realizouAlteracaoNoDia) break; // Sai do loop de dias se uma alteração foi feita

                        idxDiaSemanaMatriz++;
                    }

                    if (!realizouAlteracaoNoDia)
                    {
                        // Se nenhuma alteração foi possível, quebra o while
                        break;
                    }
                }
            }

            return alteracoes;
        }



        public bool TentaColocarOProfessorDesalocadoNumaVagaVazia(NecessidadeProfessor necessidadeProfessorRetirado)
        {
            NecessidadeProfessor necessidade = necessidadeProfessorRetirado; // Diretamente utilizar a referência passada

            bool alteracoes = false; // Variável para rastrear se houve alterações

            // Encontra o TurmaIndex correspondente ao nome da turma da necessidade
            var turmaProf = ListTurmasProf.FirstOrDefault(t => t.Nome == necessidade.Turma);
            if (turmaProf == null)
                return false; // Se não encontrar a turma, retorna false, indicando que nenhuma alocação foi possível

            // Matrizes de horários por dia da semana
            string[][,] matrizes = {
        matrizHorariosSegunda,
        matrizHorariosTerca,
        matrizHorariosQuarta,
        matrizHorariosQuinta,
        matrizHorariosSexta
    };

            // Nomes dos dias da semana correspondentes às matrizes
            string[] diasDaSemana = {
        "Segunda-feira",
        "Terça-feira",
        "Quarta-feira",
        "Quinta-feira",
        "Sexta-feira"
    };
            int idxDiaSemanaMatriz = 0;
            ValidadorDeAgendamento validadorDeAgendamento = new ValidadorDeAgendamento(this);
            // Iterar sobre todos os dias da semana
            for (int i = 0; i < diasDaSemana.Length; i++)
            {
                idxDiaSemanaMatriz = i;
                string[,] matrizDia = matrizes[i];
                for (int hora = 0; hora < matrizDia.GetLength(0); hora++)
                {
                    // Verifica apenas na coluna correspondente ao TurmaIndex do professor
                    if (VerificaEspacoVazioEAlocaProfessorCasoPossa(matrizDia, hora, turmaProf.TurmaIndex, necessidade.NomeProfessor, necessidade.Turma))
                    {


                        var resultados = validadorDeAgendamento.ValidarAgendamento(matrizDia, idxDiaSemanaMatriz, necessidade.NomeProfessor, hora, turmaProf.TurmaIndex);

                        bool podeAlocar = true;
                        foreach (var resultado in resultados.Values)
                        {
                            if (!resultado)
                            {
                                podeAlocar = false;
                                break;
                            }
                        }

                        if (necessidade.NomeProfessor == "ALESSANDRA" && necessidade.Turma == "9W" && idxDiaSemanaMatriz == 3 && hora == 1)
                        {
                            string prof = "testar";
                        }
                        if (necessidade.NomeProfessor == "LUIS" && necessidade.Turma == "7X" && idxDiaSemanaMatriz == 3 && hora == 2)
                        {
                            string prof = "testar";
                        }
                        if (necessidade.NomeProfessor == "LUIS" && necessidade.Turma == "7X" && idxDiaSemanaMatriz == 3 && hora == 3)
                        {
                            string prof = "testar";
                        }

                        if (podeAlocar)
                        {
                            int contarVezesProfessorNaColuna = ContarVezesDoProfessorNaColunaNaMatriz(matrizDia, turmaProf.TurmaIndex, necessidade.NomeProfessor);
                            if (contarVezesProfessorNaColuna < 2)
                            {

                                if (idxDiaSemanaMatriz == 4)
                                {
                                    string naoigualzero = "";
                                }

                                if (idxDiaSemanaMatriz == 4)
                                {
                                    if (necessidade.NomeProfessor == "LUIS")
                                    {
                                        if ((hora == 5) || (hora == 6))
                                        {
                                            {
                                                string E_SEXTA = "AQUI";
                                            }

                                        }
                                    }
                                }

                                if (necessidade.NomeProfessor == "LUIS")
                                {
                                    string achei = "";
                                }





                                // Coloca o professor no espaço vazio encontrado
                                matrizDia[hora, turmaProf.TurmaIndex] = necessidade.NomeProfessor;
                                alteracoes = true;
                                break; // Sai do loop interno se a alocação for bem-sucedida
                            }
                        }
                    }
                }
                if (alteracoes) break; // Sai do loop dos dias se a alocação foi realizada
            }
            return alteracoes;
        }


        public bool VerificaEspacoVazioEAlocaProfessorCasoPossa(string[,] matriz, int horario, int turmaIndex, string nomeProfessor, string turma)
        {
            //Verifica se o horário na matriz está disponível
            if (!IsHorarioDisponivel(matriz, horario, turmaIndex))
            {
                return false;  // Falha na validação porque o horário já está ocupado
            }

            // Chamada ao método ValidarAlocacaoProfessorParaReagendar para verificar se o professor pode ser desalocado
            //Sigla = string.Empty;
            //AnoEscolar = 0;
            // Verifica se o professor já está agendado no mesmo horário em qualquer turma
            if (IsProfessorAgendadoNoHorario(matriz, horario, nomeProfessor))
            {
                return false;  // Falha na validação devido a conflito
            }

            // Verifica se o professor já está agendado na mesma turma em qualquer horário
            if (IsProfessorAgendadoNaTurma(matriz, turmaIndex, nomeProfessor, turma))
            {
                return false;  // Falha na validação devido a conflito
            }


            // Verifica se o professor ainda tem aulas a serem distribuídas
            //if (!ProfessorTemAulasRestantes(nomeProfessor, turma))
            //{
            //    return false;  // Falha na validação porque o professor não tem mais aulas a serem distribuídas
            //}


            // Passou todas as validações
            return true;

        }


        //    public static bool ValidarAlocacaoProfessorParaReaReagendar(string[,] matriz, int horario, int turmaIndex, string nomeProfessor, out string Sigla, out int AnoEscolar, string turma)
        public bool PodeDesalocar(string[,] matriz, int horario, int turmaIndex, string nomeProfessor, out string Sigla, out int AnoEscolar, string turma)
        {
            // Chamada ao método ValidarAlocacaoProfessorParaReagendar para verificar se o professor pode ser desalocado
            Sigla = string.Empty;
            AnoEscolar = 0;

            if (IsProfessorAgendadoNaCelulaEMesmoDaPesquisa(matriz, horario, turmaIndex, nomeProfessor))
            {
                return false;  // Falha na validação devido a conflito o professor pesquisado e o mesmo na celula
            }


            // Verifica se o professor já está agendado no mesmo horário em qualquer turma
            if (IsProfessorAgendadoNoHorario(matriz, horario, nomeProfessor))
            {
                return false;  // Falha na validação devido a conflito
            }

            // Verifica se o professor já está agendado na mesma turma em qualquer horário
            if (IsProfessorAgendadoNaTurma(matriz, turmaIndex, nomeProfessor, turma))
            {
                return false;  // Falha na validação devido a conflito
            }


            // Verifica se o horário na matriz está disponível
            //if (!IsHorarioDisponivel(matriz, horario, turmaIndex))
            //{
            //    return false;  // Falha na validação porque o horário já está ocupado
            //}

            // Verifica se o professor ainda tem aulas a serem distribuídas
            //if (!ProfessorTemAulasRestantes(nomeProfessor, turma))
            //{
            //    return false;  // Falha na validação porque o professor não tem mais aulas a serem distribuídas
            //}


            // Passou todas as validações
            return true;

        }


        public void IdentificarTurmasSemAgendamentoCompleto()
        {
            List<MovimentacaoTurma> lstMov = movimentacoes.Where(m => m.QtdAulaCalculada > 0).OrderBy(x => x.NomeProfessor).ToList();

            foreach (var mov in lstMov)
            {
                string uniqueKey = $"{mov.NomeProfessor}_{mov.Turma}";

                if (necessidadesProfessores.TryGetValue(uniqueKey, out NecessidadeProfessor necessidadeExistente))
                {
                    // Atualiza os dados existentes
                    necessidadeExistente.QtdAula += mov.QtdAulaCalculada; // Incrementa a quantidade de aulas que faltam
                    necessidadeExistente.TentativasDeRealocacao++; // Incrementa o número de tentativas de realocação se necessário
                }
                else
                {
                    // Insere uma nova necessidade
                    necessidadesProfessores.Add(uniqueKey,
                        new NecessidadeProfessor(
                            mov.NomeProfessor,
                            mov.Turma,
                            mov.Sigla,
                            mov.AnoEscolar,
                            mov.QtdAulaCalculada, // Quantidade que falta
                            0, // diaDaSemana inicialmente zero
                            0, // horario inicialmente zero
                            0,  // tentativasDeRealocacao iniciar com zero
                            _appState
                        ));
                }
            }
        }



        private bool ProfessorPodeDarAulaNoDia(string nomeProfessor, string dia)
        {
            // Implementar a lógica que determina se o professor pode dar aula em um determinado dia
            return true; // Exemplo simplificado
        }




        public Dictionary<string, List<(int hora, int turma)>> EncontrarHorariosVagos()
        {
            Dictionary<string, string[,]> matrizesPorDia = new Dictionary<string, string[,]>() {
        {"Segunda-feira", matrizHorariosSegunda},
        {"Terca-feira", matrizHorariosTerca},
        {"Quarta-feira", matrizHorariosQuarta},
        {"Quinta-feira", matrizHorariosQuinta},
        {"Sexta-feira", matrizHorariosSexta}
    };

            Dictionary<string, List<(int hora, int turma)>> horariosVagosPorDia = new Dictionary<string, List<(int hora, int turma)>>();

            foreach (var diaMatriz in matrizesPorDia)
            {
                string dia = diaMatriz.Key;
                string[,] matriz = diaMatriz.Value;
                horariosVagosPorDia[dia] = new List<(int hora, int turma)>();

                for (int i = 0; i < matriz.GetLength(0); i++)
                {
                    for (int j = 0; j < matriz.GetLength(1); j++)
                    {
                        if (string.IsNullOrEmpty(matriz[i, j]))
                        {
                            horariosVagosPorDia[dia].Add((i, j));
                        }
                    }
                }
            }

            return horariosVagosPorDia;
        }



        public void ReagendarHorariosNaoDistribuidos()
        {
            string[][,] matrizes = {
        matrizHorariosSegunda,
        matrizHorariosTerca,
        matrizHorariosQuarta,
        matrizHorariosQuinta,
        matrizHorariosSexta
    };

            string[] diasDaSemana = {
        "Segunda-feira",
        "Terça-feira",
        "Quarta-feira",
        "Quinta-feira",
        "Sexta-feira"
    };
            int idxDiaSemanaMatriz = 0; // nao esta sendo usado


            // Dicionário para armazenar os horários vagos por dia, obtidos por EncontrarHorariosVagos()
            var horariosVagos = EncontrarHorariosVagos();

            foreach (var necessidade in necessidadesProfessores.Values)
            {
                if (necessidade.QtdAula > 0)
                {
                    // Tenta alocar as aulas do professor em qualquer dia disponível
                    for (int i = 0; i < matrizes.Length; i++) // Itera sobre todas as matrizes
                    {
                        int aulasAlocadas = 0;

                        for (int hora = 0; hora < matrizes[i].GetLength(0); hora++) // Itera sobre todos os horários
                        {
                            if (aulasAlocadas >= necessidade.QtdAula) break; // Sai do loop se todas as aulas forem alocadas

                            for (int turmaIndex = 0; turmaIndex < matrizes[i].GetLength(1); turmaIndex++) // Itera sobre todas as turmas
                            {
                                if (string.IsNullOrEmpty(matrizes[i][hora, turmaIndex])) // Checa se o horário está vago
                                {
                                    string Sigla;
                                    int AnoEscolar;



                                    if (ValidarAlocacaoProfessorParaReaReagendar(matrizes[i], idxDiaSemanaMatriz, hora, turmaIndex, necessidade.NomeProfessor, out Sigla, out AnoEscolar, diasDaSemana[i]))
                                    {
                                        if (necessidade.NomeProfessor == "DIEGO")
                                        {
                                            string prof = "DIEGO";
                                        }

                                        AlocarProfessor(matrizes[i], idxDiaSemanaMatriz, hora, turmaIndex, necessidade.NomeProfessor, movimentacoes, 1, Sigla, AnoEscolar);
                                        aulasAlocadas++;
                                        if (aulasAlocadas >= necessidade.QtdAula) break; // Sai do loop se todas as aulas forem alocadas
                                    }
                                }
                            }
                        }
                        if (aulasAlocadas >= necessidade.QtdAula) break; // Sai do loop de dias se todas as aulas forem alocadas

                        idxDiaSemanaMatriz++;
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------
        // regra para unir os professores que saem de uma torma e volta novamente.
        ////public void UnirProfessoresAgendadoDeHorarios()
        ////{
        ////    // Gerar possibilidades de troca
        ////    var listaPossibilidades = UnirProfessoresGerarPossibilidades();

        ////    // Processar e aplicar a melhor troca
        ////    UnirProfessores_ProcessarTrocas(listaPossibilidades);
        ////}


        public void UnirProfessoresAgendadoDeHorarios()
        {
            int maxIteracoes = 100; // Número máximo de iterações
            int iteracaoAtual = 0;

            while (iteracaoAtual < maxIteracoes)
            {
                // Gerar possibilidades de troca
                var listaPossibilidades = UnirProfessoresGerarPossibilidades();

                if (!listaPossibilidades.Any())
                {
                    Console.WriteLine($"Sem mais possibilidades após {iteracaoAtual} iterações.");
                    break; // Interrompe o loop se não houver mais possibilidades
                }

                foreach (var professoresPossibilidade in listaPossibilidades)
                {






                    // Processar e aplicar a melhor troca
                    UnirProfessores_ProcessarTrocas(listaPossibilidades);

                }
                                

                iteracaoAtual++;
            }

            Console.WriteLine($"Processamento concluído após {iteracaoAtual} iterações.");
        }



        private void UnirProfessores_ProcessarTrocas(List<PossibilidadeTroca> listaPossibilidades)
        {
            // Passo 1: Filtrar trocas válidas
            var trocasValidas = listaPossibilidades
                .Where(t => t.TrocaOrigemParaDestinoValida && t.TrocaDestinoParaOrigemValida)
                .ToList();

            if (!trocasValidas.Any())
            {
                Console.WriteLine("Nenhuma troca válida encontrada.");
                return;
            }

            // Passo 2: Priorizar trocas que formam dobradinha
            var trocasComDobradinha = trocasValidas.Where(t => t.FormaDobradinha).ToList();

            // Passo 3: Selecionar a melhor troca
            PossibilidadeTroca melhorTroca;

            if (trocasComDobradinha.Any())
            {
                // Se houver trocas com dobradinha, selecionar a primeira
                melhorTroca = trocasComDobradinha.First();
                Console.WriteLine("Troca com dobradinha selecionada.");
            }
            else
            {
                // Caso contrário, selecionar a primeira troca válida
                melhorTroca = trocasValidas.First();
                Console.WriteLine("Troca sem dobradinha selecionada.");
            }

            // Passo 4: Atualizar a matriz com a melhor troca
            AtualizarMatrizComTroca(melhorTroca);

            Console.WriteLine($"Troca realizada entre {melhorTroca.ProfessorOrigem} e {melhorTroca.ProfessorDestino}.");
        }




        ////private void AtualizarMatrizComTroca(PossibilidadeTroca troca)
        ////{
        ////    // Atualizar matriz com base na troca escolhida
        ////    var matriz = UnirProfessores_ObterMatrizPorDia(troca.Matriz);

        ////    // Atualizar origem e destino
        ////    matriz[troca.LinhaDestino, troca.ColunaDestino] = troca.ProfessorOrigem;
        ////    matriz[troca.LinhaOrigem, troca.ColunaOrigem] = troca.ProfessorDestino;

        ////    Console.WriteLine($"Troca realizada: {troca.ProfessorOrigem} -> {troca.Matriz} (Linha {troca.LinhaDestino}, Coluna {troca.ColunaDestino})");
        ////}


        private void AtualizarMatrizComTroca(PossibilidadeTroca troca)
        {
            // Obter a matriz de origem
            var matrizOrigem = UnirProfessores_ObterMatrizPorDia(troca.MatrizOrigem);
            if (matrizOrigem == null)
            {
                Console.WriteLine($"Erro: Matriz de origem para o dia {troca.MatrizOrigem} não foi encontrada.");
                return;
            }

            // Obter a matriz de destino
            var matrizDestino = UnirProfessores_ObterMatrizPorDia(troca.MatrizDestino);
            if (matrizDestino == null)
            {
                Console.WriteLine($"Erro: Matriz de destino para o dia {troca.MatrizDestino} não foi encontrada.");
                return;
            }



            if (troca.ProfessorOrigem == "ELENICE")
            {
                    string texto = "ELENICE";    
            }

            if (troca.ProfessorDestino == "ELENICE")
            {
                    string texto = "ELENICE";
            }

            //Atualizar as matrizes com base na troca
            matrizDestino[troca.LinhaDestino, troca.ColunaDestino] = troca.ProfessorOrigem;
            matrizOrigem[troca.LinhaOrigem, troca.ColunaOrigem] = troca.ProfessorDestino;

            Console.WriteLine($"Troca realizada: {troca.ProfessorOrigem} (Origem: {troca.MatrizOrigem}, Linha {troca.LinhaOrigem}, Coluna {troca.ColunaOrigem}) ↔ " +
                              $"{troca.ProfessorDestino} (Destino: {troca.MatrizDestino}, Linha {troca.LinhaDestino}, Coluna {troca.ColunaDestino}).");
        }








        public List<PossibilidadeTroca> UnirProfessoresGerarPossibilidades()
        {
            // Identifica conflitos relacionados ao professor "VITOR"
            var professoresConflitos = UnirProfessores_IdentificarConflitos().ToList();
           // .Where(x => x.Professor == "ELENICE").Take(1).ToList();
           // .Where(x => x.Professor == "ELENICE").Take(1).ToList();


            if (professoresConflitos.Count == 0)
            {
                Console.WriteLine("Nenhum conflito encontrado.");
                return new List<PossibilidadeTroca>(); // Retorna lista vazia se não houver conflitos
            }

            var listaPossibilidades = new List<PossibilidadeTroca>();

            foreach (var conflito in professoresConflitos)
            {
                // Mantém a matriz de origem fixa com base no dia do conflito
                var matrizOrigem = UnirProfessores_ObterMatrizPorDia(conflito.DiaSemana);
                int linhaOrigem = conflito.Horarios[0]; // Linha do professor na matriz de origem

                foreach (var diaSemana in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    var matrizDestino = UnirProfessores_ObterMatrizPorDia(diaSemana);

                    for (int linhaDestino = 0; linhaDestino < matrizDestino.GetLength(0); linhaDestino++)
                    {
                        if (linhaDestino ==10)
                        {
                            string dez = "";
                        }
                        if (conflito.Coluna == 10)
                        {
                            string dez = "";
                        }


                        var professorDestino = matrizDestino[linhaDestino, conflito.Coluna];


                        if (conflito.Professor == "ELENICE")
                        {
                            string ACHEI = "IDELSON";
                        }

                        if (professorDestino == "ELENICE")
                        {
                            string ACHEI = "IDELSON";
                        }


                        if (!string.IsNullOrEmpty(professorDestino) && professorDestino != conflito.Professor)
                        {
                            // Validação de troca origem -> destino e destino -> origem
                            bool trocaOrigemParaDestinoValida = UnirProfessores_ValidarTroca(
                                conflito.Professor,
                                professorDestino,
                                conflito.Coluna,
                                matrizOrigem,
                                matrizDestino,
                                linhaOrigem
                            );

                            //bool trocaDestinoParaOrigemValida = UnirProfessores_ValidarTroca(
                            //    professorDestino,
                            //    conflito.Professor,
                            //    conflito.Coluna,                                
                            //    matrizOrigem,
                            //    matrizDestino,
                            //    linhaDestino
                            //);

                            if (trocaOrigemParaDestinoValida) //&& trocaDestinoParaOrigemValida)
                            {
                                listaPossibilidades.Add(new PossibilidadeTroca
                                {
                                    MatrizOrigem = conflito.DiaSemana, // Nome da matriz de origem
                                    MatrizDestino = diaSemana, // Nome da matriz de destino
                                    LinhaOrigem = linhaOrigem,
                                    ColunaOrigem = conflito.Coluna,
                                    ProfessorOrigem = conflito.Professor,
                                    LinhaDestino = linhaDestino,
                                    ColunaDestino = conflito.Coluna,
                                    ProfessorDestino = professorDestino,
                                    TrocaOrigemParaDestinoValida = trocaOrigemParaDestinoValida,
                                    TrocaDestinoParaOrigemValida = true //trocaDestinoParaOrigemValida
                                });
                            }
                        }
                    }
                }
            }


            // **Nova etapa de filtragem**
            listaPossibilidades = UnirProfessores_FiltrarPossibilidades(listaPossibilidades);

            //  List<PossibilidadeTroca> listaPossibilidades2 = UnirProfessores_ValidarOpcoesProfessores(listaPossibilidades);
            listaPossibilidades = UnirProfessores_ValidarOpcoesProfessores(listaPossibilidades);

            //this.Professores.Where(x => x.Key == "ALESSANDRA");
            // Validar proximidade e exibir as possibilidades
            UnirProfessores_ValidarProximidade(listaPossibilidades); // dobradinha analise sim ou nao
            UnirProfessores_ExibirPossibilidades(listaPossibilidades);

            return listaPossibilidades;
        }




        public List<PossibilidadeTroca> UnirProfessores_ValidarOpcoesProfessores(List<PossibilidadeTroca> listaPossibilidades)
        {
            var listaFiltrada = new List<PossibilidadeTroca>();

            foreach (var possibilidade in listaPossibilidades)
            {
                // Obter professor origem e destino
                var professorOrigem = possibilidade.ProfessorOrigem;
                var professorDestino = possibilidade.ProfessorDestino;

                // Verificar se os professores estão na lista global
                if (!this.Professores.ContainsKey(professorOrigem) || !this.Professores.ContainsKey(professorDestino))
                {
                    Console.WriteLine($"Erro: Professor {professorOrigem} ou {professorDestino} não encontrado na lista global.");
                    continue;
                }




                // Obter as opções de bloqueio (como string concatenada) para ambos os professores
                string listOpcoesOrigem = this.Professores[professorOrigem].Item3; // Lista de bloqueios do professor de origem
                string listOpcoesDestino = this.Professores[professorDestino].Item3; // Lista de bloqueios do professor de destino

                // Gerar abreviações para verificar bloqueios
                // ATENCAO PRECISA FAZER A INVERSAO PARA COMPARAR SE PODE MUDAR OU NAO
                // NOTE QUE INVERTI A MATRIZ PARA COMPARAR SE PODE OU NAO 
                string abreviacaoOrigem = ObterAbreviacao(possibilidade.MatrizDestino, possibilidade.LinhaDestino);
                string abreviacaoDestino = ObterAbreviacao(possibilidade.MatrizOrigem, possibilidade.LinhaOrigem);


                    if (professorOrigem == "LUIS")
                    {
                        if(abreviacaoOrigem.Contains("SEX3"))
                        {
                            string texto = "DANIELI";
                        }
                    
                    }

                    if (professorDestino == "LUIS")
                    {
                        if (abreviacaoDestino.Contains("SEX3"))
                        {
                            string texto = "JUDENIR";
                        }
                    }

                    // Verificar se os professores estão bloqueados para os horários/dias específicos
                if (listOpcoesOrigem.Contains(abreviacaoOrigem))
                {
                    Console.WriteLine($"Troca inválida: {professorOrigem} está bloqueado para {abreviacaoOrigem}.");
                    continue;
                }

                if (listOpcoesDestino.Contains(abreviacaoDestino))
                {
                    Console.WriteLine($"Troca inválida: {professorDestino} está bloqueado para {abreviacaoDestino}.");
                    continue;
                }

                // Se passar pelas validações, adicionar à lista filtrada
                listaFiltrada.Add(possibilidade);
            }

            Console.WriteLine($"Total de trocas válidas após validação de opções: {listaFiltrada.Count}");
            return listaFiltrada;
        }



        private string ObterAbreviacao(string diaSemana, int linha)
        {
            // Converter o nome do dia para abreviação
            string abreviacaoDia = diaSemana switch
            {
                "Segunda-feira" => "SEG",
                "Terça-feira" => "TER",
                "Quarta-feira" => "QUA",
                "Quinta-feira" => "QUI",
                "Sexta-feira" => "SEX",
                _ => throw new ArgumentException($"Dia da semana inválido: {diaSemana}")
            };

            // Ajustar a linha para 1-based (se necessário)
            int horario = linha + 1;

            // Retornar a abreviação no formato correto
            return $"{abreviacaoDia}{horario}";
        }





        // Novo método específico para validação em proximidade
        private bool UnirProfessores_ValidarProximidade_ValidarDuplicidadeNaColuna(string professor, string[,] matriz, int colunaDestino)
        {


            // Garantir que o índice da coluna está dentro dos limites da matriz
            if (colunaDestino < 0 || colunaDestino >= matriz.GetLength(1))
            {
                Console.WriteLine($"Erro: colunaDestino ({colunaDestino}) está fora dos limites da matriz.");
                return false; // Retorna false se a coluna está fora do intervalo
            }

            // Processar todas as linhas na coluna fornecida
            int totalLinhas = matriz.GetLength(0); // Descobre dinamicamente o total de linhas
            int ocorrencias = 0; // Contador para ocorrências do professor na coluna

            for (int linha = 0; linha < totalLinhas; linha++) // Itera por todas as linhas da coluna especificada
            {
                if (matriz[linha, colunaDestino] == professor) // Verifica duplicidade
                {
                    ocorrencias++; // Incrementa o contador de ocorrências
                    Console.WriteLine($"Conflito parcial: {professor} encontrado na coluna {colunaDestino}, linha {linha}.");
                }
                
                ////if (ProfessorHelper.ExtrairNomeProfessor(matriz[linha, colunaDestino]) ==
                ////    ProfessorHelper.ExtrairNomeProfessor(professor)) // Verifica duplicidade
                ////{
                ////    ocorrencias++; // Incrementa o contador de ocorrências
                ////    Console.WriteLine($"Conflito parcial: {ProfessorHelper.ExtrairNomeProfessor(professor)} encontrado na coluna {colunaDestino}, linha {linha}.");
                ////}


                // Se mais de 1 ocorrência for encontrada, a duplicidade não é aceitável
                if (ocorrencias > 0)
                {
                    Console.WriteLine($"Conflito: Mais de uma ocorrência de {professor} na coluna {colunaDestino}.");
                    return true;
                }
            }

            // Retorna false se as duplicidades forem aceitáveis (0 ou 1 ocorrência)
            Console.WriteLine($"Nenhum conflito crítico encontrado para {professor} na coluna {colunaDestino}.");
            return false;
        }

        // Atualização do método para filtrar possibilidades
        private List<PossibilidadeTroca> UnirProfessores_FiltrarPossibilidades(List<PossibilidadeTroca> possibilidades)
        {
            var listaFiltrada = new List<PossibilidadeTroca>();

            foreach (var possibilidade in possibilidades)
            {
                // Obter as matrizes de origem e destino
                var matrizOrigem = UnirProfessores_ObterMatrizPorDia(possibilidade.MatrizOrigem);
                var matrizDestino = UnirProfessores_ObterMatrizPorDia(possibilidade.MatrizDestino);

                // Validar se o professor ainda está presente em outra posição na matriz de origem
                bool duplicidadeOrigem = UnirProfessores_ValidarProximidade_ValidarDuplicidadeNaColuna(
                    possibilidade.ProfessorOrigem, matrizDestino, possibilidade.ColunaDestino);

                //Validar se o professor de destino cria duplicidade na matriz de destino
                bool duplicidadeDestino = UnirProfessores_ValidarProximidade_ValidarDuplicidadeNaColuna(
                    possibilidade.ProfessorDestino, matrizOrigem , possibilidade.ColunaOrigem);


                if (possibilidade.ProfessorOrigem == "ELENICE")
                {
                        string texto = "ELENICE"; 
                }

                if (possibilidade.ProfessorDestino == "ELENICE")
                {
                        string texto = "ELENICE";
                }




                // Adicionar à lista somente se não houver duplicidade
                if (!duplicidadeOrigem && !duplicidadeDestino)
                {
                    listaFiltrada.Add(possibilidade);
                }
            }

            Console.WriteLine($"Possibilidades válidas após filtragem: {listaFiltrada.Count}");
            return listaFiltrada;
        }











        private void UnirProfessores_ExibirPossibilidades(List<PossibilidadeTroca> possibilidades)
        {
            //Console.WriteLine("Tabela de Possibilidades de Trocas:");
            //Console.WriteLine("| Matriz Origem   | Linha Origem | Coluna Origem | Professor Origem | Matriz Destino   | Linha Destino | Coluna Destino | Professor Destino | Troca Origem -> Destino Válida? | Troca Destino -> Origem Válida? | Forma Dobradinha? |");
            //foreach (var possibilidade in possibilidades)
            //{
            //    //Console.WriteLine($"| {possibilidade.MatrizOrigem,-15} | {possibilidade.LinhaOrigem,-12} | {possibilidade.ColunaOrigem,-13} | {possibilidade.ProfessorOrigem,-17} | " +
            //    //                  $"{possibilidade.MatrizDestino,-15} | {possibilidade.LinhaDestino,-13} | {possibilidade.ColunaDestino,-14} | {possibilidade.ProfessorDestino,-18} | " +
            //    //                  $"{possibilidade.TrocaOrigemParaDestinoValida,-29} | {possibilidade.TrocaDestinoParaOrigemValida,-29} | {possibilidade.FormaDobradinha,-16} |");
            //}
        }



        private bool UnirProfessores_FormaDobradinha(string professor, int coluna, string[,] matriz, int linhaDestino)
        {
            int totalLinhas = matriz.GetLength(0);
            bool dobradinhaImediata =
                (linhaDestino > 0 && matriz[linhaDestino - 1, coluna] == professor) ||
                (linhaDestino < totalLinhas - 1 && matriz[linhaDestino + 1, coluna] == professor);

            // Verificar se há pelo menos uma ocorrência adicional na mesma coluna
            bool existePar = false;
            for (int linha = 0; linha < totalLinhas; linha++)
            {
                //////if (linha != linhaDestino && matriz[linha, coluna] == professor)
                //////{
                //////    existePar = true;
                //////    break;
                //////}
                ///
                string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
                string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                if (linha != linhaDestino && nomeExtraidoMatriz == nomeExtraidoProfessor)
                {
                    existePar = true;
                    break;
                }

            }

            // Retornar true apenas se ambos forem verdadeiros
            return dobradinhaImediata && existePar;
        }






        ////private List<ProfessorConflito> UnirProfessores_IdentificarConflitos()
        ////{
        ////    var conflitos = new List<ProfessorConflito>();
        ////    string[][,] matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };
        ////    string[] diasDaSemana = { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };

        ////    for (int dia = 0; dia < matrizes.Length; dia++)
        ////    {
        ////        var matriz = matrizes[dia];
        ////        var diaSemana = diasDaSemana[dia];

        ////        for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
        ////        {
        ////            var horariosPorProfessor = new Dictionary<string, List<int>>();

        ////            for (int linha = 0; linha < matriz.GetLength(0); linha++)
        ////            {
        ////                var professor = matriz[linha, coluna];
        ////                if (!string.IsNullOrEmpty(professor))
        ////                {
        ////                    if (!horariosPorProfessor.ContainsKey(professor))
        ////                        horariosPorProfessor[professor] = new List<int>();

        ////                    horariosPorProfessor[professor].Add(linha);
        ////                }
        ////            }

        ////            foreach (var entry in horariosPorProfessor)
        ////            {
        ////                var horarios = entry.Value;
        ////                for (int i = 1; i < horarios.Count; i++)
        ////                {
        ////                    if (horarios[i] != horarios[i - 1] + 1)
        ////                    {
        ////                        conflitos.Add(new ProfessorConflito
        ////                        {
        ////                            Professor = entry.Key,
        ////                            Coluna = coluna,
        ////                            DiaSemana = diaSemana,
        ////                            Horarios = horarios
        ////                        });
        ////                        break;
        ////                    }
        ////                }
        ////            }
        ////        }
        ////    }

        ////    return conflitos;
        ////}



        // Método para identificar conflitos

        private List<ProfessorConflito> UnirProfessores_IdentificarConflitos()
        {
            // Lista para armazenar os conflitos identificados
            var conflitos = new List<ProfessorConflito>();

            // Matrizes de horários para cada dia da semana
            string[][,] matrizes = { matrizHorariosSegunda, matrizHorariosTerca, matrizHorariosQuarta, matrizHorariosQuinta, matrizHorariosSexta };

            // Nomes dos dias da semana correspondentes às matrizes
            string[] diasDaSemana = { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };

            // Iterar sobre cada dia da semana (índice das matrizes)
            for (int dia = 0; dia < matrizes.Length; dia++)
            {
                var matriz = matrizes[dia];          // Obter a matriz de horários do dia atual
                var diaSemana = diasDaSemana[dia];  // Nome do dia da semana correspondente

                // Iterar sobre cada coluna da matriz (representando um horário específico)
                for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                {
                    // Dicionário para armazenar os horários de cada professor na coluna atual
                    var horariosPorProfessor = new Dictionary<string, List<int>>();

                    // Iterar sobre cada linha da matriz (representando um professor ou uma turma)
                    for (int linha = 0; linha < matriz.GetLength(0); linha++)
                    {
                        var professor = matriz[linha, coluna]; // Obter o nome do professor na posição atual
                        if (!string.IsNullOrEmpty(professor)) // Ignorar células vazias
                        {
                            // Se o professor ainda não está no dicionário, inicializa a lista
                            if (!horariosPorProfessor.ContainsKey(professor))
                                horariosPorProfessor[professor] = new List<int>();

                            // Adicionar a linha atual à lista de horários do professor
                            horariosPorProfessor[professor].Add(linha);
                        }
                    }

                    // Iterar sobre cada professor e verificar conflitos
                    foreach (var entry in horariosPorProfessor)
                    {
                        var horarios = entry.Value; // Lista de horários ocupados pelo professor

                        // Ignorar professores com apenas um horário (não pode haver conflitos)
                        if (horarios.Count < 2)
                            continue;

                        // Verificar se os horários não são consecutivos
                        for (int i = 1; i < horarios.Count; i++)
                        {
                            // Comparar o horário atual com o anterior; se não forem consecutivos, há conflito
                            if (horarios[i] != horarios[i - 1] + 1)
                            {
                                // Adicionar o conflito à lista de resultados
                                conflitos.Add(new ProfessorConflito
                                {
                                    Professor = entry.Key,     // Nome do professor
                                    Coluna = coluna,           // Coluna onde ocorreu o conflito
                                    DiaSemana = diaSemana,     // Dia da semana correspondente
                                    Horarios = horarios        // Lista completa de horários do professor
                                });

                                // Interromper o loop para evitar múltiplos conflitos para o mesmo professor
                                break;
                            }
                        }
                    }
                }
            }

            // Retornar a lista de conflitos identificados
            return conflitos;
        }








        private bool UnirProfessores_ValidarTroca(
           string professorOrigem,
           string professorDestino,
           int coluna,
           string[,] matrizOrigem,
           string[,] matrizDestino,
           int linhaOrigem)
        {


           if(professorDestino =="ALESSANDRA")
            {
                string ACHOU = "ALESSANDRA";
            }


            // Validações separadas
            //bool proximidadeDestino = UnirProfessores_ValidarProximidadeDestino(professorOrigem, coluna, matrizDestino);
            // Validações separadas
            bool duplicidadeNaLinha_professorOrigem = !UnirProfessores_ValidarDuplicidadeNaLinha(professorOrigem, matrizDestino, coluna, linhaOrigem);
            // bool duplicidadeNaColuna_professorOrigem = !UnirProfessores_ValidarDuplicidadeNaColuna(professorOrigem, matrizDestino, coluna);
            bool duplicidadeNaColuna_professorOrigem = !UnirProfessores_ValidarDuplicidadeNaColuna(professorOrigem, matrizDestino, coluna, linhaOrigem);


            bool duplicidadeNaLinha_professorDestino = !UnirProfessores_ValidarDuplicidadeNaLinha(professorDestino, matrizOrigem, coluna, linhaOrigem);
            bool duplicidadeNaColuna_professorDestino = !UnirProfessores_ValidarDuplicidadeNaColuna(professorDestino, matrizOrigem, coluna, linhaOrigem);

            //bool proximidadeOrigem = UnirProfessores_ValidarProximidadeOrigem(professorDestino, coluna, matrizOrigem);
            //if (proximidadeDestino && duplicidadeNaLinha && duplicidadeNaColuna && proximidadeOrigem)
            if (duplicidadeNaLinha_professorOrigem && duplicidadeNaColuna_professorOrigem && duplicidadeNaLinha_professorDestino && duplicidadeNaColuna_professorDestino)
            {
                Console.WriteLine($"Troca válida entre {professorOrigem} e {professorDestino}.");
                return true;
            }

            Console.WriteLine($"Troca inválida entre {professorOrigem} e {professorDestino}: uma ou mais condições não atendidas.");
            return false;
        }

                                                                                              
        private bool UnirProfessores_ValidarDuplicidadeNaLinha(string professor, string[,] matriz, int colunaAtual, int linhaDestino)
        {
            // Verificar se o índice da linha está dentro dos limites da matriz
            if (linhaDestino < 0 || linhaDestino >= matriz.GetLength(0))
            {
                Console.WriteLine($"Erro: linhaDestino ({linhaDestino}) está fora dos limites da matriz.");
                return false;
            }

            // Iterar por todas as colunas na linha fornecida
            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
            {
                // Ignorar a coluna atual para não verificar a posição que será trocada
                if (coluna == colunaAtual)
                {
                    continue;
                }

                // Verificar duplicidade
                ////////if (matriz[linhaDestino, coluna] == professor)
                ////////{
                ////////    Console.WriteLine($"Conflito: {professor} já está presente na linha {linhaDestino}, coluna {coluna}.");
                ////////    return true;
                ////////}
                ///
                string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linhaDestino, coluna]);
                string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                {
                    Console.WriteLine($"Conflito: {nomeExtraidoProfessor} já está presente na linha {linhaDestino}, coluna {coluna}.");
                }

            }

            // Retornar false se nenhuma duplicidade for encontrada
            Console.WriteLine($"Nenhum conflito encontrado para {professor} na linha {linhaDestino}, excluindo a coluna {colunaAtual}.");
            return false;
        }







        private bool UnirProfessores_ValidarProximidadeDestino(string professor, int colunaDestino, string[,] matrizDestino)
        {
            for (int linha = 0; linha < matrizDestino.GetLength(0); linha++)
            {
                if (matrizDestino[linha, colunaDestino] == professor)
                {
                    // Verificar proximidade com as linhas adjacentes
                    if (linha > 0 && matrizDestino[linha - 1, colunaDestino] == professor)
                    {
                        Console.WriteLine($"Conflito de proximidade encontrado acima para {professor} na coluna {colunaDestino}.");
                        return false;
                    }
                    if (linha < matrizDestino.GetLength(0) - 1 && matrizDestino[linha + 1, colunaDestino] == professor)
                    {
                        Console.WriteLine($"Conflito de proximidade encontrado abaixo para {professor} na coluna {colunaDestino}.");
                        return false;
                    }
                }
            }
            return true;
        }


        private bool UnirProfessores_ValidarDuplicidadeNaColuna(string professor, string[,] matriz, int colunaDestino, int linhaAtual)
        {
            // Garantir que o índice da coluna está dentro dos limites da matriz
            if (colunaDestino < 0 || colunaDestino >= matriz.GetLength(1))
            {
                Console.WriteLine($"Erro: colunaDestino ({colunaDestino}) está fora dos limites da matriz.");
                return false; // Retorna false se a coluna está fora do intervalo
            }

            // Processar todas as linhas na coluna fornecida
            int totalLinhas = matriz.GetLength(0); // Descobre dinamicamente o total de linhas
            int ocorrencias = 0; // Contador para ocorrências do professor na coluna

            for (int linha = 0; linha < totalLinhas; linha++)
            {
                // Ignorar a linha atual ao verificar duplicidade
                if (linha == linhaAtual)
                {
                    continue;
                }

                if (matriz[linha, colunaDestino] == professor) // Verifica duplicidade
                {
                    ocorrencias++; // Incrementa o contador de ocorrências
                    Console.WriteLine($"Conflito parcial: {professor} encontrado na coluna {colunaDestino}, linha {linha}.");
                }
                
                //////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, colunaDestino]);
                //////string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                //////if (nomeExtraidoMatriz == nomeExtraidoProfessor) // Verifica duplicidade
                //////{
                //////    ocorrencias++; // Incrementa o contador de ocorrências
                //////    Console.WriteLine($"Conflito parcial: {nomeExtraidoProfessor} encontrado na coluna {colunaDestino}, linha {linha}.");
                //////}


                // Se mais de 1 ocorrência for encontrada, a duplicidade não é aceitável
                if (ocorrencias > 1)
                {
                    Console.WriteLine($"Conflito: Mais de uma ocorrência de {professor} na coluna {colunaDestino}.");
                    return true;
                }
            }

            // Retorna false se as duplicidades forem aceitáveis (0 ou 1 ocorrência além da posição atual)
            Console.WriteLine($"Nenhum conflito crítico encontrado para {professor} na coluna {colunaDestino}, excluindo a linha {linhaAtual}.");
            return false;
        }





        private void UnirProfessores_ValidarProximidade(List<PossibilidadeTroca> possibilidades)
        {
            foreach (var possibilidade in possibilidades)
            {
                var matrizDestino = UnirProfessores_ObterMatrizPorDia(possibilidade.MatrizDestino);

                possibilidade.FormaDobradinha = UnirProfessores_FormaDobradinha(
                    possibilidade.ProfessorOrigem,
                    possibilidade.ColunaDestino,
                    matrizDestino,
                    possibilidade.LinhaDestino);
            }
        }






        //////////private bool UnirProfessores_ValidarProximidadeOrigem(string professor, int coluna, string[,] matriz)
        //////////{
        //////////    var linhasOcupadas = new List<int>();

        //////////    for (int linha = 0; linha < matriz.GetLength(0); linha++)
        //////////    {
        //////////        if (matriz[linha, coluna] == professor)
        //////////        {
        //////////            linhasOcupadas.Add(linha);
        //////////        }
        //////////    }

        //////////    foreach (var linha in linhasOcupadas)
        //////////    {
        //////////        if (linhasOcupadas.Any(l => Math.Abs(l - linha) == 1))
        //////////        {
        //////////            return true;
        //////////        }
        //////////    }

        //////////    return false;
        //////////}





        // Obter a matriz por dia da semana
        private string[,] UnirProfessores_ObterMatrizPorDia(string diaSemana)
        {
            return diaSemana switch
            {
                "Segunda-feira" => this.matrizHorariosSegunda,
                "Terça-feira" => this.matrizHorariosTerca,
                "Quarta-feira" => this.matrizHorariosQuarta,
                "Quinta-feira" => this.matrizHorariosQuinta,
                "Sexta-feira" => this.matrizHorariosSexta,
                _ => throw new ArgumentException($"Dia da semana inválido: {diaSemana}")
            };
        }








        // fim 
        // -----------------------------------------------------------------------------------------------------------------------------------


        // Método auxiliar para converter número do dia da semana para string correspondente
        //private string DiaDaSemanaToString(int dia)
        //{
        //    switch (dia)
        //    {
        //        case 1: return "Segunda-feira";
        //        case 2: return "Terça-feira";
        //        case 3: return "Quarta-feira";
        //        case 4: return "Quinta-feira";
        //        case 5: return "Sexta-feira";
        //        default: return "Segunda-feira"; // Valor padrão por segurança
        //    }
        //}





        // ===================================================================================================

        // ======== INCLUIR PROFESSOR VERTICAL E MoverHorizontal 

        private string[,] CopiarMatriz(string[,] matrizOriginal)
        {
            int linhas = matrizOriginal.GetLength(0);
            int colunas = matrizOriginal.GetLength(1);
            var matrizCopia = new string[linhas, colunas];

            for (int i = 0; i < linhas; i++)
            {
                for (int j = 0; j < colunas; j++)
                {
                    matrizCopia[i, j] = matrizOriginal[i, j];
                }
            }

            return matrizCopia;
        }






        public void IncluirProfessorVertical_MoverHorizontal()
        {
            int maxIteracoes = 10; // Número máximo de iterações
            int iteracaoAtual = 0;


            // Controle de progresso para cada professor
            Dictionary<string, int> progressoProfessores = new Dictionary<string, int>();


            //while (iteracaoAtual < maxIteracoes)
            //{

            // Identifica necessidades para inclusão do professor
            var ListprofessoresInserir = IncluirProfessorVertical_IdentificarNecessidades().ToList();
            //.Where(x => x.Professor == "ZILRENE")
            //.Where(x => x.Turma == "9Y")
            //.ToList();
            for (int i = 1; i <= maxIteracoes; i++)
            {
                //ListprofessoresInserir = IncluirProfessorVertical_IdentificarNecessidades().ToList();

                ListprofessoresInserir = IncluirProfessorVertical_IdentificarNecessidades().ToList();
                ////.Where(x => x.Professor == "ZILRENE")
                ////.Where(x => x.Turma == "9Y")
                ////.ToList();


                //.Where(x => x.Turma == "9Y").ToList().ToList()
                foreach (var professoresInserir in ListprofessoresInserir)
                {


                    // Inicializa o progresso do professor, se ainda não existir
                    if (!progressoProfessores.ContainsKey(professoresInserir.Professor))
                    {
                        progressoProfessores[professoresInserir.Professor] = 0;
                    }



                    // Gerar possibilidades de troca
                    var ListaProcessamentoInsercao = IncluirProfessorVertical_GerarPossibilidades(new List<ProfessorNecessidadeInsercao> { professoresInserir });

                    if (!ListaProcessamentoInsercao.Any())
                    {
                        Console.WriteLine($"Sem mais possibilidades após {iteracaoAtual} iterações.");
                        break; // Interrompe o loop se não houver mais possibilidades
                    }


                    if (professoresInserir.Professor == "ZILRENE")
                    {
                        if (professoresInserir.Turma == "9Y")
                        {
                            String CHEGOU = "ZILRENE";
                        }
                    }

                    // verificar bloqueio e opcoes 
                    var ListaProcessamentoInsercaoValidarOpcoes = IncluirProfessorVertical_ValidarOpcoesProfessores(ListaProcessamentoInsercao);



                    List<ProcessamentoInsercao> ListaProfessoresValidados_NaMatrizCopia = new List<ProcessamentoInsercao>();
                    // confrontar os bloqueios dentro da matriz copia
                    // simulação da movimentacao na matriz copia
                    if (ListaProcessamentoInsercao.Count > 0) //; ; if (ListaProcessamentoInsercaoValidarOpcoes.Count > 0)
                    {
                        ListaProfessoresValidados_NaMatrizCopia = IncluirProfessorVertical_SimularTroca_MatrizesCopia(ListaProcessamentoInsercao);
                    }


                     // Processar e aplicar a melhor troca
                     //** Pode acontecer que não há possiblidade de troca 
                    if (ListaProfessoresValidados_NaMatrizCopia.Count > 0)
                    {
                        IncluirProfessorVertical_ProcessarTrocas(ListaProfessoresValidados_NaMatrizCopia, progressoProfessores);
                    }

                    var verNecessidade = necessidadesProfessores;
                    iteracaoAtual++;
                   

                    Console.WriteLine($"Processamento concluído após {iteracaoAtual} iterações.");
                }
            }
        }



        private List<ProcessamentoInsercao> IncluirProfessorVertical_SimularTroca_MatrizesCopia(List<ProcessamentoInsercao>  ListaProcessamentoInsercaoValidarOpcoes)
        {
            // Nova lista para armazenar os processamentos válidos
            var listaFiltrada = new List<ProcessamentoInsercao>();

            bool valideVertical_Destino = false;
            bool valideVertical_Inserido = false;
            bool valideHorizontal_Origem = false;
            bool valideHorizontal_Destino = false;

            bool validaColuna = false;
            bool validaLinha = false;
            foreach (var processamento in ListaProcessamentoInsercaoValidarOpcoes)
            {
                // vamos processar  cada linha do vertical para validar os dados 
                // e dentro do horizontal vamos fazer os dois tipos de dados
                // vertical e horizontal dos professores

                var possibilidadesHorizontalValidas = new List<ProfessorInsercaoPossibilidade>();

                



                foreach (var processamento_Horizontal in processamento.PossibilidadesHorizontal)
                {

                    //reinicia as variaveis 
                    valideVertical_Destino = false;
                    valideVertical_Inserido = false;
                    valideHorizontal_Origem = false;
                    valideHorizontal_Destino = false;
                    validaColuna = false;
                    validaLinha = false;

                    // Criar cópias das matrizes originais para esta iteração do primeiro loop
                    var copiaMatriz_Segunda = CopiarMatriz(matrizHorariosSegunda);
                    var copiaMatriz_Terca = CopiarMatriz(matrizHorariosTerca);
                    var copiaMatriz_Quarta = CopiarMatriz(matrizHorariosQuarta);
                    var copiaMatriz_Quinta = CopiarMatriz(matrizHorariosQuinta);
                    var copiaMatriz_Sexta = CopiarMatriz(matrizHorariosSexta);


                    // **Etapa 1: Processar a troca Vertical**
                    var possibilidadeVertical = processamento.PossibilidadeVertical;





                    if (possibilidadeVertical.ProfessorInserido == "RAQUEL")
                    {
                        string achoue = "RAQUEL";
                    }

                    if (possibilidadeVertical.ProfessorDestino == "RAQUEL")
                    {
                        string achoue = "RAQUEL";
                    }



                    // Obter os grids correspondentes
                    // Obter os grids correspondentes de forma dinâmica
                    string diaSemanaOrigem = possibilidadeVertical.MatrizOrigem;
                    var matrizOrigemVertical = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };

                    diaSemanaOrigem = possibilidadeVertical.MatrizDestino;
                    var matrizDestinoVertical = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };


                    // ----------------------------------------------------------------------------
                    // # professor 1 vertical

                    // Atualizar o grid de origem (remover o professor origem e inserir o professor inserido)
                    matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = string.Empty;
                    matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = possibilidadeVertical.ProfessorDestino;


                    // ----------------------------------------------------------------------------
                    // Professor 2
                    // Atualizar o grid  (inserir o professor NOVO)
                    matrizOrigemVertical[possibilidadeVertical.LinhaProfessorInserido, possibilidadeVertical.ColunaProfessorInserido] = possibilidadeVertical.ProfessorInserido;



                    // ----------------------------------------------------------------------------
                    // troca professor 3


                    // Obter os grids correspondentes
                    diaSemanaOrigem = processamento_Horizontal.MatrizOrigem;
                    var matrizOrigemHorizontal = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };

                    diaSemanaOrigem = processamento_Horizontal.MatrizDestino;
                    var matrizDestinoHorizontal = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };

                    // # professor 3 Horizontal

                    // Atualizar o grid de origem (remover o professor origem e inserir o professor destino)
                    matrizOrigemHorizontal[processamento_Horizontal.LinhaOrigem, processamento_Horizontal.ColunaOrigem] = string.Empty;
                    matrizOrigemHorizontal[processamento_Horizontal.LinhaOrigem, processamento_Horizontal.ColunaOrigem] = processamento_Horizontal.ProfessorDestino;

                    // ----------------------------------------------------------------------------
                    // troca professor 4

                    // Atualizar o grid de destino (inserir o professor origem no destino)
                    matrizDestinoHorizontal[processamento_Horizontal.LinhaDestino, processamento_Horizontal.ColunaDestino] = processamento_Horizontal.ProfessorOrigem;



                    // ----------------------------------------------------------------------------
                    validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(possibilidadeVertical.ProfessorDestino, matrizDestinoVertical, possibilidadeVertical.LinhaDestino);
                    validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(possibilidadeVertical.ProfessorDestino, matrizDestinoVertical, possibilidadeVertical.ColunaDestino);

                    if(validaColuna && validaLinha)
                    {
                        valideVertical_Destino = true;


                        // vamos validar se os professores nao estão bloqueados para a opcao inserida
                        string abreviacaoProfessorDestino = $"{possibilidadeVertical.MatrizDestino.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaDestino}";
                        // Obter as opções de bloqueio dos professores com validação robusta
                        string listOpcoesOrigemVertical_ProfessorDestino =
                            !string.IsNullOrEmpty(possibilidadeVertical.ProfessorDestino) &&
                            this.Professores.ContainsKey(possibilidadeVertical.ProfessorDestino)
                                ? this.Professores[possibilidadeVertical.ProfessorDestino].Item3
                                : string.Empty;


                        // Validar se os professores estão bloqueados nos horários horizontais
                        if (listOpcoesOrigemVertical_ProfessorDestino.Contains(abreviacaoProfessorDestino))
                        {
                            // muda para false nao passou
                            valideVertical_Destino = false;
                        }

                    }
                    else
                    {
                        // Se qualquer validação falhar, a troca não é válida
                        valideVertical_Destino = false;
                    }




                    // # professor 2 vertical

                    validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(possibilidadeVertical.ProfessorInserido, matrizOrigemVertical, possibilidadeVertical.LinhaProfessorInserido);
                    validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(possibilidadeVertical.ProfessorInserido, matrizOrigemVertical, possibilidadeVertical.ColunaProfessorInserido);

                    if (validaColuna && validaLinha)
                    {
                        valideVertical_Inserido = true; // Inicialmente, consideramos como válido

                                                        // PAREI AQUI NO DEBUG COM A ELENICE SE DER PROBLEM CONTINAR A PARTIR DAQUI EU RETIREI O +1 DO FINAL DA LINHA
                        string abreviacaoProfessorInserido = $"{possibilidadeVertical.MatrizOrigem.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaProfessorInserido + 1}";
                        string listOpcoesDestinoVertical_ProfessorInserido =
                            !string.IsNullOrEmpty(possibilidadeVertical.ProfessorInserido) &&
                            this.Professores.ContainsKey(possibilidadeVertical.ProfessorInserido)
                                ? this.Professores[possibilidadeVertical.ProfessorInserido].Item3
                                : string.Empty;


                        if (listOpcoesDestinoVertical_ProfessorInserido.Contains(abreviacaoProfessorInserido))
                        {
                            // muda para false nao passou
                            valideVertical_Inserido = false;
                        }


                    }
                    else
                    {
                        // Se qualquer validação falhar, a troca não é válida
                        valideVertical_Inserido = false;
                    }

                    // ------------------------------------------------------


                    // **Etapa 2: Selecionar e processar um único item Horizontal**
                    if (processamento_Horizontal != null)
                    {
                        Console.WriteLine($"Processando troca Horizontal: Professor {processamento_Horizontal.ProfessorInserido} sendo inserido.");

                        validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(processamento_Horizontal.ProfessorDestino, matrizOrigemHorizontal, processamento_Horizontal.LinhaOrigem);
                        validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(processamento_Horizontal.ProfessorDestino, matrizOrigemHorizontal, processamento_Horizontal.ColunaOrigem);

                        if (validaColuna && validaLinha)
                        {
                            valideHorizontal_Origem = true;

                            // ATENCAO TEM QUE VALIDAR IGUAL ENCIMA NA MESMA MATRIZ VALIDADA ONDE O PROFESSOR FOI GRAVADO
                            string abreviacaoOrigemHorizontal_ProfessorDestino = $"{processamento_Horizontal.MatrizOrigem.Substring(0, 3).ToUpper()}{processamento_Horizontal.LinhaOrigem + 1}";

                            // Obter as opções de bloqueio dos professores com validação robusta
                            string listOpcoesOrigemHorizontal_ProfessorDestino =
                                !string.IsNullOrEmpty(processamento_Horizontal.ProfessorDestino) &&
                                this.Professores.ContainsKey(processamento_Horizontal.ProfessorDestino)
                                    ? this.Professores[processamento_Horizontal.ProfessorDestino].Item3
                                    : string.Empty;

                            if (listOpcoesOrigemHorizontal_ProfessorDestino.Contains(abreviacaoOrigemHorizontal_ProfessorDestino))
                            {
                                valideHorizontal_Origem = false;
                            }

                        }
                        else
                        {
                            // Se qualquer validação falhar, a troca não é válida
                            valideHorizontal_Origem = false;
                        }


                        // # professor 4 Horizontal                    

                        //LINHA 4 COLUNA 1  segunda feira

                        validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(processamento_Horizontal.ProfessorOrigem, matrizDestinoHorizontal, processamento_Horizontal.LinhaDestino);
                        validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(processamento_Horizontal.ProfessorOrigem, matrizDestinoHorizontal, processamento_Horizontal.ColunaDestino);

                        if (validaColuna && validaLinha)
                        {
                            valideHorizontal_Destino = true;


                            string abreviacaoOrigemHorizontal_ProfessorOrigem = $"{processamento_Horizontal.MatrizDestino.Substring(0, 3).ToUpper()}{processamento_Horizontal.LinhaDestino + 1}";

                            // Obter as opções de bloqueio dos professores com validação robusta
                            string listOpcoesOrigemHorizontal_ProfessorOrigem =
                                !string.IsNullOrEmpty(processamento_Horizontal.ProfessorOrigem) &&
                                this.Professores.ContainsKey(processamento_Horizontal.ProfessorOrigem)
                                    ? this.Professores[processamento_Horizontal.ProfessorOrigem].Item3
                                    : string.Empty;

                            if (listOpcoesOrigemHorizontal_ProfessorOrigem.Contains(abreviacaoOrigemHorizontal_ProfessorOrigem))
                            {
                                valideHorizontal_Destino = false;
                            }

                        }
                        else
                        {
                            // Se qualquer validação falhar, a troca não é válida
                            valideHorizontal_Destino = false;
                        }


                        if (valideVertical_Destino && valideVertical_Inserido && valideHorizontal_Origem && valideHorizontal_Destino)
                        {
                           // Adiciona a possibilidade horizontal à lista de válidas
                            possibilidadesHorizontalValidas.Add(processamento_Horizontal);
                        }
                    }
                }


                // Após processar todas as possibilidades horizontais para essa vertical

                    if (possibilidadesHorizontalValidas.Any())
                    {
                        var novoProcessamento = new ProcessamentoInsercao
                        {
                            PossibilidadeVertical = processamento.PossibilidadeVertical,
                            PossibilidadesHorizontal = possibilidadesHorizontalValidas
                        };

                        listaFiltrada.Add(novoProcessamento);
                    }
               

            }

            return listaFiltrada;
        }

  
        private bool IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(string professor, string[,] matriz, int linha)
        {
            // Contador para ocorrências do professor na linha
            int contador = 0;

            // Iterar por todas as colunas baseando-se no tamanho da lista `Turmas`
            for (int coluna = 0; coluna < this.Turmas.Count; coluna++)
            {
                // Verificar se o professor está na célula da matriz correspondente
                //////if (matriz[linha, coluna] == professor)
                //////{
                //////    contador++;
                //////    // Se o professor aparecer mais de uma vez, retorna **false** indicando problema
                //////    if (contador > 1)
                //////    {
                //////        Console.WriteLine($"Professor '{professor}' encontrado mais de uma vez na linha {linha + 1}.");
                //////        return false; // Indica problema
                //////    }


                //////}
                ///


                string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
                string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                {
                    contador++;
                    // Se o professor aparecer mais de uma vez, retorna **false** indicando problema
                    if (contador > 1)
                    {
                        Console.WriteLine($"Professor '{professor}' encontrado mais de uma vez na linha {linha + 1}.");
                        return false; // Indica problema
                    }
                }

            }

            // Retorna **true** se o professor apareceu no máximo uma vez (sem problemas)
            return true;
        }


        private bool IncluirProfessorVertical_Contar_Professor_Colunas(string professor, string[,] matriz, int colunafixa)
        {
            // Contador para ocorrências do professor na coluna
            int contador = 0;

            int _turmaIndex = colunafixa;
            string E_Polivalente = ListTurmasProf.First(x => x.TurmaIndex == _turmaIndex).Polivalente;


            // Verificar se a coluna fornecida está dentro dos limites válidos
            if (colunafixa < 0 || colunafixa >= this.Turmas.Count)
            {
                Console.WriteLine($"Coluna inválida: {colunafixa}. Não está dentro do intervalo permitido.");
                return false; // Indica problema
            }

            // Iterar sobre todas as linhas da matriz para a coluna fixa
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                // 3AULAS
                // Verificar se o professor está na célula da matriz correspondente
                if (matriz[linha, colunafixa] == professor)
                {
                    contador++;
                }
                
                ////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, colunafixa]);
                ////string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                ////if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                ////{
                ////    contador++;
                ////}

            }


            // Exibir o número de ocorrências encontradas para debug
            Console.WriteLine($"Professor '{professor}' encontrado {contador} vez(es) na coluna {colunafixa}.");


            //if (E_Polivalente == "SIM")
            //{
            //    // retornar zero para permitir muitos lancamentos na mesmo coluna
            //    // porque é turma polivalente.
            //    contador = 0;
            //}

            // Retorna **true** se o professor está dentro do limite permitido (no máximo 2)
            return contador <= 2;
        }


        private List<ProcessamentoInsercao> IncluirProfessorVertical_ValidarOpcoesProfessores(List<ProcessamentoInsercao> listaProcessamentoInsercao)
        {


            // Nova lista para armazenar os processamentos válidos
            var listaFiltrada = new List<ProcessamentoInsercao>();

            foreach (var processamento in listaProcessamentoInsercao)
            {
                // **Validação Vertical**
                var possibilidadeVertical = processamento.PossibilidadeVertical;

                // Gerar abreviação para a origem e destino possibilidadeVertical
                string abreviacaoProfessorOrigem = $"{possibilidadeVertical.MatrizDestino.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaDestino + 1}";
                string abreviacaoProfessorInserido = $"{possibilidadeVertical.MatrizOrigem.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaProfessorInserido + 1}";


                string listOpcoesOrigemVertical_ProfessorOrigem =
                    !string.IsNullOrEmpty(possibilidadeVertical.ProfessorOrigem) &&
                    this.Professores.ContainsKey(possibilidadeVertical.ProfessorOrigem)
                        ? this.Professores[possibilidadeVertical.ProfessorOrigem].Item3
                        : string.Empty;

                string listOpcoesDestinoVertical_ProfessorInserido =
                    !string.IsNullOrEmpty(possibilidadeVertical.ProfessorInserido) &&
                    this.Professores.ContainsKey(possibilidadeVertical.ProfessorInserido)
                        ? this.Professores[possibilidadeVertical.ProfessorInserido].Item3
                        : string.Empty;




                // Validar se os professores estão bloqueados nos horários horizontais
                if (listOpcoesOrigemVertical_ProfessorOrigem.Contains(abreviacaoProfessorOrigem))
                {
                    Console.WriteLine($"Troca inválida: {possibilidadeVertical.ProfessorOrigem} está bloqueado para {abreviacaoProfessorOrigem}.");
                    continue;
                }

                if (listOpcoesDestinoVertical_ProfessorInserido.Contains(abreviacaoProfessorInserido))
                {
                    Console.WriteLine($"Troca inválida: {possibilidadeVertical.ProfessorInserido} está bloqueado para {abreviacaoProfessorInserido}.");
                    continue;
                }

                // **Validação Horizontal**
                var listaHorizontalValidas = new List<ProfessorInsercaoPossibilidade>();

                foreach (var possibilidadeHorizontal in processamento.PossibilidadesHorizontal)
                {


                    // Obter as opções de bloqueio dos professores com validação robusta
                    string listOpcoesOrigemHorizontal_ProfessorOrigem =
                        !string.IsNullOrEmpty(possibilidadeHorizontal.ProfessorOrigem) &&
                        this.Professores.ContainsKey(possibilidadeHorizontal.ProfessorOrigem)
                            ? this.Professores[possibilidadeHorizontal.ProfessorOrigem].Item3
                            : string.Empty;

                    string listOpcoesDestinoHorizontal_ProfessorDestino =
                        !string.IsNullOrEmpty(possibilidadeHorizontal.ProfessorDestino) &&
                        this.Professores.ContainsKey(possibilidadeHorizontal.ProfessorDestino)
                            ? this.Professores[possibilidadeHorizontal.ProfessorDestino].Item3
                            : string.Empty;





                    // Gerar abreviação para a origem e destino Horizontal
                    // ** aqui precisei fazer a inversao das matrizes pois estou
                    // realizando trocas entre eles
                    string abreviacaoOrigemHorizontal_ProfessorOrigem = $"{possibilidadeHorizontal.MatrizDestino.Substring(0, 3).ToUpper()}{possibilidadeHorizontal.LinhaDestino + 1}";
                    string abreviacaoDestinoHorizontal_ProfessorDestino = $"{possibilidadeHorizontal.MatrizOrigem.Substring(0, 3).ToUpper()}{possibilidadeHorizontal.LinhaOrigem + 1}";



                    // Validar se os professores estão bloqueados nos horários verticais
                    if (listOpcoesOrigemHorizontal_ProfessorOrigem.Contains(abreviacaoOrigemHorizontal_ProfessorOrigem))
                    {
                        Console.WriteLine($"Troca inválida: {possibilidadeHorizontal.ProfessorOrigem} está bloqueado para {abreviacaoOrigemHorizontal_ProfessorOrigem}.");
                        continue;
                    }

                    if (listOpcoesDestinoHorizontal_ProfessorDestino.Contains(abreviacaoDestinoHorizontal_ProfessorDestino))
                    {
                        Console.WriteLine($"Troca inválida: {possibilidadeHorizontal.ProfessorInserido} está bloqueado para {abreviacaoDestinoHorizontal_ProfessorDestino}.");
                        continue;
                    }

                    // Adicionar à lista Horizontal válida
                    listaHorizontalValidas.Add(possibilidadeHorizontal);
                }

                // Adicionar o processamento apenas se houver pelo menos uma possibilidade Horizontal válida
                if (listaHorizontalValidas.Any())
                {
                    listaFiltrada.Add(new ProcessamentoInsercao
                    {
                        PossibilidadeVertical = possibilidadeVertical,
                        PossibilidadesHorizontal = listaHorizontalValidas //possibilidadeHorizontal

                    });
                }
            }

            Console.WriteLine($"Total de processamentos válidos: {listaFiltrada.Count}");
            return listaFiltrada;
        }


        public List<ProcessamentoInsercao> IncluirProfessorVertical_GerarPossibilidades(List<ProfessorNecessidadeInsercao> professoresInserir)
        {


            var listaProcessamento = new List<ProcessamentoInsercao>();

            foreach (var necessidade in professoresInserir)
            {
                foreach (var diaSemanaOrigem in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    var matrizOrigem = IncluirProfessorVertical_ObterMatrizPorDia(diaSemanaOrigem);

                    for (int linhaOrigem = 0; linhaOrigem < matrizOrigem.GetLength(0); linhaOrigem++)
                    {
                        var professorOrigem = matrizOrigem[linhaOrigem, necessidade.Coluna];
                        var turmaOrigem = this.Turmas[necessidade.Coluna];

                        // Ignorar a posição onde o professor já está
                        if (professorOrigem == necessidade.Professor)
                            continue;

                        foreach (var diaSemanaDestino in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                        {
                            var matrizDestino = IncluirProfessorVertical_ObterMatrizPorDia(diaSemanaDestino);

                            for (int linhaDestino = 0; linhaDestino < matrizDestino.GetLength(0); linhaDestino++)
                            {
                                var professorDestino = matrizDestino[linhaDestino, necessidade.Coluna];
                                var turmaDestino = this.Turmas[necessidade.Coluna];

                                //**Nova Condição: Repetir a Mesma Validação do Primeiro foreach**
                                //Ignorar a posição se for a mesma linha e matriz
                                if (diaSemanaOrigem == diaSemanaDestino && linhaDestino == linhaOrigem && professorDestino == necessidade.Professor)
                                        continue;
                                
                                ////// **Nova Condição: Repetir a Mesma Validação do Primeiro foreach**
                                ////// Ignorar a posição se for a mesma linha e matriz
                                ////if (diaSemanaOrigem == diaSemanaDestino &&
                                ////    linhaDestino == linhaOrigem &&
                                ////    ProfessorHelper.ExtrairNomeProfessor(professorDestino) ==
                                ////    ProfessorHelper.ExtrairNomeProfessor(necessidade.Professor))
                                ////{
                                ////    continue;
                                ////}

                                if (string.IsNullOrEmpty(professorDestino))
                                {
                                    var possibilidade = new ProfessorInsercaoPossibilidade
                                    {
                                        MatrizOrigem = diaSemanaOrigem,
                                        LinhaOrigem = linhaOrigem,
                                        ColunaOrigem = necessidade.Coluna,
                                        ProfessorOrigem = professorOrigem,
                                        TurmaOrigem = turmaOrigem,

                                        MatrizDestino = diaSemanaDestino,
                                        LinhaDestino = linhaDestino,
                                        ColunaDestino = necessidade.Coluna,
                                        ProfessorDestino = professorOrigem,    //professorDestino,
                                        TurmaDestino = turmaDestino,

                                        ProfessorInserido = necessidade.Professor,
                                        LinhaProfessorInserido = linhaOrigem, //linhaDestino,
                                        ColunaProfessorInserido = necessidade.Coluna,
                                        TurmaProfessorInserido = turmaDestino,

                                        InsercaoValida = true,
                                        RealocacaoValida = true,
                                        FormaDobradinha = false
                                    };

                                    var possibilidadesHorizontal = IncluirProfessorVertical_FiltrarPossibilidades(
                                        new List<ProfessorInsercaoPossibilidade> { possibilidade });

                                    if (possibilidadesHorizontal.Any())
                                    {
                                        listaProcessamento.Add(new ProcessamentoInsercao
                                        {
                                            PossibilidadeVertical = possibilidade,
                                            PossibilidadesHorizontal = possibilidadesHorizontal                                            
                                        });


                                    }
                                }
                            }
                        }
                    }
                }
            }

            return listaProcessamento;
        }



        // Atualização do método para filtrar possibilidades de inserção
        private List<ProfessorInsercaoPossibilidade> IncluirProfessorVertical_FiltrarPossibilidades(List<ProfessorInsercaoPossibilidade> possibilidades)
        {
            var listaFiltrada = new List<ProfessorInsercaoPossibilidade>();
            var listaFiltradaOcorrencia = new List<ProfessorInsercaoPossibilidade>();

            foreach (var possibilidade in possibilidades)
            {
                // Obter a matriz de destino
                var matrizDestino = IncluirProfessorVertical_ObterMatrizPorDia(possibilidade.MatrizDestino);

                // Verificar duplicidade para o professor na origem
                bool duplicidadeProfessorOrigem = IncluirProfessorVertical_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                    possibilidade.ProfessorOrigem, matrizDestino, possibilidade.ColunaDestino);

                // Processar apenas se não houver duplicidade
                if (!duplicidadeProfessorOrigem)
                {
                    // Lista para armazenar ocorrências do professor em outras colunas
                    var ocorrencias = new List<ProfessorInsercaoPossibilidade>();

                    // Varrer todas as colunas da mesma linha na matriz atual
                    for (int coluna = 0; coluna < matrizDestino.GetLength(1); coluna++)
                    {
                        // Ignorar a coluna onde o professor já está
                        if (coluna == possibilidade.ColunaDestino) continue;

                        // Verificar se o professor está presente na coluna
                        if (matrizDestino[possibilidade.LinhaDestino, coluna] == possibilidade.ProfessorOrigem)
                        {
                            var _TurmaOrigem = this.Turmas[coluna];
                            var _TurmaDestino = this.Turmas[coluna];
                            // Criar um objeto representando essa ocorrência
                            var novaPossibilidade = new ProfessorInsercaoPossibilidade
                            {
                                MatrizOrigem = possibilidade.MatrizDestino, // Mesma matriz
                                LinhaOrigem = possibilidade.LinhaDestino,   // Mesma linha
                                ColunaOrigem = coluna,                     // Coluna encontrada
                                ProfessorOrigem = possibilidade.ProfessorOrigem,
                                TurmaOrigem = _TurmaOrigem, //possibilidade.TurmaDestino, // Turma associada

                                //MatrizDestino = possibilidade.MatrizDestino, // Matriz atual
                                //LinhaDestino = possibilidade.LinhaDestino,   // Linha atual
                                //ColunaDestino = possibilidade.ColunaDestino, // Coluna de destino (original)
                                //ProfessorInserido = possibilidade.ProfessorInserido,
                                //TurmaDestino = possibilidade.TurmaDestino,
                                ProfessorDestino = "BUSCAR PROFESSOR PARA TROCA",

                                InsercaoValida = false, // Aqui ainda não validamos nada
                                RealocacaoValida = false, // Apenas registramos a duplicidade
                                FormaDobradinha = false
                            };

                            // Adicionar essa ocorrência à lista de ocorrências
                            ocorrencias.Add(novaPossibilidade);
                        }
                    }

                    // Adicionar todas as ocorrências encontradas à lista filtrada
                    listaFiltradaOcorrencia.AddRange(ocorrencias);
                }
            }

            // **Parte 2: Processar Novas Trocas na ColunaOrigem**
            var novasPossibilidades = new List<ProfessorInsercaoPossibilidade>();

            foreach (var ocorrencia in listaFiltradaOcorrencia)
            {
                // Obter a matriz de origem (onde o professor está atualmente, ex.: quinta-feira)
                var matrizOrigemVertical = IncluirProfessorVertical_ObterMatrizPorDia(ocorrencia.MatrizOrigem);

                // Processar todas as matrizes (segunda-feira a sexta-feira) para trocas
                foreach (var diaSemana in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    // Obter a matriz de destino para o dia específico
                    var matrizDestinoVerticalDaTroca = IncluirProfessorVertical_ObterMatrizPorDia(diaSemana);

                    // Processar todas as linhas da coluna de origem
                    for (int linhaDaTroca = 0; linhaDaTroca < matrizDestinoVerticalDaTroca.GetLength(0); linhaDaTroca++)
                    {
                        //pego o primeiro da linha na coluna achada
                        var professorDestino = matrizDestinoVerticalDaTroca[linhaDaTroca, ocorrencia.ColunaOrigem];

                        // Ignorar células vazias
                        if (string.IsNullOrEmpty(professorDestino)) continue;

                        // Ignorar quando o professor origem for igual ao professor destino
                        if (professorDestino == ocorrencia.ProfessorOrigem) continue;

                        // Validar duplicidade na linha e coluna da matrizDestino e matrizOrigem
                        bool duplicidadeColunaDestino = IncluirProfessorVertical_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                            ocorrencia.ProfessorOrigem, matrizDestinoVerticalDaTroca, ocorrencia.ColunaOrigem);

                        bool duplicidadeLinhaDestino = IncluirProfessorVertical_ValidarDuplicidadeNaLinhaEmTodasColunas(
                            ocorrencia.ProfessorOrigem, matrizDestinoVerticalDaTroca, ocorrencia.ColunaOrigem, linhaDaTroca);

                        // o codigo abaixo considera se existir 1 na coluna e considerado 2 porque pretendo adicionar mais 1
                        // entao o codigo nao permite adicionar
                        bool duplicidadeColunaOrigem = IncluirProfessorVertical_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                            professorDestino, matrizOrigemVertical, ocorrencia.ColunaOrigem);

                        bool duplicidadeLinhaOrigem = IncluirProfessorVertical_ValidarDuplicidadeNaLinhaEmTodasColunas(
                            professorDestino, matrizOrigemVertical, ocorrencia.ColunaOrigem, ocorrencia.LinhaOrigem);

                        // Só continuar se não houver duplicidade em nenhuma validação
                        if (!duplicidadeLinhaDestino && !duplicidadeLinhaOrigem && !duplicidadeColunaDestino && !duplicidadeColunaOrigem)
                        {
                            var _TurmaDestino = this.Turmas[ocorrencia.ColunaOrigem];
                            // Registrar a nova possibilidade de troca
                            // ATENCAO VOU COLOCAR AQUI AS COODENADAS ENCONTRADAS 
                            //PARA CADA UM SERA NECESSARIO FAZER A INVERSAO PARA GRAVACAO OK
                            novasPossibilidades.Add(new ProfessorInsercaoPossibilidade
                            {
                                MatrizOrigem = diaSemana,
                                LinhaOrigem = linhaDaTroca,
                                ColunaOrigem = ocorrencia.ColunaOrigem,
                                ProfessorOrigem = professorDestino,
                                TurmaOrigem = ocorrencia.TurmaOrigem,

                                MatrizDestino = ocorrencia.MatrizOrigem,
                                LinhaDestino = ocorrencia.LinhaOrigem,
                                ColunaDestino = ocorrencia.ColunaOrigem,
                                ProfessorDestino = ocorrencia.ProfessorOrigem,
                                TurmaDestino = _TurmaDestino, //ocorrencia.TurmaDestino,

                                TrocaOrigemParaDestinoValida = true,
                                TrocaDestinoParaOrigemValida = true,
                                RealocacaoValida = true,
                                FormaDobradinha = false
                            });
                        }
                    }
                }
            }

            // Adicionar as novas possibilidades à lista final
            listaFiltrada.AddRange(novasPossibilidades);

            return listaFiltrada;
        }



        // Novo método específico para validação em proximidade
        private bool IncluirProfessorVertical_ValidarDuplicidadeNaColunaAcimaDe2Professores(string professor, string[,] matriz, int colunaDestino)
        {


            // Garantir que o índice da coluna está dentro dos limites da matriz
            if (colunaDestino < 0 || colunaDestino >= matriz.GetLength(1))
            {
                Console.WriteLine($"Erro: colunaDestino ({colunaDestino}) está fora dos limites da matriz.");
                return false; // Retorna false se a coluna está fora do intervalo
            }

            // Processar todas as linhas na coluna fornecida
            int totalLinhas = matriz.GetLength(0); // Descobre dinamicamente o total de linhas
            int ocorrencias = 0; // Contador para ocorrências do professor na coluna

            for (int linha = 0; linha < totalLinhas; linha++) // Itera por todas as linhas da coluna especificada
            {

                // 3AULAS
                if (matriz[linha, colunaDestino] == professor) // Verifica duplicidade
                {
                    ocorrencias++; // Incrementa o contador de ocorrências
                    Console.WriteLine($"Conflito parcial: {professor} encontrado na coluna {colunaDestino}, linha {linha}.");
                }
                

                ////////if (ProfessorHelper.ExtrairNomeProfessor(matriz[linha, colunaDestino]) ==
                ////////    ProfessorHelper.ExtrairNomeProfessor(professor)) // Verifica duplicidade
                ////////{
                ////////    ocorrencias++; // Incrementa o contador de ocorrências
                ////////    Console.WriteLine($"Conflito parcial: {professor} encontrado na coluna {colunaDestino}, linha {linha}.");
                ////////}


                // Até uma  ocorrência é aceitável   pois sera somada com a inclusao da nova coluna
                // totalizando no maximo 2
                if (ocorrencias > 1)
                {
                    Console.WriteLine($"Conflito: Mais de uma ocorrência de {professor} na coluna {colunaDestino}.");

                    //nao pode haver 1 ocorrencia na coluna pois é duplicidade de coluna com mesmo professor entendeu
                    
                    return true;
                }
            }

            // Retorna false se as duplicidades forem aceitáveis (0 ou 1 ocorrência)
            Console.WriteLine($"Nenhum conflito crítico encontrado para {professor} na coluna {colunaDestino}.");
            return false;
        }


        private bool IncluirProfessorVertical_ValidarDuplicidadeNaLinhaEmTodasColunas(string professor, string[,] matriz, int colunaAtual, int linhaDestino)
        {
            // Verificar se o índice da linha está dentro dos limites da matriz
            if (linhaDestino < 0 || linhaDestino >= matriz.GetLength(0))
            {
                Console.WriteLine($"Erro: linhaDestino ({linhaDestino}) está fora dos limites da matriz.");
                return false; // Fora dos limites, não é uma duplicidade válida
            }

            // Iterar por todas as colunas da linha fornecida
            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
            {
                // Ignorar a coluna atual porque estou processando ela
                if (coluna == colunaAtual)
                {
                    Console.WriteLine($"Ignorando a coluna {colunaAtual} na verificação de duplicidade.");
                    continue;
                }

                //////// Verificar se o professor está presente em outra coluna na mesma linha
                //////if (matriz[linhaDestino, coluna] == professor)
                //////{
                //////    Console.WriteLine($"Conflito encontrado: {professor} já está na linha {linhaDestino}, coluna {coluna}.");
                //////    return true; // Duplicidade encontrada
                //////}
                ///
                string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linhaDestino, coluna]);
                string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                {
                    Console.WriteLine($"Conflito encontrado: {professor} já está na linha {linhaDestino}, coluna {coluna}.");
                    return true; // Duplicidade encontrada
                }

            }

            // Retornar false se nenhuma duplicidade for encontrada
            Console.WriteLine($"Nenhum conflito encontrado para {professor} na linha {linhaDestino}, excluindo a coluna {colunaAtual}.");
            return false; // Nenhuma duplicidade
        }




        private void IncluirProfessorVertical_ProcessarTrocas(List<ProcessamentoInsercao> listaProcessamento, Dictionary<string, int> progressoProfessores)
        {
            MovimentacaoProfessorFechamento movimentacaoprof = new MovimentacaoProfessorFechamento();
            // Este método será responsável por processar as trocas baseadas nas possibilidades geradas.
            var necessidade = necessidadesProfessores;

            // Variável de controle para indicar se já foi processado
            bool processado = false;

            // Iterar pela lista de processamentos
            foreach (var processamento in listaProcessamento)
            {

                var professor = processamento.PossibilidadeVertical.ProfessorOrigem;

                // Verifica se o professor tem progresso registrado
                if (!progressoProfessores.ContainsKey(professor))
                {
                    progressoProfessores[professor] = 0;
                }

                // Obtém o índice do próximo item a ser processado
                int indiceAtual = progressoProfessores[professor];


                // Verifica se ainda há itens para processar
                if (indiceAtual >= processamento.PossibilidadesHorizontal.Count)
                {
                    Console.WriteLine($"Todas as possibilidades já foram processadas para o professor {professor}.");
                    continue;
                }




                // Verificar se já foi processado
                if (processado)
                    break;



                var obterProcessamentoVertical_ATUAL = processamento.PossibilidadeVertical;
                var obterpossibilidadeHorizontal_ATUAL = processamento.PossibilidadesHorizontal[indiceAtual];
                var ListaProcessamento_Atual = new List<ProcessamentoInsercao>
                {
                    new ProcessamentoInsercao
                    {
                        PossibilidadeVertical = obterProcessamentoVertical_ATUAL,
                        PossibilidadesHorizontal = new List<ProfessorInsercaoPossibilidade>{ obterpossibilidadeHorizontal_ATUAL }
                    }
                };

                // Simular a troca com as matrizes de cópia
                var RetornoSucesso = IncluirProfessorVertical_SimularTroca_MatrizesCopia(ListaProcessamento_Atual);



                // Verificar se o processamento foi bem-sucedido
                if (RetornoSucesso.Count == 0)
                {
                    // Deve continuar com o próximo da lista, pois os dados foram validados
                    continue;
                }




                if (processamento.PossibilidadeVertical.ProfessorOrigem == "ELENICE-GEO")
                {
                    string achou = "RAQUEL";
                }

                if (processamento.PossibilidadeVertical.ProfessorDestino == "ELENICE-GEO")
                {
                    string achou = "RAQUEL";
                }


                if (processamento.PossibilidadeVertical.TurmaDestino == "6W")
                {
                    string achou = "6W";
                }
                // **Etapa 1: Processar a troca Vertical**
                var possibilidadeVertical = processamento.PossibilidadeVertical;

                Console.WriteLine($"Processando troca Vertical: Professor {possibilidadeVertical.ProfessorInserido} sendo inserido.");

                // Obter os grids correspondentes
                var matrizOrigemVertical = IncluirProfessorVertical_ObterMatrizPorDia(possibilidadeVertical.MatrizOrigem);
                var matrizDestinoVertical = IncluirProfessorVertical_ObterMatrizPorDia(possibilidadeVertical.MatrizDestino);

                // Atualizar o grid de origem (remover o professor origem e inserir o professor inserido)
                matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = string.Empty;
                matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = possibilidadeVertical.ProfessorDestino;
                
                //GUARDA OS DADOS INSERIDOS
                movimentacaoprof.ProfessorTrocaTurma = possibilidadeVertical.TurmaProfessorInserido; // esta na mesma turma do professor inserido
                movimentacaoprof.ProfessorTroca = possibilidadeVertical.ProfessorDestino;
                movimentacaoprof.MatrizProfessorTroca = possibilidadeVertical.MatrizDestino;
                movimentacaoprof.MatrizProfessorTrocaLinha = possibilidadeVertical.LinhaDestino;


                // Atualizar o grid  (inserir o professor NOVO)
                matrizOrigemVertical[possibilidadeVertical.LinhaProfessorInserido, possibilidadeVertical.ColunaProfessorInserido] = possibilidadeVertical.ProfessorInserido;
                
                //GUARDA OS DADOS INSERIDOS
                movimentacaoprof.ProfessorInseridoTurma = possibilidadeVertical.TurmaProfessorInserido;
                movimentacaoprof.ProfessorInserido = possibilidadeVertical.ProfessorInserido;
                movimentacaoprof.MatrizProfessorInserido = possibilidadeVertical.MatrizOrigem;
                movimentacaoprof.MatrizProfessorInseridoLinha = possibilidadeVertical.LinhaProfessorInserido;

                Console.WriteLine($"Troca Vertical realizada: {possibilidadeVertical.ProfessorOrigem} -> {possibilidadeVertical.ProfessorInserido}");




                // **Etapa 2: Selecionar e processar um único item Horizontal**
               // subitituido temporariamente  var possibilidadeHorizontal = processamento.PossibilidadesHorizontal.FirstOrDefault();

                // Processa o item atual
                var possibilidadeHorizontal = processamento.PossibilidadesHorizontal[indiceAtual];









                if (possibilidadeHorizontal != null)
                {
                    Console.WriteLine($"Processando troca Horizontal: Professor {possibilidadeHorizontal.ProfessorInserido} sendo inserido.");

                    // Obter os grids correspondentes
                    var matrizOrigemHorizontal = IncluirProfessorVertical_ObterMatrizPorDia(possibilidadeHorizontal.MatrizOrigem);
                    var matrizDestinoHorizontal = IncluirProfessorVertical_ObterMatrizPorDia(possibilidadeHorizontal.MatrizDestino);

                    // Atualizar o grid de origem (remover o professor origem e inserir o professor destino)
                    matrizOrigemHorizontal[possibilidadeHorizontal.LinhaOrigem, possibilidadeHorizontal.ColunaOrigem] = string.Empty;
                    matrizOrigemHorizontal[possibilidadeHorizontal.LinhaOrigem, possibilidadeHorizontal.ColunaOrigem] = possibilidadeHorizontal.ProfessorDestino;

                    //GUARDA DADOS INSERIDOS
                    movimentacaoprof.ProfessorDestinoTurma = possibilidadeHorizontal.TurmaDestino;
                    movimentacaoprof.ProfessorDestino = possibilidadeHorizontal.ProfessorDestino;
                    movimentacaoprof.MatrizProfessorDestino = possibilidadeHorizontal.MatrizOrigem;
                    movimentacaoprof.MatrizProfessorDestinoLinha = possibilidadeHorizontal.LinhaOrigem;




                    // Atualizar o grid de destino (inserir o professor origem no destino)
                    matrizDestinoHorizontal[possibilidadeHorizontal.LinhaDestino, possibilidadeHorizontal.ColunaDestino] = possibilidadeHorizontal.ProfessorOrigem;

                    movimentacaoprof.ProfessorOrigemTurma = possibilidadeHorizontal.TurmaOrigem;
                    movimentacaoprof.ProfessorOrigem = possibilidadeHorizontal.ProfessorOrigem; 
                    movimentacaoprof.MatrizProfessorOrigem = possibilidadeHorizontal.MatrizDestino; // nao esta errado fazemos trocas entre as matrizes
                    movimentacaoprof.MatrizProfessorOrigemLinha = possibilidadeHorizontal.LinhaDestino;


                    this.ListaMovimentacaoProfessorFechamento.Add(movimentacaoprof);

                    //Console.WriteLine($"Troca Horizontal realizada: {possibilidadeHorizontal.ProfessorOrigem} -> {possibilidadeHorizontal.ProfessorInserido}");

                    // **Etapa 3: Atualizar a necessidade do professor**
                    if (possibilidadeVertical.ProfessorInserido.Contains("ELENICE"))
                    {
                        string achei = "ELENICE";
                    }
                    IncluirProfessorVertical_AtualizarNecessidade(necessidade, possibilidadeVertical.ProfessorInserido);

                    // Indicar que o processamento foi concluído
                    processado = true;
                }
                else
                {
                    Console.WriteLine("Nenhuma possibilidade Horizontal disponível para esta troca horizontal.");
                }

                // Confirmar o processamento
                Console.WriteLine($"Processamento de trocas concluído para o professor {possibilidadeVertical.ProfessorInserido}.");
            }
        }



        /// <summary>
        /// Atualiza a necessidade de um professor no dicionário de necessidades.
        /// </summary>
        /// <param name="necessidade">Dicionário de necessidades.</param>
        /// <param name="professorInserido">Nome do professor inserido.</param>
        private void IncluirProfessorVertical_AtualizarNecessidade(Dictionary<string, NecessidadeProfessor> necessidade, string professorInserido)
        {
            // Localizar a chave correspondente
            if (string.IsNullOrEmpty(professorInserido))
            {
                Console.WriteLine("Erro: professor inserido está vazio.");
                return; // Não tente processar um professor vazio
            }

            // Localizar a chave correspondente
            //////var chaveEncontrada = necessidade.Keys.FirstOrDefault(chave => chave.StartsWith(professorInserido + "_"));
            var chaveEncontrada = necessidade.Keys
            .FirstOrDefault(chave => chave.StartsWith(professorInserido + "_") || chave.StartsWith(professorInserido + "-"));


            if (!string.IsNullOrEmpty(chaveEncontrada))
            {
                var necessidadeAtual = necessidade[chaveEncontrada];

                if (necessidadeAtual.QtdAula == 1)
                {
                    // Remover a chave do dicionário se QtdAula for 1
                    Console.WriteLine($"Removendo a necessidade de {chaveEncontrada}, pois QtdAula chegou a zero.");
                    necessidade.Remove(chaveEncontrada);
                }
                else
                {
                    // Decrementar QtdAula se maior que 1
                    necessidadeAtual.QtdAula -= 1;
                    Console.WriteLine($"Atualizando a necessidade de {chaveEncontrada}. Nova QtdAula: {necessidadeAtual.QtdAula}");
                }
            }
            else
            {
                Console.WriteLine($"Erro: chave para o professor {professorInserido} não encontrada em necessidades.");
            }
        }


        public List<ProfessorNecessidadeInsercao> IncluirProfessorVertical_IdentificarNecessidades()
        {
            var necessidadesInsercao = new List<ProfessorNecessidadeInsercao>();


            ////foreach (var necessidade in necessidadesProfessores)
            ////{
            ////    // Extrair professor e turma dinamicamente a partir da chave
            ////    var separadores = new char[] { '_', '-' };
            ////    var chaveDividida = necessidade.Key.Split(separadores, StringSplitOptions.RemoveEmptyEntries);

            ////    if (chaveDividida.Length != 2)
            ////    {
            ////        Console.WriteLine($"Chave inválida: {necessidade.Key}");
            ////        continue;
            ////    }


            ////    string professor = chaveDividida[0];
            ////    string turma = chaveDividida[1];

            ////    // Verificar se a turma existe na lista dinâmica de turmas
            ////    int coluna = this.Turmas.IndexOf(turma);
            ////    if (coluna == -1)
            ////    {
            ////        Console.WriteLine($"Turma {turma} não encontrada na configuração dinâmica de turmas.");
            ////        continue;
            ////    }

            ////    // Iterar pelas matrizes (um dia da semana para cada matriz)
            ////    foreach (var diaSemana in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
            ////    {
            ////        var matriz = IncluirProfessorVertical_ObterMatrizPorDia(diaSemana);
            ////        var horarios = new List<int>();

            ////        for (int linha = 0; linha < matriz.GetLength(0); linha++)
            ////        {
            ////            //////if (matriz[linha, coluna] == professor)
            ////            //////{
            ////            //////    horarios.Add(linha);
            ////            //////}
            ////            ///
            ////            string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
            ////            string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

            ////            if (nomeExtraidoMatriz == nomeExtraidoProfessor)
            ////            {
            ////                horarios.Add(linha);
            ////            }



            ////        }

            ////        if (horarios.Count > 0)
            ////        {
            ////            necessidadesInsercao.Add(new ProfessorNecessidadeInsercao
            ////            {
            ////                Professor = professor,     // Nome do professor
            ////                Turma = turma,             // Turma associada ao professor
            ////                Coluna = coluna,           // Coluna identificada dinamicamente
            ////                DiaSemana = diaSemana,     // Dia da semana correspondente
            ////                Horarios = horarios,       // Lista completa de horários do professor
            ////                QtdAula = necessidade.Value.QtdAula // Quantidade de aulas necessárias
            ////            });

            ////            // Interromper o loop para evitar múltiplos conflitos para o mesmo professor
            ////            break;
            ////        }
            ////    }
            ////}


            foreach (var necessidade in necessidadesProfessores)
            {
                // Extrair professor e turma dinamicamente a partir da chave
                var separadores = new char[] { '_', '-' };
                var chaveDividida = necessidade.Key.Split(separadores, StringSplitOptions.RemoveEmptyEntries);

                // Validar a estrutura da chave
                if (chaveDividida.Length < 2 || chaveDividida.Length > 3)
                {
                    Console.WriteLine($"Chave inválida: {necessidade.Key}");
                    continue;
                }

                // Lógica para determinar professor e turma com base no formato da chave
                string professor;
                string turma;

                if (chaveDividida.Length == 2)
                {
                    professor = chaveDividida[0];
                    turma = chaveDividida[1];
                }
                else if (chaveDividida.Length == 3)
                {
                    professor = chaveDividida[0] + "-" + chaveDividida[1];
                    turma = chaveDividida[2];
                }
                else
                {
                    Console.WriteLine($"Formato inesperado para a chave: {necessidade.Key}");
                    continue;
                }

                // Verificar se a turma existe na lista dinâmica de turmas
                int coluna = this.Turmas.IndexOf(turma);
                if (coluna == -1)
                {
                    Console.WriteLine($"Turma {turma} não encontrada na configuração dinâmica de turmas.");
                    continue;
                }

                // Iterar pelas matrizes (um dia da semana para cada matriz)
                foreach (var diaSemana in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    var matriz = IncluirProfessorVertical_ObterMatrizPorDia(diaSemana);
                    var horarios = new List<int>();

                    for (int linha = 0; linha < matriz.GetLength(0); linha++)
                    {
                        //string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
                        //string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                        string nomeExtraidoMatriz = matriz[linha, coluna];
                        string nomeExtraidoProfessor = professor;

                        if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                        {
                            horarios.Add(linha);
                        }
                    }

                    if (horarios.Count > 0)
                    {
                        necessidadesInsercao.Add(new ProfessorNecessidadeInsercao
                        {
                            Professor = professor,     // Nome do professor
                            Turma = turma,             // Turma associada ao professor
                            Coluna = coluna,           // Coluna identificada dinamicamente
                            DiaSemana = diaSemana,     // Dia da semana correspondente
                            Horarios = horarios,       // Lista completa de horários do professor
                            QtdAula = necessidade.Value.QtdAula // Quantidade de aulas necessárias
                        });

                        // Interromper o loop para evitar múltiplos conflitos para o mesmo professor
                        break;
                    }
                }
            }


            // Retornar a lista de necessidades de inserção identificadas
            return necessidadesInsercao;
        }




        private string[,] IncluirProfessorVertical_ObterMatrizPorDia(string diaSemana)
        {
            // Mapeia os dias da semana para as matrizes correspondentes
            return diaSemana switch
            {
                "Segunda-feira" => matrizHorariosSegunda,
                "Terça-feira" => matrizHorariosTerca,
                "Quarta-feira" => matrizHorariosQuarta,
                "Quinta-feira" => matrizHorariosQuinta,
                "Sexta-feira" => matrizHorariosSexta,
                _ => throw new ArgumentException($"Dia da semana inválido: {diaSemana}")
            };
        }




        // ================================================================================================
        // =======fim MOVER VERTICAL 




        /// <summary> ****************************************************
        /// inicio do bloco 
        //// BLOCO DO CODIGO SAI E VOLTA DOS PROFESSORES 




        public void ResolverSairVoltaAula_MoverHorizontal()
        {
            int maxIteracoes = 1; // Número máximo de iterações
            int iteracaoAtual = 0;



            // Controle de progresso para cada professor
            Dictionary<string, int> progressoProfessores = new Dictionary<string, int>();

            // Identifica necessidades para inclusão do professor
            //var ListprofessoresInserir = ResolverSairVoltaAula_IdentificarNecessidades().ToList();
   



            //Listprofessor = Listprofessor.ToList()
            //                 .Where(x => x.Professor == "IDELSON")
            //                 .Where(x => x.Coluna == 3)
            //                 .Where(x => x.DiaSemana == "Segunda-feira")
            //                 .Take(1)
            //                 .ToList();

            for (int i = 1; i <= maxIteracoes; i++)
            {

                var Listprofessor = ResolverSairVoltaAula_IdentificarNecessidades().ToList();

                if (Listprofessor.Count == 0)
                    i = maxIteracoes + 1;

                foreach (var professoresInserir in Listprofessor)
                {


                   var ListprofessoresInserir = ResolverSairVoltaAula_IdentificarNecessidades().ToList();
                    //.Where(x => x.Professor == professoresInserir.Professor)
                    //.Where(x => x.Coluna == professoresInserir.Coluna)
                    //.Where(x => x.DiaSemana == professoresInserir.DiaSemana)
                    //.Take(1)
                    //.ToList();





                    //nao há registros para processar encerra o processamento.
                    if (ListprofessoresInserir.Count == 0)
                        break;


                    foreach (var professoresInserirAjusta in ListprofessoresInserir)
                    {
                        // Inicializa o progresso do professor, se ainda não existir
                        if (!progressoProfessores.ContainsKey(professoresInserirAjusta.Professor))
                        {
                            progressoProfessores[professoresInserirAjusta.Professor] = 0;
                        }

                        // Gerar possibilidades de troca
                        var ListaProcessamentoInsercao = ResolverSairVoltaAula_GerarPossibilidades(new List<ProfessorNecessidadeInsercao> { professoresInserirAjusta });
                        ListaProcessamentoInsercao = ListaProcessamentoInsercao.ToList().Where(x => x.PossibilidadesHorizontal.Count > 0).ToList();



                        if (!ListaProcessamentoInsercao.Any())
                        {
                            Console.WriteLine($"Sem mais possibilidades após {iteracaoAtual} iterações.");
                            break; // Interrompe o loop se não houver mais possibilidades
                        }

                        // verificar bloqueio e opcoes 
                        //////var ListaProcessamentoInsercaoValidarOpcoes = ResolverSairVoltaAula_ValidarOpcoesProfessores(ListaProcessamentoInsercao);

                        //////List<ProcessamentoInsercao> ListaProfessoresValidados_NaMatrizCopia = new List<ProcessamentoInsercao>();

                        // confrontar os bloqueios dentro da matriz copia
                        // simulação da movimentacao na matriz copia
                        var ListaProfessoresValidados_NaMatrizCopia = new List<ProcessamentoInsercao>();
                        if (ListaProcessamentoInsercao.Count > 0)
                        {
                            ListaProfessoresValidados_NaMatrizCopia = ResolverSairVoltaAula_SimularTroca_MatrizesCopia(ListaProcessamentoInsercao);
                        }

                        // Processar e aplicar a melhor troca
                        if (ListaProfessoresValidados_NaMatrizCopia.Count > 0)
                        {
                            ResolverSairVoltaAula_ProcessarTrocas(ListaProfessoresValidados_NaMatrizCopia, progressoProfessores);

                            // processar um unico  professor 
                            //forcar um break para ele nao distribuir e sim reavalizar toda a matriz 
                            // e obter novamente as opcoes de distribuicao com o status novo da matriz 

                        }

                        var verNecessidade = necessidadesProfessores;
                        iteracaoAtual++;

                        Console.WriteLine($"Processamento concluído após {iteracaoAtual} iterações.");
                    }
                }
            }
        }


        //////////private void ResolverSairVoltaAula_ObterProfessorSaiVolta()
        //////////{
        //////////    // Garantir que a lista esteja inicializada
        //////////    //if (ListaProfessorNecessidadeSaiVolta == null)
        //////////    ListaProfessorNecessidadeSaiVolta = new List<ProfessorNecessidadeSaiVolta>();

        //////////    var diasDaSemana = new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };

        //////////    foreach (var diaSemana in diasDaSemana)
        //////////    {
        //////////        var matriz = ResolverSairVoltaAula_ObterMatrizPorDia(diaSemana);
        //////////        int totalLinhas = matriz.GetLength(0);
        //////////        int totalColunas = matriz.GetLength(1);

        //////////        // Processar cada coluna da matriz
        //////////        for (int coluna = 0; coluna < totalColunas; coluna++)
        //////////        {
        //////////            var nomesDistintos = new HashSet<string>();
        //////////            var ocorrenciasProfessores = new Dictionary<string, List<int>>();

        //////////            // Passo 1: Obter os nomes distintos e mapear suas linhas
        //////////            for (int linha = 0; linha < totalLinhas; linha++)
        //////////            {
        //////////                string professorAtual = matriz[linha, coluna];

        //////////                if (!string.IsNullOrEmpty(professorAtual))
        //////////                {
        //////////                    nomesDistintos.Add(professorAtual);

        //////////                    if (!ocorrenciasProfessores.ContainsKey(professorAtual))
        //////////                        ocorrenciasProfessores[professorAtual] = new List<int>();

        //////////                    ocorrenciasProfessores[professorAtual].Add(linha);
        //////////                }
        //////////            }

        //////////            // Passo 2: Verificar os professores com "saída e retorno"
        //////////            foreach (var professor in nomesDistintos)
        //////////            {
        //////////                var linhas = ocorrenciasProfessores[professor];

        //////////                if (linhas.Count > 1) // Apareceu mais de uma vez
        //////////                {
        //////////                    bool temDobrada = true;

        //////////                    // Verificar se as linhas são consecutivas
        //////////                    for (int i = 1; i < linhas.Count; i++)
        //////////                    {
        //////////                        if (linhas[i] != linhas[i - 1] + 1)
        //////////                        {
        //////////                            temDobrada = false; // Não é uma dobradinha
        //////////                            break;
        //////////                        }
        //////////                    }

        //////////                    // Se não for dobradinha, adiciona o professor à lista
        //////////                    if (!temDobrada)
        //////////                    {
        //////////                        if (!ListaProfessorNecessidadeSaiVolta.Any(p =>
        //////////                            p.Professor == professor &&
        //////////                            p.Coluna == coluna &&
        //////////                            p.DiaSemana == diaSemana))
        //////////                        {
        //////////                            ListaProfessorNecessidadeSaiVolta.Add(new ProfessorNecessidadeSaiVolta
        //////////                            {
        //////////                                Professor = professor,
        //////////                                Coluna = coluna,
        //////////                                Linha = linhas[0], // Primeira ocorrência do professor
        //////////                                Turma = Turmas[coluna], // Nome da turma (coluna)
        //////////                                DiaSemana = diaSemana
        //////////                            });

        //////////                            Console.WriteLine($"Professor '{professor}' adicionado com saída e retorno na turma {Turmas[coluna]} no dia {diaSemana}.");
        //////////                        }
        //////////                    }
        //////////                }
        //////////            }
        //////////        }
        //////////    }
        //////////}





        private void ResolverSairVoltaAula_ObterProfessorSaiVolta()
        {
            ListaProfessorNecessidadeSaiVolta = new List<ProfessorNecessidadeSaiVolta>();

            var diasDaSemana = new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };

            foreach (var diaSemana in diasDaSemana)
            {
                var matriz = ResolverSairVoltaAula_ObterMatrizPorDia(diaSemana);
                int totalLinhas = matriz.GetLength(0);
                int totalColunas = matriz.GetLength(1);

                for (int coluna = 0; coluna < totalColunas; coluna++)
                {
                    var nomesDistintos = new HashSet<string>();
                    var ocorrenciasProfessores = new Dictionary<string, List<int>>();

                    for (int linha = 0; linha < totalLinhas; linha++)
                    {
                        string professorAtual = matriz[linha, coluna];

                        if (!string.IsNullOrEmpty(professorAtual))
                        {
                            nomesDistintos.Add(professorAtual);

                            if (!ocorrenciasProfessores.ContainsKey(professorAtual))
                                ocorrenciasProfessores[professorAtual] = new List<int>();

                            ocorrenciasProfessores[professorAtual].Add(linha);
                        }
                    }

                    var professoresJaAdicionados = new HashSet<string>();

                    foreach (var professor in nomesDistintos)
                    {
                        var linhas = ocorrenciasProfessores[professor];

                        if (linhas.Count > 1)
                        {
                            bool temDobrada = true;

                            for (int i = 1; i < linhas.Count; i++)
                            {
                                if (linhas[i] != linhas[i - 1] + 1)
                                {
                                    temDobrada = false;
                                    break;
                                }
                            }

                            if (!temDobrada && !professoresJaAdicionados.Contains(professor))
                            {
                                ListaProfessorNecessidadeSaiVolta.Add(new ProfessorNecessidadeSaiVolta
                                {
                                    Professor = professor,
                                    Coluna = coluna,
                                    Linha = linhas[0],
                                    Turma = Turmas[coluna],
                                    DiaSemana = diaSemana
                                });

                                professoresJaAdicionados.Add(professor);
                                Console.WriteLine($"Professor '{professor}' adicionado com saída e retorno na turma {Turmas[coluna]} no dia {diaSemana}.");
                            }
                        }
                    }
                }
            }
        }



        private List<ProcessamentoInsercao> ResolverSairVoltaAula_SimularTroca_MatrizesCopia(List<ProcessamentoInsercao> ListaProcessamentoInsercaoValidarOpcoes)
        {
            // Nova lista para armazenar os processamentos válidos
            var listaFiltrada = new List<ProcessamentoInsercao>();

            bool valideVertical_Destino = false;
            bool valideVertical_Inserido = false;
            bool valideHorizontal_Origem = false;
            bool valideHorizontal_Destino = false;

            bool validaColuna = false;
            bool validaLinha = false;
            foreach (var processamento in ListaProcessamentoInsercaoValidarOpcoes)
            {
                // vamos processar  cada linha do vertical para validar os dados 
                // e dentro do horizontal vamos fazer os dois tipos de dados
                // vertical e horizontal dos professores

                var possibilidadesHorizontalValidas = new List<ProfessorInsercaoPossibilidade>();





                foreach (var processamento_Horizontal in processamento.PossibilidadesHorizontal)
                {

                    //reinicia as variaveis 
                    valideVertical_Destino = false;
                    valideVertical_Inserido = false;
                    valideHorizontal_Origem = false;
                    valideHorizontal_Destino = false;
                    validaColuna = false;
                    validaLinha = false;

                    // Criar cópias das matrizes originais para esta iteração do primeiro loop
                    var copiaMatriz_Segunda = CopiarMatriz(matrizHorariosSegunda);
                    var copiaMatriz_Terca = CopiarMatriz(matrizHorariosTerca);
                    var copiaMatriz_Quarta = CopiarMatriz(matrizHorariosQuarta);
                    var copiaMatriz_Quinta = CopiarMatriz(matrizHorariosQuinta);
                    var copiaMatriz_Sexta = CopiarMatriz(matrizHorariosSexta);


                    // **Etapa 1: Processar a troca Vertical**
                    var possibilidadeVertical = processamento.PossibilidadeVertical;





                    if (possibilidadeVertical.ProfessorOrigem == "IDELSON")
                    {
                        if (possibilidadeVertical.TurmaOrigem == "7W")
                        {
                            string achoue = "IDELSON";
                        }
                    }

                    if (possibilidadeVertical.ProfessorDestino == "IDELSON")
                    {
                        if (possibilidadeVertical.TurmaOrigem == "7W")
                        {
                            string achoue = "IDELSON";
                        }
                    }



                    // Obter os grids correspondentes
                    // Obter os grids correspondentes de forma dinâmica
                    string diaSemanaOrigem = possibilidadeVertical.MatrizOrigem;
                    var matrizOrigemVertical = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };

                    diaSemanaOrigem = possibilidadeVertical.MatrizDestino;
                    var matrizDestinoVertical = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };


                    // ----------------------------------------------------------------------------
                    // # professor 1 vertical

                    // Atualizar o grid de origem (remover o professor origem e inserir o professor inserido)
                    matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = string.Empty;
                    matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = possibilidadeVertical.ProfessorOrigem; /// REALIZANDO A INVERSAO DOS NOMES 


                    // ----------------------------------------------------------------------------
                    // Professor 2
                    // Atualizar o grid  (inserir o professor NOVO)
                    matrizOrigemVertical[possibilidadeVertical.LinhaOrigem, possibilidadeVertical.ColunaOrigem] = possibilidadeVertical.ProfessorDestino;



                    // ----------------------------------------------------------------------------
                    // troca professor 3


                    // Obter os grids correspondentes
                    diaSemanaOrigem = processamento_Horizontal.MatrizOrigem;
                    var matrizOrigemHorizontal = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };

                    diaSemanaOrigem = processamento_Horizontal.MatrizDestino;
                    var matrizDestinoHorizontal = diaSemanaOrigem switch
                    {
                        "Segunda-feira" => copiaMatriz_Segunda,
                        "Terça-feira" => copiaMatriz_Terca,
                        "Quarta-feira" => copiaMatriz_Quarta,
                        "Quinta-feira" => copiaMatriz_Quinta,
                        "Sexta-feira" => copiaMatriz_Sexta,
                        _ => throw new ArgumentException($"Dia da semana inválido: {diaSemanaOrigem}")
                    };

                    // # professor 3 Horizontal

                    // Atualizar o grid de origem (remover o professor origem e inserir o professor destino)
                    matrizOrigemHorizontal[processamento_Horizontal.LinhaOrigem, processamento_Horizontal.ColunaOrigem] = string.Empty;
                    matrizOrigemHorizontal[processamento_Horizontal.LinhaOrigem, processamento_Horizontal.ColunaOrigem] = processamento_Horizontal.ProfessorDestino;

                    // ----------------------------------------------------------------------------
                    // troca professor 4

                    // Atualizar o grid de destino (inserir o professor origem no destino)
                    matrizDestinoHorizontal[processamento_Horizontal.LinhaDestino, processamento_Horizontal.ColunaDestino] = processamento_Horizontal.ProfessorOrigem;


                    
                    // ----------------------------------------------------------------------------
                    ////validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(possibilidadeVertical.ProfessorDestino, matrizDestinoVertical, possibilidadeVertical.LinhaDestino);
                    ////validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(possibilidadeVertical.ProfessorDestino, matrizDestinoVertical, possibilidadeVertical.ColunaDestino);

                    validaColuna = ResolverSairVoltaAula_Professor_Colunas_Na_Mesma_Linha(possibilidadeVertical.ProfessorDestino, matrizDestinoVertical, possibilidadeVertical.LinhaDestino);
                    validaLinha = ResolverSairVoltaAula_Contar_Professor_Colunas(possibilidadeVertical.ProfessorDestino, matrizDestinoVertical, possibilidadeVertical.ColunaDestino);



                    if (validaColuna && validaLinha)
                    {
                        valideVertical_Destino = true;


                        // vamos validar se os professores nao estão bloqueados para a opcao inserida
                        string abreviacaoProfessorDestino = $"{possibilidadeVertical.MatrizDestino.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaDestino}";
                        // Obter as opções de bloqueio dos professores com validação robusta
                        string listOpcoesOrigemVertical_ProfessorDestino =
                            !string.IsNullOrEmpty(possibilidadeVertical.ProfessorDestino) &&
                            this.Professores.ContainsKey(possibilidadeVertical.ProfessorDestino)
                                ? this.Professores[possibilidadeVertical.ProfessorDestino].Item3
                                : string.Empty;


                        // Validar se os professores estão bloqueados nos horários horizontais
                        if (listOpcoesOrigemVertical_ProfessorDestino.Contains(abreviacaoProfessorDestino))
                        {
                            // muda para false nao passou
                            valideVertical_Destino = false;
                        }

                    }
                    else
                    {
                        // Se qualquer validação falhar, a troca não é válida
                        valideVertical_Destino = false;
                    }

                    if (possibilidadeVertical.ColunaOrigem == 4)
                    {
                        string achou = "IDELSON";
                    }


                    // # professor 2 vertical

                    //////validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(possibilidadeVertical.ProfessorOrigem, matrizOrigemVertical, possibilidadeVertical.LinhaOrigem);
                    //////validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(possibilidadeVertical.ProfessorOrigem, matrizOrigemVertical, possibilidadeVertical.ColunaOrigem);

                    validaColuna = ResolverSairVoltaAula_Professor_Colunas_Na_Mesma_Linha(possibilidadeVertical.ProfessorOrigem, matrizOrigemVertical, possibilidadeVertical.LinhaOrigem);
                    validaLinha = ResolverSairVoltaAula_Contar_Professor_Colunas(possibilidadeVertical.ProfessorOrigem, matrizOrigemVertical, possibilidadeVertical.ColunaOrigem);


                    if (validaColuna && validaLinha)
                    {
                        valideVertical_Inserido = true; // Inicialmente, consideramos como válido

                        // PAREI AQUI NO DEBUG COM A ELENICE SE DER PROBLEM CONTINAR A PARTIR DAQUI EU RETIREI O +1 DO FINAL DA LINHA
                        string abreviacaoProfessorInserido = $"{possibilidadeVertical.MatrizOrigem.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaOrigem + 1}";
                        string listOpcoesDestinoVertical_ProfessorInserido =
                            !string.IsNullOrEmpty(possibilidadeVertical.ProfessorOrigem) &&
                            this.Professores.ContainsKey(possibilidadeVertical.ProfessorOrigem)
                                ? this.Professores[possibilidadeVertical.ProfessorOrigem].Item3
                                : string.Empty;


                        if (listOpcoesDestinoVertical_ProfessorInserido.Contains(abreviacaoProfessorInserido))
                        {
                            // muda para false nao passou
                            valideVertical_Inserido = false;
                        }


                    }
                    else
                    {
                        // Se qualquer validação falhar, a troca não é válida
                        valideVertical_Inserido = false;
                    }

                    // ------------------------------------------------------


                    // **Etapa 2: Selecionar e processar um único item Horizontal**
                    if (processamento_Horizontal != null)
                    {
                        Console.WriteLine($"Processando troca Horizontal: Professor {processamento_Horizontal.ProfessorInserido} sendo inserido.");

                        //////validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(processamento_Horizontal.ProfessorDestino, matrizOrigemHorizontal, processamento_Horizontal.LinhaOrigem);
                        //////validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(processamento_Horizontal.ProfessorDestino, matrizOrigemHorizontal, processamento_Horizontal.ColunaOrigem);

                        validaColuna = ResolverSairVoltaAula_Professor_Colunas_Na_Mesma_Linha(processamento_Horizontal.ProfessorDestino, matrizOrigemHorizontal, processamento_Horizontal.LinhaOrigem);
                        validaLinha = ResolverSairVoltaAula_Contar_Professor_Colunas(processamento_Horizontal.ProfessorDestino, matrizOrigemHorizontal, processamento_Horizontal.ColunaOrigem);




                        if (validaColuna && validaLinha)
                        {
                            valideHorizontal_Origem = true;

                            // ATENCAO TEM QUE VALIDAR IGUAL ENCIMA NA MESMA MATRIZ VALIDADA ONDE O PROFESSOR FOI GRAVADO
                            string abreviacaoOrigemHorizontal_ProfessorDestino = $"{processamento_Horizontal.MatrizOrigem.Substring(0, 3).ToUpper()}{processamento_Horizontal.LinhaOrigem + 1}";

                            // Obter as opções de bloqueio dos professores com validação robusta
                            string listOpcoesOrigemHorizontal_ProfessorDestino =
                                !string.IsNullOrEmpty(processamento_Horizontal.ProfessorDestino) &&
                                this.Professores.ContainsKey(processamento_Horizontal.ProfessorDestino)
                                    ? this.Professores[processamento_Horizontal.ProfessorDestino].Item3
                                    : string.Empty;

                            if (listOpcoesOrigemHorizontal_ProfessorDestino.Contains(abreviacaoOrigemHorizontal_ProfessorDestino))
                            {
                                valideHorizontal_Origem = false;
                            }

                        }
                        else
                        {
                            // Se qualquer validação falhar, a troca não é válida
                            valideHorizontal_Origem = false;
                        }


                        // # professor 4 Horizontal                    

                        //LINHA 4 COLUNA 1  segunda feira

                        ////////validaColuna = IncluirProfessorVertical_Professor_Colunas_Na_Mesma_Linha(processamento_Horizontal.ProfessorOrigem, matrizDestinoHorizontal, processamento_Horizontal.LinhaDestino);
                        ////////validaLinha = IncluirProfessorVertical_Contar_Professor_Colunas(processamento_Horizontal.ProfessorOrigem, matrizDestinoHorizontal, processamento_Horizontal.ColunaDestino);

                        validaColuna = ResolverSairVoltaAula_Professor_Colunas_Na_Mesma_Linha(processamento_Horizontal.ProfessorOrigem, matrizDestinoHorizontal, processamento_Horizontal.LinhaDestino);
                        validaLinha = ResolverSairVoltaAula_Contar_Professor_Colunas(processamento_Horizontal.ProfessorOrigem, matrizDestinoHorizontal, processamento_Horizontal.ColunaDestino);




                        if (validaColuna && validaLinha)
                        {
                            valideHorizontal_Destino = true;


                            string abreviacaoOrigemHorizontal_ProfessorOrigem = $"{processamento_Horizontal.MatrizDestino.Substring(0, 3).ToUpper()}{processamento_Horizontal.LinhaDestino + 1}";

                            // Obter as opções de bloqueio dos professores com validação robusta
                            string listOpcoesOrigemHorizontal_ProfessorOrigem =
                                !string.IsNullOrEmpty(processamento_Horizontal.ProfessorOrigem) &&
                                this.Professores.ContainsKey(processamento_Horizontal.ProfessorOrigem)
                                    ? this.Professores[processamento_Horizontal.ProfessorOrigem].Item3
                                    : string.Empty;

                            if (listOpcoesOrigemHorizontal_ProfessorOrigem.Contains(abreviacaoOrigemHorizontal_ProfessorOrigem))
                            {
                                valideHorizontal_Destino = false;
                            }

                        }
                        else
                        {
                            // Se qualquer validação falhar, a troca não é válida
                            valideHorizontal_Destino = false;
                        }


                        if (valideVertical_Destino && valideVertical_Inserido && valideHorizontal_Origem && valideHorizontal_Destino)
                        {
                            // Adiciona a possibilidade horizontal à lista de válidas
                            possibilidadesHorizontalValidas.Add(processamento_Horizontal);
                        }
                    }
                }


                // Após processar todas as possibilidades horizontais para essa vertical

                if (possibilidadesHorizontalValidas.Any())
                {
                    var novoProcessamento = new ProcessamentoInsercao
                    {
                        PossibilidadeVertical = processamento.PossibilidadeVertical,
                        PossibilidadesHorizontal = possibilidadesHorizontalValidas
                    };

                    listaFiltrada.Add(novoProcessamento);
                }


            }

            return listaFiltrada;
        }




        private bool ResolverSairVoltaAula_Professor_Colunas_Na_Mesma_Linha(string professor, string[,] matriz, int linha)
        {
            // Contador para ocorrências do professor na linha
            int contador = 0;

            // Iterar por todas as colunas baseando-se no tamanho da lista `Turmas`
            for (int coluna = 0; coluna < this.Turmas.Count; coluna++)
            {
                //////// Verificar se o professor está na célula da matriz correspondente
                //////if (matriz[linha, coluna] == professor)
                //////{
                //////    contador++;
                //////    // Se o professor aparecer mais de uma vez, retorna **false** indicando problema
                //////    if (contador > 1)
                //////    {
                //////        Console.WriteLine($"Professor '{professor}' encontrado mais de uma vez na linha {linha + 1}.");
                //////        return false; // Indica problema
                //////    }
                //////}
                ///
                // Verificar se o professor está na célula da matriz correspondente
                if (ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]) ==
                    ProfessorHelper.ExtrairNomeProfessor(professor))
                {
                    contador++;
                    // Se o professor aparecer mais de uma vez, retorna **false** indicando problema
                    if (contador > 1)
                    {
                        Console.WriteLine($"Professor '{ProfessorHelper.ExtrairNomeProfessor(professor)}' encontrado mais de uma vez na linha {linha + 1}.");
                        return false; // Indica problema
                    }
                }

            }

            // Retorna **true** se o professor apareceu no máximo uma vez (sem problemas)
            return true;
        }


        private bool ResolverSairVoltaAula_Contar_Professor_Colunas(string professor, string[,] matriz, int colunafixa)
        {
            // Contador para ocorrências do professor na coluna
            int contador = 0;

            // Verificar se a coluna fornecida está dentro dos limites válidos
            if (colunafixa < 0 || colunafixa >= this.Turmas.Count)
            {
                Console.WriteLine($"Coluna inválida: {colunafixa}. Não está dentro do intervalo permitido.");
                return false; // Indica problema
            }

            // Iterar sobre todas as linhas da matriz para a coluna fixa
            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
               // Verificar se o professor está na célula da matriz correspondente
                if (matriz[linha, colunafixa] == professor)
                {
                    contador++;
                }
                
                ////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, colunafixa]);
                ////string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                ////if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                ////{
                ////    contador++;
                ////}
            }



            if (contador == 3)
            {
                if (professor == "IDELSON")
                {
                    string achei = "xxx";
                }
            }
            // Exibir o número de ocorrências encontradas para debug
            Console.WriteLine($"Professor '{professor}' encontrado {contador} vez(es) na coluna {colunafixa}.");

            // Retorna **true** se o professor está dentro do limite permitido (no máximo 2)
            return contador <= 2;
        }


        private List<ProcessamentoInsercao> ResolverSairVoltaAula_ValidarOpcoesProfessores(List<ProcessamentoInsercao> listaProcessamentoInsercao)
        {
            // Nova lista para armazenar os processamentos válidos
            var listaFiltrada = new List<ProcessamentoInsercao>();

            foreach (var processamento in listaProcessamentoInsercao)
            {
                // **Validação Vertical**
                var possibilidadeVertical = processamento.PossibilidadeVertical;

                // Gerar abreviação para a origem e destino possibilidadeVertical
                string abreviacaoProfessorOrigem = $"{possibilidadeVertical.MatrizDestino.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaDestino + 1}";
                string abreviacaoProfessorInserido = $"{possibilidadeVertical.MatrizOrigem.Substring(0, 3).ToUpper()}{possibilidadeVertical.LinhaProfessorInserido + 1}";

                string listOpcoesOrigemVertical_ProfessorOrigem =
                    !string.IsNullOrEmpty(possibilidadeVertical.ProfessorOrigem) &&
                    this.Professores.ContainsKey(possibilidadeVertical.ProfessorOrigem)
                        ? this.Professores[possibilidadeVertical.ProfessorOrigem].Item3
                        : string.Empty;

                string listOpcoesDestinoVertical_ProfessorInserido =
                    !string.IsNullOrEmpty(possibilidadeVertical.ProfessorInserido) &&
                    this.Professores.ContainsKey(possibilidadeVertical.ProfessorInserido)
                        ? this.Professores[possibilidadeVertical.ProfessorInserido].Item3
                        : string.Empty;

                // Validar se os professores estão bloqueados nos horários horizontais
                if (listOpcoesOrigemVertical_ProfessorOrigem.Contains(abreviacaoProfessorOrigem))
                {
                    Console.WriteLine($"Troca inválida: {possibilidadeVertical.ProfessorOrigem} está bloqueado para {abreviacaoProfessorOrigem}.");
                    continue;
                }

                if (listOpcoesDestinoVertical_ProfessorInserido.Contains(abreviacaoProfessorInserido))
                {
                    Console.WriteLine($"Troca inválida: {possibilidadeVertical.ProfessorInserido} está bloqueado para {abreviacaoProfessorInserido}.");
                    continue;
                }

                // **Validação Horizontal**
                var listaHorizontalValidas = new List<ProfessorInsercaoPossibilidade>();

                foreach (var possibilidadeHorizontal in processamento.PossibilidadesHorizontal)
                {
                    // Obter as opções de bloqueio dos professores com validação robusta
                    string listOpcoesOrigemHorizontal_ProfessorOrigem =
                        !string.IsNullOrEmpty(possibilidadeHorizontal.ProfessorOrigem) &&
                        this.Professores.ContainsKey(possibilidadeHorizontal.ProfessorOrigem)
                            ? this.Professores[possibilidadeHorizontal.ProfessorOrigem].Item3
                            : string.Empty;

                    string listOpcoesDestinoHorizontal_ProfessorDestino =
                        !string.IsNullOrEmpty(possibilidadeHorizontal.ProfessorDestino) &&
                        this.Professores.ContainsKey(possibilidadeHorizontal.ProfessorDestino)
                            ? this.Professores[possibilidadeHorizontal.ProfessorDestino].Item3
                            : string.Empty;

                    // Gerar abreviação para a origem e destino Horizontal
                    string abreviacaoOrigemHorizontal_ProfessorOrigem = $"{possibilidadeHorizontal.MatrizDestino.Substring(0, 3).ToUpper()}{possibilidadeHorizontal.LinhaDestino + 1}";
                    string abreviacaoDestinoHorizontal_ProfessorDestino = $"{possibilidadeHorizontal.MatrizOrigem.Substring(0, 3).ToUpper()}{possibilidadeHorizontal.LinhaOrigem + 1}";

                    // Validar se os professores estão bloqueados nos horários verticais
                    if (listOpcoesOrigemHorizontal_ProfessorOrigem.Contains(abreviacaoOrigemHorizontal_ProfessorOrigem))
                    {
                        Console.WriteLine($"Troca inválida: {possibilidadeHorizontal.ProfessorOrigem} está bloqueado para {abreviacaoOrigemHorizontal_ProfessorOrigem}.");
                        continue;
                    }

                    if (listOpcoesDestinoHorizontal_ProfessorDestino.Contains(abreviacaoDestinoHorizontal_ProfessorDestino))
                    {
                        Console.WriteLine($"Troca inválida: {possibilidadeHorizontal.ProfessorInserido} está bloqueado para {abreviacaoDestinoHorizontal_ProfessorDestino}.");
                        continue;
                    }

                    // Adicionar à lista Horizontal válida
                    listaHorizontalValidas.Add(possibilidadeHorizontal);
                }

                // Adicionar o processamento apenas se houver pelo menos uma possibilidade Horizontal válida
                if (listaHorizontalValidas.Any())
                {
                    listaFiltrada.Add(new ProcessamentoInsercao
                    {
                        PossibilidadeVertical = possibilidadeVertical,
                        PossibilidadesHorizontal = listaHorizontalValidas
                    });
                }
            }

            Console.WriteLine($"Total de processamentos válidos: {listaFiltrada.Count}");
            return listaFiltrada;
        }



        public List<ProcessamentoInsercao> ResolverSairVoltaAula_GerarPossibilidades(List<ProfessorNecessidadeInsercao> professoresInserir)
        {
            var listaProcessamento = new List<ProcessamentoInsercao>();

            // **Primeiro Fluxo**: Processa as trocas que formam dobradinhas
            //listaProcessamento.AddRange(ResolverSairVoltaAula_ResolverTrocasPorTipo(professoresInserir, true));

            // **Segundo Fluxo**: Processa as trocas simples (sem dobradinhas)
            listaProcessamento.AddRange(ResolverSairVoltaAula_ResolverTrocasPorTipo(professoresInserir, false));

            return listaProcessamento;
        }

        /// <summary>
        /// Processa as trocas, separando dobradinhas e trocas simples.
        /// </summary>
        private List<ProcessamentoInsercao> ResolverSairVoltaAula_ResolverTrocasPorTipo(List<ProfessorNecessidadeInsercao> professoresInserir, bool apenasDobradinhas)
        {
            var listaPossibilidades = new List<ProcessamentoInsercao>();

            foreach (var necessidade in professoresInserir)
            {
                foreach (var diaSemanaOrigem in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    var matrizOrigem = ResolverSairVoltaAula_ObterMatrizPorDia(diaSemanaOrigem);

                    for (int linhaOrigem = 0; linhaOrigem < matrizOrigem.GetLength(0); linhaOrigem++)
                    {
                        var professorOrigem = matrizOrigem[linhaOrigem, necessidade.Coluna];
                        var turmaOrigem = this.Turmas[necessidade.Coluna];

                        // Ignora o próprio professor
                        if (professorOrigem == necessidade.Professor) continue;

                        // Verifica se o professorOrigem já possui dobradinha
                        bool origemTemDobrada = TemDobradaNaColuna(professorOrigem, matrizOrigem, necessidade.Coluna);

                        foreach (var diaSemanaDestino in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                        {
                            var matrizDestino = ResolverSairVoltaAula_ObterMatrizPorDia(diaSemanaDestino);

                            for (int linhaDestino = 0; linhaDestino < matrizDestino.GetLength(0); linhaDestino++)
                            {
                                var professorDestino = matrizDestino[linhaDestino, necessidade.Coluna];
                                var turmaDestino = this.Turmas[necessidade.Coluna];

                                // Ignora posições inválidas
                                if (professorDestino == necessidade.Professor)  continue;//  || professorDestino == professorOrigem
                                if (diaSemanaOrigem == diaSemanaDestino && linhaOrigem == linhaDestino) continue;

                                // Verifica se o professorDestino já possui dobradinha
                                bool destinoTemDobrada = TemDobradaNaColuna(professorDestino, matrizDestino, necessidade.Coluna);

                                // Se estamos no fluxo "apenasDobradinhas", ignora trocas simples
                                if (apenasDobradinhas && (origemTemDobrada || destinoTemDobrada))
                                    continue;

                                // Se estamos no fluxo "trocas simples", ignora se ambos já têm dobradinha
                                if (!apenasDobradinhas && (origemTemDobrada || destinoTemDobrada))
                                    continue;




                                var possibilidade = new ProfessorInsercaoPossibilidade
                                {
                                    //backup
                                    //MatrizOrigem = diaSemanaOrigem,
                                    //LinhaOrigem = linhaOrigem,
                                    //ColunaOrigem = necessidade.Coluna,
                                    //ProfessorOrigem = professorOrigem,
                                    //TurmaOrigem = turmaOrigem,

                                    MatrizOrigem = diaSemanaOrigem,
                                    LinhaOrigem = necessidade.Horarios[0], //linhaOrigem,
                                    ColunaOrigem = necessidade.Coluna,
                                    ProfessorOrigem = necessidade.Professor,
                                    TurmaOrigem = turmaDestino,


                                    //aqui
                                    MatrizDestino = diaSemanaDestino,
                                    LinhaDestino = linhaDestino,
                                    ColunaDestino = necessidade.Coluna,
                                    ProfessorDestino = professorDestino,
                                    TurmaDestino = turmaDestino,


                                    //backup
                                    //ProfessorInserido = necessidade.Professor,
                                    //LinhaProfessorInserido = necessidade.Horarios[0], //linhaOrigem,
                                    //ColunaProfessorInserido = necessidade.Coluna,
                                    //TurmaProfessorInserido = turmaDestino,

                                    InsercaoValida = true,
                                    RealocacaoValida = true,
                                    FormaDobradinha = apenasDobradinhas // True se for dobradinha, False se for troca simples
                                };

                                var possibilidadesHorizontal = ResolverSairVoltaAula_FiltrarPossibilidades(
                                new List<ProfessorInsercaoPossibilidade> { possibilidade });

                                listaPossibilidades.Add(new ProcessamentoInsercao
                                {
                                    PossibilidadeVertical = possibilidade,
                                    PossibilidadesHorizontal = possibilidadesHorizontal
                                });
                            }
                        }
                    }
                }
            }

            return listaPossibilidades;
        }






        /// <summary>
        /// Verifica se um professor possui uma dobradinha em uma coluna.
        /// </summary>
        private bool TemDobradaNaColuna(string professor, string[,] matriz, int coluna)
        {
            int contador = 0;

            for (int linha = 0; linha < matriz.GetLength(0); linha++)
            {
                if (matriz[linha, coluna] == professor)
                    contador++;
                
                ////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, coluna]);
                ////string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                ////if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                ////{
                ////    contador++;
                ////}


                if (contador >= 2)
                    return true; // Existe uma dobradinha
            }

            return false;
        }



        // Atualização do método para filtrar possibilidades de inserção
        private List<ProfessorInsercaoPossibilidade> ResolverSairVoltaAula_FiltrarPossibilidades(List<ProfessorInsercaoPossibilidade> possibilidades)
        {
            var listaFiltrada = new List<ProfessorInsercaoPossibilidade>();
            var listaFiltradaOcorrencia = new List<ProfessorInsercaoPossibilidade>();

            foreach (var possibilidade in possibilidades)
            {
                // Obter a matriz de destino
                var matrizDestino = ResolverSairVoltaAula_ObterMatrizPorDia(possibilidade.MatrizDestino);

                // Verificar duplicidade para o professor na origem
                bool duplicidadeProfessorOrigem = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                    possibilidade.ProfessorOrigem, matrizDestino, possibilidade.ColunaDestino);

                // Processar apenas se não houver duplicidade
                if (!duplicidadeProfessorOrigem)
                {
                    var ocorrencias = new List<ProfessorInsercaoPossibilidade>();

                    for (int coluna = 0; coluna < matrizDestino.GetLength(1); coluna++)
                    {
                        if (coluna == possibilidade.ColunaDestino) continue;

                        if (matrizDestino[possibilidade.LinhaDestino, coluna] == possibilidade.ProfessorOrigem)
                        {
                            var _TurmaOrigem = this.Turmas[coluna];
                            var novaPossibilidade = new ProfessorInsercaoPossibilidade
                            {
                                MatrizOrigem = possibilidade.MatrizDestino,
                                LinhaOrigem = possibilidade.LinhaDestino,
                                ColunaOrigem = coluna,
                                ProfessorOrigem = possibilidade.ProfessorOrigem,
                                TurmaOrigem = _TurmaOrigem,
                                ProfessorDestino = "BUSCAR PROFESSOR PARA TROCA",
                                InsercaoValida = false,
                                RealocacaoValida = false,
                                FormaDobradinha = false
                            };

                            ocorrencias.Add(novaPossibilidade);
                        }
                    }

                    listaFiltradaOcorrencia.AddRange(ocorrencias);
                }
            }

            var novasPossibilidades = new List<ProfessorInsercaoPossibilidade>();

            foreach (var ocorrencia in listaFiltradaOcorrencia)
            {
                var matrizOrigemVertical = ResolverSairVoltaAula_ObterMatrizPorDia(ocorrencia.MatrizOrigem);

                foreach (var diaSemana in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    var matrizDestinoVerticalDaTroca = ResolverSairVoltaAula_ObterMatrizPorDia(diaSemana);

                    for (int linhaDaTroca = 0; linhaDaTroca < matrizDestinoVerticalDaTroca.GetLength(0); linhaDaTroca++)
                    {
                        var professorDestino = matrizDestinoVerticalDaTroca[linhaDaTroca, ocorrencia.ColunaOrigem];

                        if (string.IsNullOrEmpty(professorDestino)) continue;
                        if (professorDestino == ocorrencia.ProfessorOrigem) continue;

                        bool duplicidadeColunaDestino = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                            ocorrencia.ProfessorOrigem, matrizDestinoVerticalDaTroca, ocorrencia.ColunaOrigem);

                        bool duplicidadeLinhaDestino = ResolverSairVoltaAula_ValidarDuplicidadeNaLinhaEmTodasColunas(
                            ocorrencia.ProfessorOrigem, matrizDestinoVerticalDaTroca, ocorrencia.ColunaOrigem, linhaDaTroca);

                        bool duplicidadeColunaOrigem = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                            professorDestino, matrizOrigemVertical, ocorrencia.ColunaOrigem);

                        bool duplicidadeLinhaOrigem = ResolverSairVoltaAula_ValidarDuplicidadeNaLinhaEmTodasColunas(
                            professorDestino, matrizOrigemVertical, ocorrencia.ColunaOrigem, ocorrencia.LinhaOrigem);

                        if (!duplicidadeLinhaDestino && !duplicidadeLinhaOrigem && !duplicidadeColunaDestino && !duplicidadeColunaOrigem)
                        {
                            var _TurmaDestino = this.Turmas[ocorrencia.ColunaOrigem];
                            novasPossibilidades.Add(new ProfessorInsercaoPossibilidade
                            {
                                MatrizOrigem = diaSemana,
                                LinhaOrigem = linhaDaTroca,
                                ColunaOrigem = ocorrencia.ColunaOrigem,
                                ProfessorOrigem = professorDestino,
                                TurmaOrigem = ocorrencia.TurmaOrigem,
                                MatrizDestino = ocorrencia.MatrizOrigem,
                                LinhaDestino = ocorrencia.LinhaOrigem,
                                ColunaDestino = ocorrencia.ColunaOrigem,
                                ProfessorDestino = ocorrencia.ProfessorOrigem,
                                TurmaDestino = _TurmaDestino,
                                TrocaOrigemParaDestinoValida = true,
                                TrocaDestinoParaOrigemValida = true,
                                RealocacaoValida = true,
                                FormaDobradinha = false
                            });
                        }
                    }
                }
            }

            listaFiltrada.AddRange(novasPossibilidades);

            return listaFiltrada;
        }



        // Novo método específico para validação em proximidade
        private bool ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(string professor, string[,] matriz, int colunaDestino)
        {

            if(colunaDestino == 7)
            {
                if (professor ==  "SAMIR")
                {
                    string e_SAMIR = "XXXX";
                    
                }
            }


            // Garantir que o índice da coluna está dentro dos limites da matriz
            if (colunaDestino < 0 || colunaDestino >= matriz.GetLength(1))
            {
                Console.WriteLine($"Erro: colunaDestino ({colunaDestino}) está fora dos limites da matriz.");
                return false; // Retorna false se a coluna está fora do intervalo
            }

            // Processar todas as linhas na coluna fornecida
            int totalLinhas = matriz.GetLength(0); // Descobre dinamicamente o total de linhas
            int ocorrencias = 0; // Contador para ocorrências do professor na coluna

            for (int linha = 0; linha < totalLinhas; linha++) // Itera por todas as linhas da coluna especificada
            {
                if (matriz[linha, colunaDestino] == professor) // Verifica duplicidade
                {
                    ocorrencias++; // Incrementa o contador de ocorrências
                    Console.WriteLine($"Conflito parcial: {professor} encontrado na coluna {colunaDestino}, linha {linha}.");
                }

                //////string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linha, colunaDestino]);
                //////string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                //////if (nomeExtraidoMatriz == nomeExtraidoProfessor) // Verifica duplicidade
                //////{
                //////    ocorrencias++; // Incrementa o contador de ocorrências
                //////    Console.WriteLine($"Conflito parcial: {nomeExtraidoProfessor} encontrado na coluna {colunaDestino}, linha {linha}.");
                //////}


                // Até uma ocorrência é aceitável, pois será somada com a inclusão da nova coluna
                // Totalizando no máximo 2
                if (ocorrencias > 1)
                {
                    if (professor == "SAMIR")
                    {
                        string e_SAMIR = "XXXX";

                    }
                    Console.WriteLine($"Conflito: Mais de uma ocorrência de {professor} na coluna {colunaDestino}.");
                    return true; // Indica que há duplicidade
                }
            }

            // Retorna false se as duplicidades forem aceitáveis (0 ou 1 ocorrência)
            Console.WriteLine($"Nenhum conflito crítico encontrado para {professor} na coluna {colunaDestino}.");
            if ( ocorrencias == 2)
            {
                string dois = "é dois";
            }
            return false;
        }



        private bool ResolverSairVoltaAula_ValidarDuplicidadeNaLinhaEmTodasColunas(string professor, string[,] matriz, int colunaAtual, int linhaDestino)
        {
            // Verificar se o índice da linha está dentro dos limites da matriz
            if (linhaDestino < 0 || linhaDestino >= matriz.GetLength(0))
            {
                Console.WriteLine($"Erro: linhaDestino ({linhaDestino}) está fora dos limites da matriz.");
                return false; // Fora dos limites, não é uma duplicidade válida
            }

            // Iterar por todas as colunas da linha fornecida
            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
            {
                // Ignorar a coluna atual porque estou processando ela
                if (coluna == colunaAtual)
                {
                    Console.WriteLine($"Ignorando a coluna {colunaAtual} na verificação de duplicidade.");
                    continue;
                }

                ////////// Verificar se o professor está presente em outra coluna na mesma linha
                ////////if (matriz[linhaDestino, coluna] == professor)
                ////////{
                ////////    Console.WriteLine($"Conflito encontrado: {professor} já está na linha {linhaDestino}, coluna {coluna}.");
                ////////    return true; // Duplicidade encontrada
                ////////}
                ///
                string nomeExtraidoMatriz = ProfessorHelper.ExtrairNomeProfessor(matriz[linhaDestino, coluna]);
                string nomeExtraidoProfessor = ProfessorHelper.ExtrairNomeProfessor(professor);

                if (nomeExtraidoMatriz == nomeExtraidoProfessor)
                {
                    Console.WriteLine($"Conflito encontrado: {nomeExtraidoProfessor} já está na linha {linhaDestino}, coluna {coluna}.");
                    return true; // Duplicidade encontrada
                }

            }

            // Retornar false se nenhuma duplicidade for encontrada
            Console.WriteLine($"Nenhum conflito encontrado para {professor} na linha {linhaDestino}, excluindo a coluna {colunaAtual}.");
            return false; // Nenhuma duplicidade
        }



        private void ResolverSairVoltaAula_ProcessarTrocas(List<ProcessamentoInsercao> listaProcessamento, Dictionary<string, int> progressoProfessores)
        {
            MovimentacaoProfessorFechamento movimentacaoprof = new MovimentacaoProfessorFechamento();
            var necessidade = necessidadesProfessores;
            bool processado = false;

            foreach (var processamento in listaProcessamento)
            {

                var professor = processamento.PossibilidadeVertical.ProfessorOrigem;

                if (!progressoProfessores.ContainsKey(professor))
                {
                    progressoProfessores[professor] = 0;
                }

                int indiceAtual = progressoProfessores[professor];

                if (indiceAtual >= processamento.PossibilidadesHorizontal.Count)
                {
                    Console.WriteLine($"Todas as possibilidades já foram processadas para o professor {professor}.");
                    continue;
                }

                if (processado)
                    break;

                var obterProcessamentoVertical_ATUAL = processamento.PossibilidadeVertical;
                var obterpossibilidadeHorizontal_ATUAL = processamento.PossibilidadesHorizontal[indiceAtual];
                var ListaProcessamento_Atual = new List<ProcessamentoInsercao>
                {
                    new ProcessamentoInsercao
                    {
                        PossibilidadeVertical = obterProcessamentoVertical_ATUAL,
                        PossibilidadesHorizontal = new List<ProfessorInsercaoPossibilidade> { obterpossibilidadeHorizontal_ATUAL }
                    }
                };

                var RetornoSucesso = ResolverSairVoltaAula_SimularTroca_MatrizesCopia(ListaProcessamento_Atual);

                if (RetornoSucesso.Count == 0)
                {
                    continue;
                }

                var possibilidadeVertical = processamento.PossibilidadeVertical;

                Console.WriteLine($"Processando troca Vertical: Professor {possibilidadeVertical.ProfessorOrigem} sendo ORIGEM.");






                var matrizOrigemVertical =  ResolverSairVoltaAula_ObterMatrizPorDia(possibilidadeVertical.MatrizOrigem);
                var matrizDestinoVertical = ResolverSairVoltaAula_ObterMatrizPorDia(possibilidadeVertical.MatrizDestino);

                var possibilidadeHorizontal = processamento.PossibilidadesHorizontal[indiceAtual];
                var matrizOrigemHorizontal = ResolverSairVoltaAula_ObterMatrizPorDia(possibilidadeHorizontal.MatrizOrigem);
                var matrizDestinoHorizontal = ResolverSairVoltaAula_ObterMatrizPorDia(possibilidadeHorizontal.MatrizDestino);

                //Boolean verif-se-pode-gravar = true;
                //inicio da verificacao
               
                 bool duplicidadeVerticalDestino = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                     possibilidadeVertical.ProfessorOrigem, matrizDestinoVertical, possibilidadeVertical.ColunaDestino);

                bool duplicidadeVerticalOrigem = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                     possibilidadeVertical.ProfessorDestino, matrizOrigemVertical, possibilidadeVertical.ColunaOrigem);


                bool duplicidadeHorizontalOrigem = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                    possibilidadeHorizontal.ProfessorDestino, matrizOrigemHorizontal, possibilidadeHorizontal.ColunaOrigem);

                bool duplicidadeHorizontalDestino = ResolverSairVoltaAula_ValidarDuplicidadeNaColunaAcimaDe2Professores(
                    possibilidadeHorizontal.ProfessorOrigem, matrizDestinoHorizontal, possibilidadeHorizontal.ColunaDestino);



                if (possibilidadeVertical.ProfessorOrigem == "SAMIR")
                {
                    string samir = "samir";

                    string samir2 = "samir";
                    if (possibilidadeVertical.ColunaDestino == 7)
                    {
                        string SIM = "";
                    }

                    if (possibilidadeVertical.ColunaOrigem == 7)
                    {
                        string SIM = "";
                    }
                };

                if (possibilidadeVertical.ProfessorDestino == "SAMIR")
                {
                    string samir2 = "samir";
                    if (possibilidadeHorizontal.ColunaDestino == 7)
                    {
                        string SIM = "";
                    }

                    if (possibilidadeHorizontal.ColunaOrigem == 7)
                    {
                        string SIM = "";
                    }
                };



                //fim da verificacao 
                // significa que passou pelas validacoes das
                // colunas entao pode fazer a gravacao com sucesso
                if (!duplicidadeVerticalDestino && !duplicidadeVerticalOrigem && !duplicidadeHorizontalOrigem && !duplicidadeHorizontalDestino)
                {
                   


                    matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = string.Empty;
                    matrizDestinoVertical[possibilidadeVertical.LinhaDestino, possibilidadeVertical.ColunaDestino] = possibilidadeVertical.ProfessorOrigem;

                    movimentacaoprof.ProfessorTrocaTurma = possibilidadeVertical.TurmaOrigem;
                    movimentacaoprof.ProfessorTroca = possibilidadeVertical.ProfessorDestino;
                    movimentacaoprof.MatrizProfessorTroca = possibilidadeVertical.MatrizDestino;
                    movimentacaoprof.MatrizProfessorTrocaLinha = possibilidadeVertical.LinhaDestino;

                    matrizOrigemVertical[possibilidadeVertical.LinhaOrigem, possibilidadeVertical.ColunaOrigem] = possibilidadeVertical.ProfessorDestino;

                    movimentacaoprof.ProfessorOrigemTurma = possibilidadeVertical.TurmaOrigem;
                    movimentacaoprof.ProfessorOrigemTurma = possibilidadeVertical.ProfessorOrigem;
                    movimentacaoprof.MatrizProfessorOrigem = possibilidadeVertical.MatrizOrigem;
                    movimentacaoprof.MatrizProfessorInseridoLinha = possibilidadeVertical.LinhaOrigem;

                    Console.WriteLine($"Troca Vertical realizada: {possibilidadeVertical.ProfessorOrigem} -> {possibilidadeVertical.ProfessorOrigem}");

                    //var possibilidadeHorizontal = processamento.PossibilidadesHorizontal[indiceAtual];

                    //if (possibilidadeHorizontal != null)
                    //{
                    Console.WriteLine($"Processando troca Horizontal: Professor {possibilidadeHorizontal.ProfessorOrigem} sendo inserido.");

                    //var matrizOrigemHorizontal = ResolverSairVoltaAula_ObterMatrizPorDia(possibilidadeHorizontal.MatrizOrigem);
                    //var matrizDestinoHorizontal = ResolverSairVoltaAula_ObterMatrizPorDia(possibilidadeHorizontal.MatrizDestino);

                    matrizOrigemHorizontal[possibilidadeHorizontal.LinhaOrigem, possibilidadeHorizontal.ColunaOrigem] = string.Empty;
                    matrizOrigemHorizontal[possibilidadeHorizontal.LinhaOrigem, possibilidadeHorizontal.ColunaOrigem] = possibilidadeHorizontal.ProfessorDestino;

                    movimentacaoprof.ProfessorDestinoTurma = possibilidadeHorizontal.TurmaDestino;
                    movimentacaoprof.ProfessorDestino = possibilidadeHorizontal.ProfessorDestino;
                    movimentacaoprof.MatrizProfessorDestino = possibilidadeHorizontal.MatrizOrigem;
                    movimentacaoprof.MatrizProfessorDestinoLinha = possibilidadeHorizontal.LinhaOrigem;

                    matrizDestinoHorizontal[possibilidadeHorizontal.LinhaDestino, possibilidadeHorizontal.ColunaDestino] = possibilidadeHorizontal.ProfessorOrigem;

                    movimentacaoprof.ProfessorOrigemTurma = possibilidadeHorizontal.TurmaOrigem;
                    movimentacaoprof.ProfessorOrigem = possibilidadeHorizontal.ProfessorOrigem;
                    movimentacaoprof.MatrizProfessorOrigem = possibilidadeHorizontal.MatrizDestino;
                    movimentacaoprof.MatrizProfessorOrigemLinha = possibilidadeHorizontal.LinhaDestino;

                    this.ListaMovimentacaoProfessorFechamento.Add(movimentacaoprof);

                    // aqui nao cabe alterar a necessidade d professor 
                    // ResolverSairVoltaAula_AtualizarNecessidade(necessidade, possibilidadeVertical.ProfessorInserido);

                    Console.WriteLine($"Processamento de trocas concluído para o professor {possibilidadeVertical.ProfessorInserido}.");

                    processado = true;
                }
                else
                {
                    processado = false;
                }
                //}
                //else
                //{
                //    Console.WriteLine("Nenhuma possibilidade Horizontal disponível para esta troca horizontal.");
                //}

               
            }
        }



        /// <summary>
        /// Atualiza a necessidade de um professor no dicionário de necessidades.
        /// </summary>
        /// <param name="necessidade">Dicionário de necessidades.</param>
        /// <param name="professorInserido">Nome do professor inserido.</param>
        private void ResolverSairVoltaAula_AtualizarNecessidade(Dictionary<string, NecessidadeProfessor> necessidade, string professorInserido)
        {
            // Verificar se o nome do professor é válido
            if (string.IsNullOrEmpty(professorInserido))
            {
                Console.WriteLine("Erro: professor inserido está vazio.");
                return; // Evitar processar um professor com nome inválido
            }

            // Localizar a chave correspondente no dicionário
            ////var chaveEncontrada = necessidade.Keys.FirstOrDefault(chave => chave.StartsWith(professorInserido + "_"));
             var chaveEncontrada = necessidade.Keys.FirstOrDefault(chave => chave.StartsWith(professorInserido + "_") || chave.StartsWith(professorInserido + "-"));

            if (!string.IsNullOrEmpty(chaveEncontrada))
            {
                var necessidadeAtual = necessidade[chaveEncontrada];

                if (necessidadeAtual.QtdAula == 1)
                {
                    // Remover a entrada do dicionário se a quantidade de aulas for 1
                    Console.WriteLine($"Removendo a necessidade de {chaveEncontrada}, pois QtdAula chegou a zero.");
                    necessidade.Remove(chaveEncontrada);
                }
                else
                {
                    // Reduzir a quantidade de aulas se for maior que 1
                    necessidadeAtual.QtdAula -= 1;
                    Console.WriteLine($"Atualizando a necessidade de {chaveEncontrada}. Nova QtdAula: {necessidadeAtual.QtdAula}");
                }
            }
            else
            {
                Console.WriteLine($"Erro: chave para o professor {professorInserido} não encontrada em necessidades.");
            }
        }




        /// <summary>
        /// Identifica as necessidades de inserção de professores considerando as configurações dinâmicas.
        /// </summary>
        /// <returns>Lista de necessidades de inserção para os professores.</returns>
        public List<ProfessorNecessidadeInsercao> ResolverSairVoltaAula_IdentificarNecessidades()
        {

            ResolverSairVoltaAula_ObterProfessorSaiVolta();

            var necessidadesInsercao = new List<ProfessorNecessidadeInsercao>();

            var var_ListaProfessorNecessidadeSaiVolta = ListaProfessorNecessidadeSaiVolta.ToList(); //.Where(x => x.Professor == "ELENICE")
                                                                                                   // .Where(x => x.Coluna == 2)
                                                                                                   // .Where(x => x.DiaSemana == "Segunda-feira").ToList();
            foreach (var necessidade in var_ListaProfessorNecessidadeSaiVolta)
            {
                string professor = necessidade.Professor; //necessidadesInsercao. chaveDividida[0];
                string turma = necessidade.Turma; //chaveDividida[1];

                // Verificar se a turma existe na lista dinâmica de turmas
                int coluna = this.Turmas.IndexOf(turma);
                if (coluna == -1)
                {
                    Console.WriteLine($"Turma {turma} não encontrada na configuração dinâmica de turmas.");
                    continue;
                }

                // Iterar pelas matrizes (um dia da semana para cada matriz)
                foreach (var diaSemana in new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" })
                {
                    var matriz = ResolverSairVoltaAula_ObterMatrizPorDia(diaSemana);
                    var horarios = new List<int>();

                    for (int linha = 0; linha < matriz.GetLength(0); linha++)
                    {
                        if (matriz[linha, coluna] == professor)
                        {
                            horarios.Add(linha);
                        }
                    }

                    if (horarios.Count > 0)
                    {
                        necessidadesInsercao.Add(new ProfessorNecessidadeInsercao
                        {
                            Professor = professor,     // Nome do professor
                            Turma = turma,             // Turma associada ao professor
                            Coluna = coluna,           // Coluna identificada dinamicamente
                            DiaSemana = diaSemana,     // Dia da semana correspondente
                            Horarios = horarios,       // Lista completa de horários do professor
                            QtdAula = 1 // valor fixo 1 sempre 1 porque é movimentacao  //necessidade.Value.QtdAula // Quantidade de aulas necessárias
                        }); 

                        // Interromper o loop para evitar múltiplos conflitos para o mesmo professor
                        break;
                    }
                }
            }

            // Retornar a lista de necessidades de inserção identificadas
            return necessidadesInsercao;
        }


          /// <summary>
        /// Obtém a matriz de horários correspondente ao dia da semana fornecido.
        /// </summary>
        /// <param name="diaSemana">Dia da semana para obter a matriz.</param>
        /// <returns>Matriz de horários correspondente ao dia da semana.</returns>
        private string[,] ResolverSairVoltaAula_ObterMatrizPorDia(string diaSemana)
        {
            // Mapeia os dias da semana para as matrizes correspondentes
            return diaSemana switch
            {
                "Segunda-feira" => matrizHorariosSegunda,
                "Terça-feira" => matrizHorariosTerca,
                "Quarta-feira" => matrizHorariosQuarta,
                "Quinta-feira" => matrizHorariosQuinta,
                "Sexta-feira" => matrizHorariosSexta,
                _ => throw new ArgumentException($"Dia da semana inválido: {diaSemana}")
            };
        }




        /// <summary> ****************************************************
        /// FIM  do bloco 
        //// FINAL DLO BLOCO  SAI E VOLTA DOS PROFESSORES 












        public bool TentarAlocarHorario(string professor, (int hora, int turma) novoHorario, string[,] matriz)
        {
            int hora = novoHorario.hora;
            int turmaIndex = novoHorario.turma;

            // Verifica se o horário está disponível e se o professor não está alocado em outro lugar no mesmo horário
            if (string.IsNullOrEmpty(matriz[hora, turmaIndex]) && !IsProfessorAgendadoNoHorario(matriz, hora, professor))
            {
                matriz[hora, turmaIndex] = professor; // Alocar professor no novo horário
                return true; // Sucesso na alocação
            }
            return false; // Falha ao alocar porque o horário está ocupado ou o professor já está alocado em outro horário
        }




        public void ImprimirRegistrosDeTroca(List<RegistroDeTroca> registrosDeTroca)
        {
            if (registrosDeTroca == null || registrosDeTroca.Count == 0)
            {
                Console.WriteLine("Não há registros de troca para exibir.");
                return;
            }

            Console.WriteLine("Registros de Troca de Horários:");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            Console.WriteLine("| NomeProfessor | Dia        | De (Hora, Turma)   | Para (Hora, Turma)  | Sucesso | Motivo de Falha |");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");

            foreach (var registro in registrosDeTroca)
            {
                string sucesso = registro.Sucesso ? "Sim" : "Não";
                string horarioOriginal = $"({registro.HorarioOriginal.hora}, {registro.HorarioOriginal.turma})";
                string novoHorario = $"({registro.NovoHorario.hora}, {registro.NovoHorario.turma})";
                string motivoFalha = registro.Sucesso ? "-" : registro.MotivoFalha;

                Console.WriteLine($"| {registro.NomeProfessor,-14} | {registro.Dia,-10} | {horarioOriginal,-18} | {novoHorario,-18} | {sucesso,-7} | {motivoFalha,-15} |");
            }

            Console.WriteLine("----------------------------------------------------------------------------------------------------");
        }


        public string GetDisciplinaDeProfessor(string nomeProfessor)
        {
            // Verifica se o professor existe no dicionário
            if (Professores.ContainsKey(nomeProfessor))
            {
                // Acessa o registro do professor e retorna os nomes das disciplinas
                var dadosProfessor = Professores[nomeProfessor];
                // 'dadosProfessor.DistribuicaoTurmas' é um Dictionary<string, List<string>>
                // Aqui estamos pegando todas as chaves deste dicionário que são os nomes das disciplinas
                var disciplinas = dadosProfessor.DistribuicaoTurmas.Keys.ToList();

                // Concatena todos os nomes de disciplinas em uma única string, separados por vírgula
                // Note que aqui apenas listamos ou retornamos a primeira, dependendo do seu requisito
                return string.Join(", ", disciplinas); // Retorna todas as disciplinas como uma string
                                                       // Ou você pode escolher retornar apenas a primeira (ou qualquer específica) assim:
                                                       // return disciplinas.FirstOrDefault();
            }
            else
            {
                return "Professor não encontrado.";
            }
        }


        public int GetQtdAulaAgendada(string nomeProfessor, string turma, string sigla)
        {
            // Extrai o ano escolar da Turma (assumindo que o primeiro caractere é um número)
            int anoEscolar = 0;
            if (char.IsDigit(turma[0])) // Garante que o primeiro caractere é um dígito
            {
                anoEscolar = int.Parse(turma.Substring(0, 1));
            }

            // Busca a quantidade de aulas programadas para a sigla
            if (materiaAulasPorAno.TryGetValue(sigla, out Dictionary<int, int> aulasPorAno))
            {
                int qtdAulas = 0;

                // Verifica se existe uma entrada universal para todas as séries (índice zero)
                if (aulasPorAno.TryGetValue(0, out qtdAulas))
                {
                    // Usar a quantidade definida para todas as séries
                }
                else if (!aulasPorAno.TryGetValue(anoEscolar, out qtdAulas))
                {
                    // Se não há entrada para o ano específico nem universal, retorna 0
                    return 0;
                }

                // Encontra a movimentação correspondente ao professor, turma e sigla
                var movimentacao = movimentacoes.FirstOrDefault(m => m.NomeProfessor == nomeProfessor && m.Turma == turma && m.Sigla == sigla);
                if (movimentacao != null)
                {
                    // Retorna a quantidade de aulas agendadas para essa movimentação
                    return movimentacao.QtdAulaAgendada;
                }
            }

            // Retorna 0 se não encontrar dados correspondentes ou se houver qualquer problema nos dados
            return 0;
        }



        // ---  FIM DA CLASSSE ABAIXO TEMOS OUTRAS CLASSES QUE PODEM SER RETIRADA DAQUI


    }


    // Definição da classe TurmaProf, se não estiver já definida em outro lugar
    public class TurmaProf
    {
        private readonly AppState _appState;
        public int TurmaIndex { get; set; }
        public string Nome { get; set; }
        public string Polivalente { get; set; }

        public TurmaProf(int turmaIndex, string nome, string polivalente, AppState appState) // Construtor da classe
        {
            TurmaIndex = turmaIndex;
            Nome = nome;
            Polivalente = polivalente;
            _appState = appState;
        }
    }

    public class Scheduler
    {

        // Limite máximo de tentativas de realocação para cada professor
        public  int MaxReallocationAttempts = 20;
        public  int sequencial_Professor_desalocado = 0;

       
        // Lista de registros de trocas como pública e estática
        public  List<RegistroDeTroca> RegistrosDeTrocas = new List<RegistroDeTroca>();

        // Método para imprimir os registros de troca
        public  void ImprimirRegistrosDeTroca()
        {
            if (RegistrosDeTrocas.Count == 0)
            {
                Console.WriteLine("Não há registros de troca para exibir.");
                return;
            }

            Console.WriteLine("Registros de Troca de Horários:");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");
            Console.WriteLine("| NomeProfessor | Dia        | De (Hora, Turma)   | Para (Hora, Turma)  | Sucesso | Motivo de Falha |");
            Console.WriteLine("----------------------------------------------------------------------------------------------------");

            foreach (var registro in RegistrosDeTrocas)
            {
                string sucesso = registro.Sucesso ? "Sim" : "Não";
                string horarioOriginal = $"({registro.HorarioOriginal.hora}, {registro.HorarioOriginal.turma})";
                string novoHorario = $"({registro.NovoHorario.hora}, {registro.NovoHorario.turma})";
                string motivoFalha = registro.Sucesso ? "-" : registro.MotivoFalha;

                Console.WriteLine($"| {registro.NomeProfessor,-14} | {registro.Dia,-10} | {horarioOriginal,-18} | {novoHorario,-18} | {sucesso,-7} | {motivoFalha,-15} |");
            }

            Console.WriteLine("----------------------------------------------------------------------------------------------------");
        }



        public class RegistroDeTroca
        {
            public string NomeProfessor { get; set; }
            public string Turma { get; set; }
            public string Dia { get; set; }
            public (int hora, int turma) HorarioOriginal { get; set; }
            public (int hora, int turma) NovoHorario { get; set; }
            public bool Sucesso { get; set; }
            public string MotivoFalha { get; set; }

            public RegistroDeTroca(string nomeProfessor, string turma, string dia, (int hora, int turma) horarioOriginal, (int hora, int turma) novoHorario, bool sucesso, string motivoFalha = "")
            {
                NomeProfessor = nomeProfessor;
                Turma = turma;
                Dia = dia;
                HorarioOriginal = horarioOriginal;
                NovoHorario = novoHorario;
                Sucesso = sucesso;
                MotivoFalha = motivoFalha;
            }
        }



    }


    public class ProfessorNecessidadeInsercao
    {
        public string Professor { get; set; }      // Nome do professor
        public int Coluna { get; set; }            // Coluna onde o professor precisa ser inserido
        public string Turma { get; set; }          // Turma associada ao professor
        public string DiaSemana { get; set; }      // Dia da semana onde a inserção é necessária
        public List<int> Horarios { get; set; }    // Horários onde o professor já está alocado
        public int QtdAula { get; set; }           // Quantidade de aulas que o professor precisa
    }




    // classe utilizada na resolucao de ir a voltar na mesma turma - quandoo professor sai e volta 
    public class ProfessorConflito
    {
        public string Professor { get; set; }
        public int Coluna { get; set; } // Turma (coluna da matriz)
        public string DiaSemana { get; set; } // Dia da semana
        public List<int> Horarios { get; set; } // Lista de horários problemáticos
    }


    public class RegistroDeTroca
    {
     

        public string NomeProfessor { get; set; }
        public string Turma { get; set; }
        public string Dia { get; set; }
        public (int hora, int turma) HorarioOriginal { get; set; }
        public (int hora, int turma) NovoHorario { get; set; }
        public bool Sucesso { get; set; }
        public string MotivoFalha { get; set; }

        public RegistroDeTroca(string nomeProfessor, string turma, string dia, (int hora, int turma) horarioOriginal, (int hora, int turma) novoHorario, bool sucesso, string motivoFalha = "")
        {
            NomeProfessor = nomeProfessor;
            Turma = turma;
            Dia = dia;
            HorarioOriginal = horarioOriginal;
            NovoHorario = novoHorario;
            Sucesso = sucesso;
            MotivoFalha = motivoFalha;
            
        }
    }


    public class NecessidadeProfessor
    {
        private readonly AppState _appState;

        public string NomeProfessor { get; set; }
        public string Turma { get; set; }
        public string Sigla { get; set; }
        public int AnoEscolar { get; set; }
        public int QtdAula { get; set; }
        public int DiaDaSemana { get; set; } // 1 = Segunda-feira, 2 = Terça-feira, etc.
        public int Horario { get; set; } // Horário em que a aula deve ser agendada
        public int TentativasDeRealocacao { get; set; } // Contador de tentativas de realocação

        public List<NecessidadeProfessor> ListGrupoProfessor_2 { get; set; }
        public List<NecessidadeProfessor> ListGrupoProfessor_3 { get; set; }
        public string NomeGrupoProfessor { get; set; }
        public int IdGrupoTroca { get; set; } //1,2,3 é a combinacao entre os professores que vao fazer a troca
        public string Sucesso { get; set; } // funciona combinado com idgrutroca/caso todos as trocas sejam possivel assinar no campo (SIM OU NAO)
        public int DiaDaSemanaTroca { get; set; } // 1 = Segunda-feira, 2 = Terça-feira, etc.
        public int HorarioTroca { get; set; } // Horário em que a aula deve ser agendada
        public int TurmaTroca { get; set; } // turma da troca

        public NecessidadeProfessor(string nome, string turma, string sigla, int anoEscolar, int qtdAula, int diaDaSemana, int horario, int tentativasDeRealocacao, AppState appState)
        {
            NomeProfessor = nome;
            Turma = turma;
            Sigla = sigla;
            AnoEscolar = anoEscolar;
            QtdAula = qtdAula;
            DiaDaSemana = diaDaSemana;
            Horario = horario;
            TentativasDeRealocacao = tentativasDeRealocacao; // Inicializa o contador de tentativas
            _appState = appState;
        }

        public void AtualizarTroca(int grupoTroca, int diaDaSemanaTroca, int horarioTroca, int turmaTroca)
        {
            IdGrupoTroca = grupoTroca;
            DiaDaSemanaTroca = diaDaSemanaTroca;
            HorarioTroca = horarioTroca;
            TurmaTroca = turmaTroca;
        }

        public void AtualizaSucessoTroca(string sucesso)
        {
            Sucesso = sucesso;
        }
    }



    public class MovimentacaoTurma
    {
        private readonly AppState _appState;

        public string NomeProfessor { get; set; }
        public string Turma { get; set; }
        public int QtdAula { get; set; }
        public int QtdAulaAgendada { get; set; }
        public int QtdAulaCalculada { get; set; }
        public int AulasAdicionadas { get; set; } // Nova propriedade
        public string Sigla { get; set; } // materia
        public int AnoEscolar { get; set; } = 0; // zero significa todas as series



        // Construtor público para acesso externo
        public MovimentacaoTurma(string nomeProfessor, string turma, int qtdAula, int aulasAdicionadas, int qtdAulaCalculada, string sigla, int anoEscolar, AppState appState)
        {
            NomeProfessor = nomeProfessor;
            Turma = turma;
            QtdAula = qtdAula;
            AulasAdicionadas = aulasAdicionadas; // Inicialize a nova propriedade
            QtdAulaAgendada = aulasAdicionadas; // Modifique para usar a nova propriedade
            QtdAulaCalculada = qtdAulaCalculada;
            Sigla = sigla;

            AnoEscolar = anoEscolar;
            _appState = appState;
        }
    }


    public class GerenciadorNecessidades
    {
        private readonly AppState _appState;
        public GerenciadorNecessidades(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        //private Dictionary<string, NecessidadeProfessor> necessidadesPendentes = new Dictionary<string, NecessidadeProfessor>();

        public void AdicionarOuAtualizarNecessidade(string nome, string turma, string sigla, int anoEscolar, int qtdAula, int diaDaSemana, int horario, int tentativasDeRealocacao)
        {
            string uniqueKey = $"{nome}_{turma}"; // Utiliza nome e turma como chave única

            if (_appState.localNecessidadeProfessor.TryGetValue(uniqueKey, out NecessidadeProfessor necessidadeExistente))
            {
                // Atualiza os dados existentes e incrementa a quantidade de aulas
                necessidadeExistente.QtdAula += qtdAula; // Incrementa quantidade de aulas
                necessidadeExistente.TentativasDeRealocacao += tentativasDeRealocacao; // Atualiza outras propriedades conforme necessário
            }
            else
            {
                // Insere uma nova necessidade
                var novaNecessidade = new NecessidadeProfessor(nome, turma, sigla, anoEscolar, qtdAula, diaDaSemana, horario, tentativasDeRealocacao, _appState);
                _appState.localNecessidadeProfessor.Add(uniqueKey, novaNecessidade);
            }
        }
    }


    public class ProfessorNecessidadeSaiVolta
    {
        public string Professor { get; set; }      // Nome do professor
        public int Coluna { get; set; }            // Coluna onde o professor precisa ser inserido
        public int Linha { get; set; }            // Coluna onde o professor esta na linha
        public string Turma { get; set; }          // Turma associada ao professor
        public string DiaSemana { get; set; }      // Dia da semana onde a inserção é necessária
    }



}