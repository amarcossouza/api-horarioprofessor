using API_HorarioProfessor.Services.CoreServices;
using API_HorarioProfessor.Services.CoreServices.ClasseDiversas;
using API_HorarioProfessor.Services.CoreServices.Classes;

namespace API_HorarioProfessor.Services.CoreServices.TrocaTresProfessores
{
    public class TrocasEntreTresProfessores
    {
        public List<NecessidadeProfessor> BaseListTrocaProfessor { get; set; }
        public List<NecessidadeProfessor> ListTrocaProfessor_1 { get; set; }
        public List<NecessidadeProfessor> ListTrocaProfessor_2 { get; set; }
        public List<NecessidadeProfessor> ListTrocaProfessor_3 { get; set; }
        public List<NecessidadeProfessor> ListTrocalugarVazio { get; set; }
        public List<NecessidadeProfessor> ListProfessor2Encontrado { get; set; }
        public List<NecessidadeProfessor> ListProfessor3Encontrado { get; set; }

        private readonly AppState _appState;

        public Dictionary<string, NecessidadeProfessor> _ProfessorFaltante = new Dictionary<string, NecessidadeProfessor>();
        public ValidadorDeAgendamento validadorDeAgendamento { get; set; }
        

        public TrocasEntreTresProfessores(AppState appState)
        {
            _appState = appState; // Injetando o AppState
            validadorDeAgendamento = new ValidadorDeAgendamento(_appState);
        }

        public void RealizarTrocasTresProfessoresExecutar(string[][,] matrizes)
        {
            Contabilizar_Aulas contabilAulas = new Contabilizar_Aulas(_appState);

            // VERIFICAR QUANTAS TURMAS AINDA FALTA SER DISTRIBUIDAS 
            // ######################################################################

            List<ContagemAulasProfessor> LerMatrizLocalizarAgendamentoFaltantes = contabilAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> GerarListaTodasAulasProfessor = contabilAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            List<ContagemAulasProfessor> agendamentosFaltantes = contabilAulas.ProcessarMatrizesEContabilizarAulas(matrizes);



            //ImprimirRelatorioDeContagemDeAulas(resultados);
         // LerMatrizLocalizarAgendamentoFaltantes = contabilAulas.ProcessarMatrizesEIdentificarTurmasFaltantes(matrizes).OrderBy(x => x.NomeProfessor).ToList();
            LerMatrizLocalizarAgendamentoFaltantes = contabilAulas.ProcessarMatrizesEIdentificarTurmasFaltantes(matrizes).ToList();
            //ImprimirRelatorioDeContagemDeAulas(resultados);

            GerarListaTodasAulasProfessor = contabilAulas.PreencherTabelaContagemAulasProfessor().OrderBy(x => x.NomeProfessor).ToList();
            // ImprimirRelatorioDeContagemDeAulas(resultados);

            agendamentosFaltantes = contabilAulas.IdentificarAgendamentosFaltantes(GerarListaTodasAulasProfessor, LerMatrizLocalizarAgendamentoFaltantes);


            var GerandoNecessidadeProfessor = contabilAulas.PreencherNecessidadesProfessores(agendamentosFaltantes);



            //// Removendo o item com chave "ProfessorA"
            //if (GerandoNecessidadeProfessor.ContainsKey("IDELSON-7X"))
            //{
            //    GerandoNecessidadeProfessor.Remove("IDELSON-7X");
            //}

            // Iterando sobre o dicionário e passando cada item individualmente
            foreach (var item in _appState.necessidadesProfessores)
            {
                var singleNecessidade = new Dictionary<string, NecessidadeProfessor>
            {
                { item.Key, item.Value }
            };
                //passando um por fez
                RealizarTrocasTresProfessores(matrizes, singleNecessidade);
            }

        }




        public void ContaProfessoresDistribuidoNaGrade(string[][,] matrizes, out List<ContagemAulasProfessor> TodasAulasProfessor, out List<ContagemAulasProfessor> AgendamentoFaltantes)
        {
            Contabilizar_Aulas contabilAulas = new Contabilizar_Aulas(_appState);

            // Inicializando as listas de saída com listas vazias para garantir que sejam atribuídas
            TodasAulasProfessor = new List<ContagemAulasProfessor>();
            AgendamentoFaltantes = new List<ContagemAulasProfessor>();

            // Processar as matrizes para obter os agendamentos
            var LerMatrizLocalizarAgendamentoFaltantes = contabilAulas.ProcessarMatrizesEContabilizarAulas(matrizes);
            //pode estar errado *** contabilAulas.ProcessarMatrizesEContabilizarAulas(matrizes).OrderBy(x => x.NomeProfessor).ToList();
            var GerarListaTodasAulasProfessor = contabilAulas.PreencherTabelaContagemAulasProfessor().OrderBy(x => x.NomeProfessor).ToList(); 
            

            // Identificar turmas faltantes
            LerMatrizLocalizarAgendamentoFaltantes = contabilAulas.ProcessarMatrizesEIdentificarTurmasFaltantes(matrizes).OrderBy(x => x.NomeProfessor).ToList();

            // Atribuir as listas de saída
            TodasAulasProfessor = GerarListaTodasAulasProfessor;
            AgendamentoFaltantes = contabilAulas.IdentificarAgendamentosFaltantes(TodasAulasProfessor, LerMatrizLocalizarAgendamentoFaltantes);
            // ESTA FUNCAO AGENDAMENTO FALTANTES ESTA 




        }





        public void RealizarTrocasTresProfessores(string[][,] matrizes, Dictionary<string, NecessidadeProfessor> NecessidadesProfessores)
        {

            _appState.necessidadesProfessores = NecessidadesProfessores;

 
            //variavel clone para guardar a lista de professor da coluna 
            BaseListTrocaProfessor = new List<NecessidadeProfessor>();
            ListTrocaProfessor_1 = new List<NecessidadeProfessor>();
            ListTrocaProfessor_2 = new List<NecessidadeProfessor>();
            ListTrocaProfessor_3 = new List<NecessidadeProfessor>();
            ListTrocalugarVazio = new List<NecessidadeProfessor>();
            ListProfessor2Encontrado = new List<NecessidadeProfessor>();

            // 1 ##### +++ obter a lista dos professores da coluna a ser processada

            foreach (var professorFaltante in _appState.necessidadesProfessores.Values)
            {
                string turma = professorFaltante.Turma;

                BaseListTrocaProfessor = new List<NecessidadeProfessor>();
                ListTrocaProfessor_1 = new List<NecessidadeProfessor>();

                int idxColunaTurmaProfessorFaltante = _appState.ListTurmasProf.FindIndex(x => x.Nome == turma);
                if (idxColunaTurmaProfessorFaltante == -1)
                {
                    Console.WriteLine($"Turma {turma} não encontrada.");
                    continue;
                }

                int idxMatrizSemana = 0;
                foreach (var matriz in matrizes)
                {
                    for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                    {
                        string nomeTurma = _appState.ListTurmasProf[coluna].Nome;

                        // 2 #### ++++ Processar todas as linhas da matriz 
                        // preenchendo a lista com os professores adquiridos
                        // **************************************************************
                        for (int linha = 0; linha < matriz.GetLength(0); linha++)
                        {
                            // AQUI ESTOU PROCESSANDO A MESMA COLUNA
                            if (coluna == idxColunaTurmaProfessorFaltante)
                            {
                                string nomeProfessor = matriz[linha, coluna].Trim();
                                string sigla = "";
                                int anoEscolar = 0;
                                int diaDaSemana = idxMatrizSemana;
                                int qtdAula = 0;
                                int horario = linha;
                                int tentativasDeRealocacao = 0;

                                var prof1 = new NecessidadeProfessor(
                                    nomeProfessor,
                                    turma,
                                    sigla,
                                    anoEscolar,
                                    qtdAula,
                                    diaDaSemana,
                                    horario,
                                    tentativasDeRealocacao,
                                    _appState
                                );

                                BaseListTrocaProfessor.Add(prof1);
                            }
                        }
                    }
                    idxMatrizSemana++;  // saber qual matriz estou processando
                }

                ListTrocalugarVazio = BaseListTrocaProfessor.Where(x => x.NomeProfessor == "").ToList();


                BaseListTrocaProfessor = BaseListTrocaProfessor.Where(x => x.NomeProfessor != "").ToList();

                // processar toda a lista pegando o primeiro item vazio e comparar se da para fazer a troca
                // *** criar uma variavel que vai alimentar o campo 
                // IdGrupoTroca de todos os objetos
                // ele fara referencia entre os três  objetos para troca
                int IDGrupoTroca = 0;


                // ESTOU TRABALHNDO A PARTIR DAQUI
                foreach (var prof1 in BaseListTrocaProfessor)
                {

                    string _nomeProf1 = prof1.NomeProfessor;
                    int _DiaDaSemana = prof1.DiaDaSemana;
                    int _horario = prof1.Horario;
                    string _turmaProf1 = prof1.Turma;

                    foreach (var profvazio in ListTrocalugarVazio)
                    {
                        // aqui eu sei qual é a matriz que contem a linha vazia
                        int idxDiaSemanaMatrizVazio = profvazio.DiaDaSemana;
                        int _HorarioVazio = profvazio.Horario;


                        string Sigla;
                        int AnoEscolar;
                        string[,] matriz = matrizes[idxDiaSemanaMatrizVazio];
                        int _turmaIndexVazio = _appState.Turmas.IndexOf(turma);

                        if (turma == "7X")
                        {
                            string estouNaturma = "7Y";
                        }

                        if (_appState.ValidarAlocacaoProfessorParaReaReagendar(matriz, idxDiaSemanaMatrizVazio, _HorarioVazio, _turmaIndexVazio, _nomeProf1, out Sigla, out AnoEscolar, _turmaProf1))
                        {
                             // VEJA AQUI ESTOU PESQUISANDO NA MATRIZ COM ESPACO VAZIO A POSSIBILIDADE DE TROCA DO PROFESSOR
                            // A PESQUISA ACONTECE NA MATRIZ COM HORARIO VAGO VAZIO
                            var resultados = validadorDeAgendamento.ValidarAgendamento(matriz, idxDiaSemanaMatrizVazio, _nomeProf1, _HorarioVazio, _turmaIndexVazio);

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
                                string PODEALOCAR = "";




                                // ** criar a chave de relacionamento
                                // *********************
                                IDGrupoTroca++;



                                // vou inserir o registro no primeiro professor
                                /// logo abaixo eu atualizao com outro metodo
                                /// //ATENCAO SAO AS INFORMACOES DA VARIVEL ANTERIOR VEJA O CODIGO.
                                string nomeProfessor = _nomeProf1;
                                string sigla = "";
                                int anoEscolar = 0;
                                int diaDaSemana = _DiaDaSemana;
                                int qtdAula = 0;
                                int horario = _horario;
                                int tentativasDeRealocacao = 0;


                                //vamos gravar os registros e realizar o relacionamento
                                //** PROF1

                                NecessidadeProfessor objprof = new NecessidadeProfessor(
                                    nomeProfessor,
                                    turma,
                                    sigla,
                                    anoEscolar,
                                    qtdAula,
                                    diaDaSemana,
                                    horario,
                                    tentativasDeRealocacao,
                                    _appState
                                );

                                List<NecessidadeProfessor> listObjProf = new List<NecessidadeProfessor>();
                                listObjProf.Add(objprof);


                                //** vou fazer registro no professor 1 informando que ele pode ir para a celula desejada
                                AtualizarProfessorGrupo(listObjProf,
                                                    _nomeProf1,
                                                    IDGrupoTroca,
                                                    idxDiaSemanaMatrizVazio,
                                                    _HorarioVazio,
                                                    _turmaIndexVazio
                                                    );

                                // PROCESSAR A SEGUNDA LISTA DO PROFESSOR COM BASE NA PRIMARA LISTA PROFESSOR
                                // 2 ##### +++ obter a lista dos SEGUNDOS professores da coluna a ser processada
                                List<NecessidadeProfessor> ListProfessor2Encontrado = new List<NecessidadeProfessor>();
                                foreach (var prof2 in listObjProf)
                                {
                                    var ObterListTrocaProfessor_1 = listObjProf.FindAll(p => p.NomeProfessor == nomeProfessor && p.Horario == horario && p.DiaDaSemana == diaDaSemana).ToList();
                                    bool sucesso_prof2 = Processar_professor2(matrizes,
                                                                               BaseListTrocaProfessor,
                                                                               ObterListTrocaProfessor_1,
                                                                               ListProfessor2Encontrado
                                                                              );


                                    foreach (var prof3 in ListProfessor2Encontrado)
                                    {
                                        List<NecessidadeProfessor> ListProfessor3Encontrado = new List<NecessidadeProfessor>();
                                        // 3 ##### +++ obter a lista dos TERCEIRO professores da coluna a ser processada
                                        var ObterListTrocaProfessor_3 = ListProfessor2Encontrado.FindAll(p => p.NomeProfessor == prof3.NomeProfessor && p.Horario == prof3.Horario && p.DiaDaSemana == prof3.DiaDaSemana).ToList();
                                        bool sucesso_prof3 = Processar_professor3(matrizes,
                                                                                   BaseListTrocaProfessor,
                                                                                   ObterListTrocaProfessor_3,
                                                                                   ListProfessor3Encontrado,
                                                                                   professorFaltante.NomeProfessor
                                                                                 );

                                        //---------------------------------------------------------------------------------------------------------------------

                                        // Montar o relacionamento entre as tres TROCAS DOS PROFESSORES
                                        // ************************************************************
                                        // nao estou testao o prof1 porque foi dele que parti para obter estes dois profs
                                        if (sucesso_prof2 == true && sucesso_prof3 == true) // deu certo para os dois processos 
                                        {

                                            //**PROF2
                                            foreach (var p2 in ListProfessor2Encontrado)
                                            {
                                                p2.IdGrupoTroca = IDGrupoTroca;
                                                p2.NomeGrupoProfessor = nomeProfessor;
                                            }
                                            objprof.ListGrupoProfessor_2 = new List<NecessidadeProfessor>();
                                            objprof.ListGrupoProfessor_2.AddRange(ListProfessor2Encontrado);


                                            //---------------------------------------------------------------------------------------------------------------------



                                            //**PROF3
                                            foreach (var p3 in ListProfessor3Encontrado)
                                            {
                                                p3.IdGrupoTroca = IDGrupoTroca;
                                                p3.NomeGrupoProfessor = nomeProfessor;
                                            }
                                            objprof.ListGrupoProfessor_3 = new List<NecessidadeProfessor>();
                                            objprof.ListGrupoProfessor_3.AddRange(ListProfessor3Encontrado);


                                            //---------------------------------------------------------------------------------------------------------------------
                                            //vamos gravar os registros e realizar o relacionamento
                                            //** PROF1


                                            ListTrocaProfessor_1.Add(objprof);


                                            //---------------------------------------------------------------------------------------------------------------------



                                        }


                                    }

                                    string verifica_houve_sucesso = "";
                                }
                            }
                        }
                    }
                }

            }


            //********** ###############  fazer as trocas dos professores

            if (ListTrocaProfessor_1.Count != 0)
            {
                //troca 1
                string NomeProfessor_1 = ListTrocaProfessor_1[0].NomeProfessor;
                int horario_1 = ListTrocaProfessor_1[0].HorarioTroca;
                int DiaDaSemana_1 = ListTrocaProfessor_1[0].DiaDaSemanaTroca;
                string turma = ListTrocaProfessor_1[0].Turma;

                int linha = horario_1;
                //int turmaIndex = _globalSettings.turmas.IndexOf(turma);
                int coluna = _appState.ListTurmasProf.First(t => t.Nome == turma).TurmaIndex;
                string[,] matriz = matrizes[DiaDaSemana_1];
                matriz[linha, coluna] = NomeProfessor_1;

                //troca 2
                string NomeProfessor_2 = ListTrocaProfessor_1[0].ListGrupoProfessor_2[0].NomeProfessor;
                int horario_2 = ListTrocaProfessor_1[0].ListGrupoProfessor_2[0].HorarioTroca;
                int DiaDaSemana_2 = ListTrocaProfessor_1[0].ListGrupoProfessor_2[0].DiaDaSemanaTroca;
                int linha2 = horario_2;
                int coluna2 = _appState.ListTurmasProf.First(t => t.Nome == turma).TurmaIndex;
                matriz = matrizes[DiaDaSemana_2];
                matriz[linha2, coluna2] = NomeProfessor_2;

                //troca 2
                string NomeProfessor_3 = ListTrocaProfessor_1[0].ListGrupoProfessor_3[0].NomeProfessor;
                int horario_3 = ListTrocaProfessor_1[0].ListGrupoProfessor_3[0].HorarioTroca;
                int DiaDaSemana_3 = ListTrocaProfessor_1[0].ListGrupoProfessor_3[0].DiaDaSemanaTroca;
                int linha3 = horario_3;
                int coluna3 = _appState.ListTurmasProf.First(t => t.Nome == turma).TurmaIndex;
                matriz = matrizes[DiaDaSemana_3];
                matriz[linha3, coluna3] = NomeProfessor_3;

            }

        }


        // Método para percorrer as matrizes e preencher as instâncias de NecessidadeProfessor
        public List<NecessidadeProfessor> ObterNecessidadesProfessores(string[,,] matrizes)
        {
            var necessidades = new List<NecessidadeProfessor>();

            for (int dia = 0; dia < matrizes.GetLength(0); dia++) // Percorre os dias da semana
            {
                for (int i = 0; i < matrizes.GetLength(1); i++) // Percorre as linhas
                {
                    for (int j = 0; j < matrizes.GetLength(2); j++) // Percorre as colunas
                    {
                        var professor = matrizes[dia, i, j];
                        if (!string.IsNullOrEmpty(professor) && !necessidades.Any(n => n.NomeProfessor == professor))
                        {
                            var necessidade = new NecessidadeProfessor(professor, "Turma", "Sigla", 0, 0, dia + 1, i, 0,_appState);
                            necessidades.Add(necessidade);
                        }
                    }
                }
            }

            return necessidades;
        }



        public bool Processar_professor2(string[][,] matrizes,
            List<NecessidadeProfessor> BaseListTrocaProfessor,
            List<NecessidadeProfessor> ListTrocaProfessor_1,
            List<NecessidadeProfessor> ListProfessor2Encontrado)
        {

            bool _retorno_sucesso = false;

            // 2 ##### +++ obter a lista dos SEGUNDOS professores da coluna a ser processada

            //ListTrocaProfessor_2 = new List<NecessidadeProfessor>();
            foreach (var professorTroca in BaseListTrocaProfessor)
            {

                string NomeProfessorTroca = professorTroca.NomeProfessor;
                string turma = professorTroca.Turma;


                int idxColunaTurmaProfessorFaltante = _appState.ListTurmasProf.FindIndex(x => x.Nome == turma);
                int idxMatrizSemana = 0;
                var obterOProfessor1Troca = ListTrocaProfessor_1;
                foreach (var profvazio in obterOProfessor1Troca)
                {

                    // aqui eu sei qual é a matriz que contem a linha vazia
                    int idxDiaSemanaMatrizVazio = profvazio.DiaDaSemana;
                    int _HorarioVazio = profvazio.Horario;


                    string Sigla;
                    int AnoEscolar;
                    string[,] matriz = matrizes[idxDiaSemanaMatrizVazio];
                    int _turmaIndexVazio = _appState.Turmas.IndexOf(turma);

                    if (turma == "8Y")
                    {
                        string estouNaturma = "8Y";
                    }

                    if (professorTroca.NomeProfessor == "RANIERE")
                    {
                        string RANIERI = "";
                    }

                    if (_appState.ValidarAlocacaoProfessorParaReaReagendarSemValidarCelulaVazia(matriz, idxDiaSemanaMatrizVazio, _HorarioVazio, _turmaIndexVazio, NomeProfessorTroca, out Sigla, out AnoEscolar, turma))
                    {

                        // VEJA AQUI ESTOU PESQUISANDO NA MATRIZ COM ESPACO VAZIO A POSSIBILIDADE DE TROCA DO PROFESSOR
                        // A PESQUISA ACONTECE NA MATRIZ COM HORARIO VAGO VAZIO
                        var resultados = validadorDeAgendamento.ValidarAgendamento(matriz, idxDiaSemanaMatrizVazio, NomeProfessorTroca, _HorarioVazio, _turmaIndexVazio);

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
                            string PODEALOCAR = "";


                            // vou inserir o registro no primeiro professor
                            /// logo abaixo eu atualizao com outro metodo
                            /// //ATENCAO SAO AS INFORMACOES DA VARIVEL ANTERIOR VEJA O CODIGO.
                            string nomeProfessor = NomeProfessorTroca;
                            string sigla = "";
                            int anoEscolar = 0;
                            int diaDaSemana = professorTroca.DiaDaSemana;
                            int qtdAula = professorTroca.QtdAula;
                            int horario = professorTroca.Horario;
                            int tentativasDeRealocacao = professorTroca.TentativasDeRealocacao;
                            NecessidadeProfessor objprof = new NecessidadeProfessor(
                                nomeProfessor,
                                turma,
                                sigla,
                                anoEscolar,
                                qtdAula,
                                diaDaSemana,
                                horario,
                                tentativasDeRealocacao,
                                _appState
                            );
                            ListProfessor2Encontrado.Add(objprof);

                            //** vou fazer registro no professor 1 informando que ele pode ir para a celula desejada
                            AtualizarProfessorGrupo(ListProfessor2Encontrado,
                                                nomeProfessor,
                                                1,// IDGrupoTroca,
                                                idxDiaSemanaMatrizVazio,
                                                _HorarioVazio,
                                                _turmaIndexVazio
                                                );


                            // conseguiu referencia com professor2 retorna sucesso
                            //*****
                            _retorno_sucesso = true;
                        }
                    }
                }
            }

            return _retorno_sucesso;
        }





        public bool Processar_professor3(string[][,] matrizes,
            List<NecessidadeProfessor> BaseListTrocaProfessor,
            List<NecessidadeProfessor> ListTrocaProfessor_1,
            List<NecessidadeProfessor> ListProfessor3Encontrado,
            string NomeProfessorFaltante)

        {
            bool _retorno_sucesso = false;

            // 2 ##### +++ obter a lista dos SEGUNDOS professores da coluna a ser processada
            foreach (var professorTroca in BaseListTrocaProfessor)
            {

                string NomeProfessorTroca = professorTroca.NomeProfessor;
                string turma = professorTroca.Turma;


                int idxColunaTurmaProfessorFaltante = _appState.ListTurmasProf.FindIndex(x => x.Nome == turma);
                int idxMatrizSemana = 0;
                var obterOProfessor1Troca = ListTrocaProfessor_1;
                foreach (var profvazio in obterOProfessor1Troca)
                {

                    // aqui eu sei qual é a matriz que contem a linha vazia
                    int idxDiaSemanaMatrizVazio = profvazio.DiaDaSemana;
                    int _HorarioVazio = profvazio.Horario;


                    string Sigla;
                    int AnoEscolar;
                    string[,] matriz = matrizes[idxDiaSemanaMatrizVazio];
                    int _turmaIndexVazio = _appState.Turmas.IndexOf(turma);

                    if (turma == "8Y")
                    {
                        string estouNaturma = "8Y";
                    }

                    if (professorTroca.NomeProfessor == "RANIERE")
                    {
                        string RANIERI = "";
                    }

                    // tudo certo pegar o professor que ira ser incluido na celula




                    if (_appState.ValidarAlocacaoProfessorParaReaReagendarSemValidarCelulaVazia(matriz, idxDiaSemanaMatrizVazio, _HorarioVazio, _turmaIndexVazio, NomeProfessorFaltante, out Sigla, out AnoEscolar, turma))
                    {
                        // VEJA AQUI ESTOU PESQUISANDO NA MATRIZ COM ESPACO VAZIO A POSSIBILIDADE DE TROCA DO PROFESSOR
                        // A PESQUISA ACONTECE NA MATRIZ COM HORARIO VAGO VAZIO
                        var resultados = validadorDeAgendamento.ValidarAgendamento(matriz, idxDiaSemanaMatrizVazio, NomeProfessorFaltante, _HorarioVazio, _turmaIndexVazio);

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
                            string PODEALOCAR = "";


                            // vou inserir o registro no primeiro professor
                            /// logo abaixo eu atualizao com outro metodo
                            /// //ATENCAO SAO AS INFORMACOES DA VARIVEL ANTERIOR VEJA O CODIGO.
                            string nomeProfessor = NomeProfessorFaltante;
                            string sigla = "";
                            int anoEscolar = 0;
                            int diaDaSemana = professorTroca.DiaDaSemana;
                            int qtdAula = professorTroca.QtdAula;
                            int horario = professorTroca.Horario;
                            int tentativasDeRealocacao = professorTroca.TentativasDeRealocacao;
                            NecessidadeProfessor objprof = new NecessidadeProfessor(
                                nomeProfessor,
                                turma,
                                sigla,
                                anoEscolar,
                                qtdAula,
                                diaDaSemana,
                                horario,
                                tentativasDeRealocacao,
                                _appState
                            );
                            ListProfessor3Encontrado.Add(objprof);

                            //** vou fazer registro no professor 1 informando que ele pode ir para a celula desejada
                            AtualizarProfessorGrupo(ListProfessor3Encontrado,
                                                nomeProfessor,
                                                1,// IDGrupoTroca,
                                                idxDiaSemanaMatrizVazio,
                                                _HorarioVazio,
                                                _turmaIndexVazio
                                                );

                            // conseguiu referencia com professor3 retorna sucesso
                            //*****
                            _retorno_sucesso = true;

                        }
                    }
                }
            }

            return _retorno_sucesso;
        }




        public void AtualizarProfessorGrupo(
                List<NecessidadeProfessor> listaProfessores,
                string nomeProfessor,
                int IdGrupoTroca,
                int diaDaSemanaTroca,
                int horarioTroca,
                int turmaTroca)
        {
            var objprofessor = listaProfessores.FirstOrDefault(p => p.NomeProfessor == nomeProfessor);
            if (objprofessor != null)
            {
                objprofessor.AtualizarTroca(IdGrupoTroca, diaDaSemanaTroca, horarioTroca, turmaTroca);
            }
            else
            {
                Console.WriteLine($"Professor {nomeProfessor} não encontrado.");
            }
        }


        public void AtualizarProfessorGrupoSucesso(
        List<NecessidadeProfessor> listaProfessores,
        string nomeProfessor,
        int IdGrupoTroca,
        string Sucesso)
        {
            var objprofessor = listaProfessores.FirstOrDefault(p => p.NomeProfessor == nomeProfessor && p.IdGrupoTroca == IdGrupoTroca);
            if (objprofessor != null)
            {
                objprofessor.AtualizaSucessoTroca(Sucesso);
            }
            else
            {
                Console.WriteLine($"Professor {nomeProfessor} não encontrado.");
            }
        }



    }
}
