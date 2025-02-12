using API_HorarioProfessor.Services.CoreServices.Classes;

namespace API_HorarioProfessor.Services.CoreServices.ClasseDiversas
{
    public class HorarioRealocacaoPorTurma
    {
        private readonly AppState _appState;
        public HorarioRealocacaoPorTurma(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        public  List<OportunidadeTroca> oportunidadesProfessorNecessidadeOrigem = new List<OportunidadeTroca>();
        public  List<OportunidadeTroca> oportunidadesProfessorNecessidadeDestino = new List<OportunidadeTroca>();

        public  List<OportunidadeTroca> resultadosOrigem = new List<OportunidadeTroca>();
        public  List<OportunidadeTroca> resultadosDestino = new List<OportunidadeTroca>();


        public async Task<Dictionary<string, NecessidadeProfessor>> ProcessarRealoacoesPorColuna(string[][,] matrizes, Dictionary<string, NecessidadeProfessor> necessidades)
        {
            // Iterar sobre o dicionário de necessidades pelo par chave-valor
            int idxDiaSemanaMatriz = 0;
            bool NAOCertoAlocacaoNaoPrecisaContinuar = false;
            foreach (var item in necessidades)
            {


                resultadosOrigem = new List<OportunidadeTroca>();
                resultadosDestino = new List<OportunidadeTroca>();
                string chaveEspecifica = item.Key;  // Aqui você tem a chave
                NecessidadeProfessor necessidade = item.Value;  // Aqui você tem o valor correspondente à chave

                // Cria um novo dicionário contendo apenas o item específico
                Dictionary<string, NecessidadeProfessor> necessidadeEspecifica = new Dictionary<string, NecessidadeProfessor>
                {
                    { chaveEspecifica, necessidade }
                };

                // Chama o método IdentificarOportunidades com apenas essa necessidade específica


                var Identificar_MudarOrdem_por_matriz = IdentificarOportunidadesOrigem(matrizes, necessidadeEspecifica);
                Identificar_MudarOrdem_por_matriz =  Identificar_MudarOrdem_por_matriz.OrderByDescending(x => x.IndexMatriz).ToList();

                oportunidadesProfessorNecessidadeOrigem = Identificar_MudarOrdem_por_matriz; // IdentificarOportunidadesOrigem(matrizes, necessidadeEspecifica);
                oportunidadesProfessorNecessidadeDestino = IdentificarOportunidadesDestino(matrizes, oportunidadesProfessorNecessidadeOrigem);

                // #### ATENCAO vou processar quantas vezes for na quantidade de origem / destino ###########
                for (int vezesOportunidade = 0; vezesOportunidade < oportunidadesProfessorNecessidadeOrigem.Count; vezesOportunidade++)
                {
                    //if (NAOCertoAlocacaoNaoPrecisaContinuar == false)
                    //{

                    //foreach (var oportunidade in oportunidadesProfessorNecessidadeOrigem)
                    //{
                    //if (ValidarRealocacao(necessidade, oportunidade))
                    //{
                    // ##################################################//
                    // pegar aprimeira oportunidadeda lista
                    // ##################################################//

                    EncontrarProfessorComRelacionamentoValido();
                    //var oportunidadeOrigem = resultadosOrigem.FirstOrDefault();
                    //var oportunidadeDestino = resultadosDestino.FirstOrDefault();

                    var oportunidadeOrigem = resultadosOrigem.ElementAtOrDefault(vezesOportunidade); // Para o primeiro item
                    var oportunidadeDestino = resultadosDestino.ElementAtOrDefault(vezesOportunidade); // Para o segundo item, e assim por diante.


                    if ((oportunidadeOrigem != null) && (oportunidadeDestino != null))
                    {
                        bool HouveSucessoNaTroca = RealizarTroca(oportunidadeOrigem, oportunidadeDestino, necessidade, matrizes, vezesOportunidade);
                        if (HouveSucessoNaTroca)
                        {
                            //decrementar a lista que foi analisada 
                            //pois a troca foi realizada com sucesso
                            item.Value.QtdAula = --item.Value.QtdAula;
                            Console.WriteLine($"Troca realizada com sucesso para o professor {necessidade.NomeProfessor}.");

                            /// se deu certo nao deve continuar a troca com o mesmo professo
                            //NAOCertoAlocacaoNaoPrecisaContinuar = true;
                        }
                        //}



                        //else
                        //{
                        //    Console.WriteLine($"Falha ao tentar trocar horário do professor {necessidade.NomeProfessor}.");
                        //    return false;
                        //}
                        //}
                    }
                }
            }

            return necessidades;
        }



        private  bool ValidarRealocacao(NecessidadeProfessor necessidade, OportunidadeTroca oportunidade)
        {
            // Inserir chamadas para métodos de validação existentes aqui
            // Exemplo: return ValidarDisponibilidade(necessidade, oportunidade) && ValidarQualificacoes(necessidade, oportunidade);
            return true; // Suponha que sempre retorne true para simplificar
        }

        private  bool RealizarTroca(OportunidadeTroca oportunidadesOrigem, OportunidadeTroca oportunidadesDestino, NecessidadeProfessor necessidade, string[][,] matrizes, int vezesOportunidade)
        {
            bool validacao = false;

            int idxDiaSemanaMatriz = oportunidadesOrigem.Dia;
            // #############################################################################################
            //** precisa fazer as validacao das oportunidades origem e oportunidade destino ** 
            // **** ambas tem que ser sucesso ##############################################################

            string[,] matrizDia = matrizes[oportunidadesOrigem.Dia];
            string[,] matrizDia2 = matrizes[oportunidadesDestino.Dia];


            if (oportunidadesOrigem.TurmaIndex == 5)
            {
                if (necessidade.NomeProfessor == "LUIS")
                {
                    string professor = "achei o luis";
                }
            }


            if (true) //depois ajustar coloquei aqui porque tirei a validacao de cima 
            {
                int contarVezesProfessorNaColunaORIGEM = _appState.ContarVezesDoProfessorNaColunaNaMatriz(matrizDia, oportunidadesOrigem.TurmaIndex, necessidade.NomeProfessor);
                bool validaLocacaoTodasMatrizesOrigem = _appState.VerificarCapacidadeAulasPorTurma(matrizes, oportunidadesOrigem.TurmaIndex, necessidade.NomeProfessor);
                int contarVezesProfessorNaColunaDESTINO = _appState.ContarVezesDoProfessorNaColunaNaMatriz(matrizDia, oportunidadesDestino.TurmaIndex, oportunidadesDestino.Professor);
                bool validaLocacaoTodasMatrizesDESTINO = _appState.VerificarCapacidadeAulasPorTurma(matrizes, oportunidadesOrigem.TurmaIndex, necessidade.NomeProfessor);
                if (
                    ((contarVezesProfessorNaColunaORIGEM < 2) && (contarVezesProfessorNaColunaDESTINO < 2)) &&
                    ((validaLocacaoTodasMatrizesOrigem == true) && (validaLocacaoTodasMatrizesDESTINO == true))
                   )
                {


                    if (oportunidadesDestino.Professor == "LUIS")
                    {
                        if (oportunidadesOrigem.TurmaIndex == 3)
                        {
                            if (oportunidadesDestino.Horario == 5)
                            {
                                string professor = "achei o luis";
                            }
                        }
                    }

                    if (oportunidadesDestino.Professor == "LUIS")
                    {
                        if (oportunidadesOrigem.TurmaIndex == 5)
                        {
                            string professor = "achei o luis";
                        }
                    }





                    ValidadorDeAgendamento validadorDeAgendamento = new ValidadorDeAgendamento(_appState);
                    // 1 
                    idxDiaSemanaMatriz = oportunidadesOrigem.Dia;
                    // 25052024 var resultadosOrigem = ValidadorDeAgendamento.ValidarAgendamento(matrizDia, idxDiaSemanaMatriz, oportunidadesOrigem.Professor, oportunidadesOrigem.Horario, oportunidadesOrigem.TurmaIndex);
                    var resultadosOrigem = validadorDeAgendamento.ValidarAgendamento(matrizDia, idxDiaSemanaMatriz, necessidade.NomeProfessor, oportunidadesOrigem.Horario, oportunidadesOrigem.TurmaIndex);
                    bool ValidaOportunidadeOrigem = true;
                    foreach (var resultado in resultadosOrigem.Values)
                    {
                        if (!resultado)
                        {
                            ValidaOportunidadeOrigem = false;
                            break;
                        }
                    }


                    //2
                    idxDiaSemanaMatriz = oportunidadesDestino.Dia;
                    // 25052024 var resultadosDestino = ValidadorDeAgendamento.ValidarAgendamento(matrizDia2, idxDiaSemanaMatriz, oportunidadesDestino.Professor, oportunidadesDestino.Horario, oportunidadesDestino.TurmaIndex);
                    var resultadosDestino = validadorDeAgendamento.ValidarAgendamento(matrizDia2, idxDiaSemanaMatriz, oportunidadesDestino.Professor, oportunidadesDestino.Horario, oportunidadesDestino.TurmaIndex);

                    bool ValidaOportunidadeDestino = true;
                    foreach (var resultado in resultadosDestino.Values)
                    {
                        if (!resultado)
                        {
                            ValidaOportunidadeOrigem = false;
                            break;
                        }
                    }


                    if ((ValidaOportunidadeOrigem) && (ValidaOportunidadeDestino))
                    {
                        if (oportunidadesOrigem.Dia == 4)
                        {
                            string naoigualzero = "";
                        }

                        if (oportunidadesOrigem.Dia == 5)
                        {
                            if (necessidade.NomeProfessor == "LUIS")
                            {
                                if ((oportunidadesOrigem.Horario == 5) || (oportunidadesOrigem.Horario == 6))
                                {
                                    {
                                        string E_SEXTA = "AQUI";
                                    }

                                }
                            }
                        }

                        if (necessidade.NomeProfessor == "LUIS")
                        {
                            if (oportunidadesOrigem.Horario == 5)
                            {
                                string PAUSA = "";
                            }
                        }

                        if (oportunidadesDestino.Professor == "LUIS")
                        {
                            if (oportunidadesDestino.Horario == 5)
                            {
                                string PAUSA = "";
                            }
                        }


                        //string[,] matrizDia = matrizes[oportunidadesOrigem.Dia];
                        string quemEstaNaCelula = matrizDia[oportunidadesOrigem.Horario, oportunidadesOrigem.TurmaIndex];
                        // Verifica se o horário está vago antes de realizar a troca
                        matrizDia[oportunidadesOrigem.Horario, oportunidadesOrigem.TurmaIndex] = necessidade.NomeProfessor;

                        //string[,] matrizDia2 = matrizes[oportunidadesDestino.Dia];
                        matrizDia2[oportunidadesDestino.Horario, oportunidadesDestino.TurmaIndex] = oportunidadesDestino.Professor;

                        validacao = true;
                    }
                }
                //{
                //    matrizDia[oportunidadesOrigem.Horario, oportunidadesOrigem.TurmaIndex] = oportunidadesOrigem.Professor;
                //    return true;
                //}
            }
            else
            {
                //

                string teste = "algo deu errado";
                Console.WriteLine($"Professor com restrição de horário - {necessidade.NomeProfessor}.");
            }

            if (validacao == false)
            {
                string NAODEU = "NAO DEU CERTO";
            }
            return validacao;
        }



        //private  List<OportunidadeTroca> IdentificarOportunidadesOrigem(string[][,] matrizes, Dictionary<string, NecessidadeProfessor> necessidades)
        //{
        //    var oportunidades = new List<OportunidadeTroca>();
        //    foreach (var necessidade in necessidades.Values)
        //    {
        //        var turmaProf = _appState.ListTurmasProf.FirstOrDefault(t => t.Nome == necessidade.Turma);
        //        if (turmaProf == null) continue;

        //        int idxDiaSemanaMatriz = 0;
        //        for (int dia = 0; dia < matrizes.Length; dia++)
        //        {
        //            string[,] matrizDia = matrizes[dia];
        //            for (int horario = 0; horario < matrizDia.GetLength(0); horario++)
        //            {
        //                //if (string.IsNullOrEmpty(matrizDia[horario, turmaProf.TurmaIndex]))
        //                //{
        //                bool ValidarOportunidade = ValidarOportunidadeDeTroca(matrizDia, idxDiaSemanaMatriz, necessidade, dia, horario, turmaProf.TurmaIndex);
        //                if (ValidarOportunidade)
        //                {
        //                    string obterNomeProfessorNaCelula = matrizDia[horario, turmaProf.TurmaIndex];
        //                    // *******************************************************************///
        //                    //verificar se o professor da celula nao é o mesmo da celula processado 
        //                    // deve desconsiderar o proprio professor da celula local pesquisado 
        //                    string professorelula = matrizDia[horario, turmaProf.TurmaIndex];
        //                    //if (obterNomeProfessorNaCelula == "")
        //                    //{
        //                    string CampoSeraPreenchidoDepois = "";
        //                    oportunidades.Add(new OportunidadeTroca(obterNomeProfessorNaCelula, dia, horario, turmaProf.TurmaIndex, CampoSeraPreenchidoDepois, idxDiaSemanaMatriz));
        //                    //}
        //                }
        //                //}
        //            }


        //            idxDiaSemanaMatriz++;
        //        }
        //    }
        //    return oportunidades;
        //}

        // VEJA O CODIGO ACIMA FIZ MUDANCA TA **** ***** **** **** **** ****  08/2024
        private List<OportunidadeTroca> IdentificarOportunidadesOrigem(string[][,] matrizes, Dictionary<string, NecessidadeProfessor> necessidades)
        {
            var oportunidades = new List<OportunidadeTroca>();

            if (_appState?.ListTurmasProf == null || matrizes == null || necessidades == null)
            {
                throw new InvalidOperationException("Os dados necessários para processar as oportunidades não estão disponíveis.");
            }

            foreach (var necessidade in necessidades.Values)
            {
                var turmaProf = _appState.ListTurmasProf.FirstOrDefault(t => t.Nome == necessidade.Turma);
                if (turmaProf == null || turmaProf.TurmaIndex < 0)
                {
                    continue;
                }

                int idxDiaSemanaMatriz = 0;
                for (int dia = 0; dia < matrizes.Length; dia++)
                {
                    string[,] matrizDia = matrizes[dia];

                    if (matrizDia == null || turmaProf.TurmaIndex >= matrizDia.GetLength(1))
                    {
                        continue;
                    }

                    for (int horario = 0; horario < matrizDia.GetLength(0); horario++)
                    {
                        bool validarOportunidade = ValidarOportunidadeDeTroca(matrizDia, idxDiaSemanaMatriz, necessidade, dia, horario, turmaProf.TurmaIndex);
                        if (validarOportunidade)
                        {
                            string obterNomeProfessorNaCelula = matrizDia[horario, turmaProf.TurmaIndex];
                            if (!string.IsNullOrEmpty(obterNomeProfessorNaCelula))
                            {
                                string CampoSeraPreenchidoDepois = "";
                                oportunidades.Add(new OportunidadeTroca(obterNomeProfessorNaCelula, dia, horario, turmaProf.TurmaIndex, CampoSeraPreenchidoDepois, idxDiaSemanaMatriz));
                            }
                        }
                    }

                    idxDiaSemanaMatriz++;
                }
            }

            return oportunidades;
        }





        private bool ValidarOportunidadeDeTroca(string[,] matriz, int idxDiaSemanaMatriz, NecessidadeProfessor necessidade, int dia, int horario, int turmaIndex)
        {
            bool validacao = false;

            string nomeProfessor = necessidade.NomeProfessor;


            //-1 validacoes
            bool achoProfessorNaLinha = _appState.VerificaProfessorMesmoHorarioQualquerTurma(matriz, horario, necessidade.NomeProfessor);
            if (achoProfessorNaLinha == true)
                validacao = false;
            else
                validacao = true;

            return validacao; // Suponha que retorne true para simplificar

        }


        private  bool ValidarOportunidadeDeTrocaDestino(string[,] matriz, NecessidadeProfessor necessidade, int dia, int horario, int turmaIndex)
        {
            bool validacao = false;
            string nomeProfessor = necessidade.NomeProfessor;

            bool achoProfessorNaLinha = _appState.VerificaProfessorMesmoHorarioQualquerTurma(matriz, horario, necessidade.NomeProfessor);
            if (achoProfessorNaLinha == true)
                validacao = false;
            else
                validacao = true;

            return validacao; // Suponha que retorne true para simplificar

        }

        private  List<OportunidadeTroca> IdentificarOportunidadesDestino(string[][,] matrizes, List<OportunidadeTroca> professorNecessidadeDestino)
        {
            var oportunidades = new List<OportunidadeTroca>();
            bool ProfessorDestinoRelacionaProfessorOrigem = false;
            int idxDiaSemanaMatriz = 0;
            foreach (var necessidade in professorNecessidadeDestino)
            {
                // Encontra o TurmaIndex correspondente ao nome da turma da necessidade

                var turmaProf = _appState.ListTurmasProf.FirstOrDefault(t => t.TurmaIndex == necessidade.TurmaIndex);
                ProfessorDestinoRelacionaProfessorOrigem = false;
                for (int dia = 0; dia < matrizes.Length; dia++)
                {

                    string[,] matrizDia = matrizes[dia];
                    for (int horario = 0; horario < matrizDia.GetLength(0); horario++)
                    {

                        bool ValidarOportunidade = ValidarOportunidadeDeTrocaDestino(matrizDia, necessidade, dia, horario, turmaProf.TurmaIndex);
                        if (ValidarOportunidade)
                        {
                            string obterNomeProfessorNaCelula = matrizDia[horario, turmaProf.TurmaIndex];
                            // #######################################################
                            /// SOMENTE PODE SER OPCAO DE TROCA NA CELULA VAZIA ////
                            /// #####################################################
                            string RelacionamentoDestino = "S";
                            string Relacionamento = "S";
                            if (obterNomeProfessorNaCelula == string.Empty)
                            {
                                ProfessorDestinoRelacionaProfessorOrigem = true;
                                AtualizarRelacionamentoTroca(oportunidadesProfessorNecessidadeOrigem, necessidade.Professor, Relacionamento);
                                oportunidades.Add(new OportunidadeTroca(necessidade.Professor, dia, horario, turmaProf.TurmaIndex, RelacionamentoDestino, idxDiaSemanaMatriz));
                            }
                        }
                    }

                    idxDiaSemanaMatriz++;

                    if (ProfessorDestinoRelacionaProfessorOrigem == false)
                    {
                        string Relacionamento = "N";
                        AtualizarRelacionamentoTroca(oportunidadesProfessorNecessidadeOrigem, necessidade.Professor, Relacionamento);
                    }
                }
            }
            return oportunidades;
        }



        public  void AtualizarRelacionamentoTroca(List<OportunidadeTroca> oportunidadesProfessorNecessidadeOrigemOuDestino, string nomeProfessor, string relacionamento)
        {
            // Percorre a lista de oportunidades de troca
            foreach (var oportunidade in oportunidadesProfessorNecessidadeOrigemOuDestino)
            {
                if (oportunidade.Professor == nomeProfessor)
                {
                    // Atualiza o campo baseado na condição booleana
                    oportunidade.ConfirmaRelacionamentoParaTroca = relacionamento;
                }
            }
        }




        private  bool ValidarOportunidadeDeTrocaDestino(string[,] matriz, OportunidadeTroca necessidade, int dia, int horario, int turmaIndex)
        {
            bool validacao = false;
            string nomeProfessor = necessidade.Professor;

            bool achoProfessorNaLinha = _appState.VerificaProfessorMesmoHorarioQualquerTurma(matriz, horario, necessidade.Professor);
            if (achoProfessorNaLinha == true)
                validacao = false;
            else
                validacao = true;

            return validacao; // Suponha que retorne true para simplificar

        }




        // Exemplo de uso dentro do método que processa as realocações
        private void ColetarPropostasDeTroca(string[][,] matrizes, List<TrocaProposta> trocasPendentes, Dictionary<string, NecessidadeProfessor> necessidades)
        {
            foreach (var necessidade in necessidades.Values)
            {
                var turmaProf = _appState.ListTurmasProf.FirstOrDefault(t => t.Nome == necessidade.Turma);
                if (turmaProf == null) continue;

                for (int dia = 0; dia < matrizes.Length; dia++)
                {
                    string[,] matrizDia = matrizes[dia];
                    for (int horario = 0; horario < matrizDia.GetLength(0); horario++)
                    {
                        if (string.IsNullOrEmpty(matrizDia[horario, turmaProf.TurmaIndex]) && horario != necessidade.Horario)
                        {
                            // Adicionando uma nova proposta de troca
                            trocasPendentes.Add(new TrocaProposta(necessidade.NomeProfessor, dia, necessidade.Horario, dia, horario, turmaProf.TurmaIndex));
                        }
                    }
                }
            }
        }





        public void EncontrarProfessorComRelacionamentoValido()
        {
            foreach (var itemOrigem in oportunidadesProfessorNecessidadeOrigem)
            {
                if (itemOrigem.ConfirmaRelacionamentoParaTroca == "S")
                {
                    // Verifica se existe um item correspondente na outra lista com as mesmas condições
                    var itemDestino = oportunidadesProfessorNecessidadeDestino.FirstOrDefault(item =>
                        item.Professor == itemOrigem.Professor &&
                        item.ConfirmaRelacionamentoParaTroca == "S");

                    if (itemDestino != null)
                    {
                        // Adiciona aos resultados se ambos os lados atendem à condição
                        resultadosOrigem.Add(itemOrigem);
                        resultadosDestino.Add(itemDestino);
                    }
                }
            }
        }

    }


}
