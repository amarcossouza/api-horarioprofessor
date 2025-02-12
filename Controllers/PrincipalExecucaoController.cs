using API_HorarioProfessor.Classes;
using API_HorarioProfessor.Data;
using API_HorarioProfessor.Dto;
using API_HorarioProfessor.DTO;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using API_HorarioProfessor.Services.CoreServices;
using API_HorarioProfessor.Services.CoreServices.MetodosAuxiliares;
using API_HorarioProfessor.Services.ServicesCore;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrincipalExecucaoController : ControllerBase
    {
        private readonly PrincipalService _principalService;
        private readonly AppState _appState;
        private readonly ApplicationDbContext _context;

        private readonly HorarioService _horarioService;
        private readonly IOpcoesProfessorService _opcoesProfessorService;
        private readonly IProfessorNewCadastroService _professorNewCadastroService;


        private readonly IProfessorService _ProfessorService;
        private readonly HorarioDistribuicaoService _horarioDistribuicaoService;

        public PrincipalExecucaoController(PrincipalService principalService, AppState appState, ApplicationDbContext context,
            HorarioService horarioService, IOpcoesProfessorService opcoesProfessorService,
            IProfessorNewCadastroService professorNewCadastroService,
            IProfessorService ProfessorService,
            HorarioDistribuicaoService horarioDistribuicaoService
            )
        {
            _principalService = principalService;
            _appState = appState;
            _context = context;
            _horarioService = horarioService;
            _opcoesProfessorService = opcoesProfessorService;
            _professorNewCadastroService = professorNewCadastroService;
            _ProfessorService = ProfessorService;
             _horarioDistribuicaoService = horarioDistribuicaoService;

        }


        [HttpPost]
        [Route("executar")]
        public async Task<IActionResult> Executar(int idEscola, string periodo, int ativo, int anoLetivo, int QuantidadeHorarios)
        {
            try
            {
                int OrdenarMaiorNumeroOpcao1OuOpcao2 = 1;
                 await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);
                return Ok("Execução realizada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro durante a execução: {ex.Message}");
            }
        }

        [HttpGet("horarios")]
        public async Task<ActionResult<HorariosDto>> GetHorarios([FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int ativo, [FromQuery] int anoLetivo,[FromQuery] int QuantidadeHorarios )
        {
            // obter todas as opcoes dos professores 
            ///List<OpcaoHorarioDto> listaOpcaoHorario = await _ProfessorService.SelecionarTodasOpcoesProfessor(anoLetivo, periodo, ativo, idEscola);

            //faz atualizacao das opcoes do cadastroprofessor a tabela de opcoes
            // atualiza as colunas  QuantidadeOpcao1  e QuantidadeOpcao2 
           // await AtualizarQuantidadeOpcoesAsync(anoLetivo, periodo, ativo, idEscola);




            int OrdenarMaiorNumeroOpcao1OuOpcao2 = 1;
            // obter a lista de professores vamos utilizar logo abaixo.
            List<ProfessorNewCadastroDto> listaProfessor = await _professorNewCadastroService.ObterTodosProfessoresAsync(idEscola, anoLetivo, periodo, OrdenarMaiorNumeroOpcao1OuOpcao2);

            

            // antes de comecar o processamento devo apagar todos os registros da tabela 
            // OpcaoHorarioProcessamento e depois carregar as opcoes padrão para tabela OpcaoHorario.
            // o sistema deve ler a tabela OpcaoHorarioProcessamento com as opcoes atuais para processamento

            // ** tabela opcoesHorario onde há o cadastro das opcoes do professor **
            int AcumuloCargo = 0;
            int exclusao = await _opcoesProfessorService.ApagarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola, AcumuloCargo);
            await Task.Delay(500); // Pequeno atraso para garantir a propagação
            // vou processar a primeira opcao do professor

            // --primeira vez
            int opcao = 1;             
int registrosCopiados = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoAsync(anoLetivo, periodo, ativo, idEscola, opcao);
            await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);

            //verifica se ha professors fora da grade
            var necessidadesProfessores = _appState.necessidadesProfessores;

            //////// -- segunda vez
            //////if (necessidadesProfessores.Count > 0)
            //////{
            //////    //exclui todos os registros das opcoes
            //////    exclusao = await _opcoesProfessorService.ApagarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola);
            //////    await Task.Delay(500);  // Pequeno atraso para garantir a propagação
            //////    // obtem novos fk_professor das necessidades professor
            //////    List<int> professoresExcluidos = ObterProfessoresExcluidos(necessidadesProfessores, listaProfessor);

            //////    // copiar todas as opcoes 1 exceto as dos professores que nao distribuiu
            //////    opcao = 1;
            //////    int registrosCopiadosComExlusao_not_in = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, professoresExcluidos, "NOT IN");
            //////    await Task.Delay(500);  // Pequeno atraso para garantir a propagação
            //////    // copiar todas as opcoes 2 mas somente as que forem dos professores excluidos 
            //////    // precisa mudar o parametro da query opcao para 2
            //////    opcao = 2;
            //////    OrdenarMaiorNumeroOpcao1OuOpcao2 = 2;
            //////    int registrosCopiadosComExlusao_in = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, professoresExcluidos, "IN");
            //////    await Task.Delay(500);  // Pequeno atraso para garantir a propagação

            //////    await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);
            //////}

            ////////verifica se ha professors fora da grade
            //////necessidadesProfessores = _appState.necessidadesProfessores;



            ////////// -- terceira vez
            //if (necessidadesProfessores.Count > 0)
            //{
            //    //exclui todos os registros das opcoes
            //    exclusao = await _opcoesProfessorService.ApagarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola);
            //    // obtem novos fk_professor das necessidades professor
            //    List<int> professoresExcluidos = ObterProfessoresExcluidos(necessidadesProfessores, listaProfessor);

            //    // copiar todas as opcoes 1 exceto as dos professores que nao distribuiu
            //    opcao = 2;
            //    int registrosCopiadosComExlusao_not_in = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, professoresExcluidos, "NOT IN");
            //    // copiar todas as opcoes 2 mas somente as que forem dos professores excluidos 
            //    // precisa mudar o parametro da query opcao para 2
            //    opcao = 2;
            //    OrdenarMaiorNumeroOpcao1OuOpcao2 = 2;
            //    int registrosCopiadosComExlusao_in = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, professoresExcluidos, "IN");

            //    //await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2);
            //}



            // outra forma de tentativa processando recursivamente um professor de cada vez
            //await ProcessarNecessidadesProfessoresAsync(anoLetivo, periodo, ativo, idEscola, listaProfessor);


            //necessidadesProfessores = _appState.necessidadesProfessores;

            ////if(necessidadesProfessores.Count > 0)
            ////{
            ////    //List<int> listaProfessores = new List<int> { 181, 188 };
            ////    List<int> listaProfessores = ObterProfessoresExcluidos(necessidadesProfessores, listaProfessor);
            ////    int registrosApagados = await _opcoesProfessorService.ApagarOpcoesPorFiltrosComProfessoresAsync(anoLetivo, periodo, ativo, idEscola, listaProfessores);
            ////    opcao = 2;
            ////    int registrosCopiadosComExlusao_in = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, listaProfessores, "IN");
            ////    await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2);

            ////}

            //necessidadesProfessores = _appState.necessidadesProfessores;


            // Lista de ProfessoresNaoDistribuidos
            // Instanciação da classe ProfessoresNaoDistribuidos
            var professoresNaoDistribuidos = new ProfessoresNaoDistribuidos(); // Sempre instanciada

            ////////////////////// Verifica se _appState.necessidadesProfessores contém registros
            ////////////////////if (_appState.necessidadesProfessores != null && _appState.necessidadesProfessores.Count > 0)
            ////////////////////{
            ////////////////////    // Itera sobre as necessidades e preenche a lista
            ////////////////////    foreach (var item in _appState.necessidadesProfessores)
            ////////////////////    {
            ////////////////////        var necessidade = item.Value;


            ////////////////////        // OBSERVE A MENSGEM ****
            ////////////////////        // INCREMETEI EM 1 PORQUE PARA  O USUARIO OLHANDO  MATRIZ AS COLUNAS E LINHAS
            ////////////////////        // COMECAM EM 1 E OLHAND A MATRIZ COMECA EM ZERO OK 

            ////////////////////        // Obtém a lista de professores atribuídos à turma
            ////////////////////        var listaProfessorParaTurma = await ObterListaProfessorAtribuidoTurmaAsync(
            ////////////////////            item.Value.Turma.ToString(),
            ////////////////////            idEscola,
            ////////////////////            periodo,
            ////////////////////            ativo,
            ////////////////////            anoLetivo
            ////////////////////        );


            ////////////////////        // Verifica as células vazias na matriz para a turma
            ////////////////////        var informacoesCelulasVazias = await VerificarCelulasVaziasAsync(
            ////////////////////            item.Value.Turma.ToString(),
            ////////////////////            listaProfessorParaTurma.Split(',').ToList() // Converte a string para uma lista
            ////////////////////        );


            ////////////////////        // Adiciona o professor à lista
            ////////////////////        professoresNaoDistribuidos.AdicionarProfessor(
            ////////////////////            professor_Turma: $"{necessidade.NomeProfessor} - {necessidade.Turma}",
            ////////////////////            professor: necessidade.NomeProfessor,
            ////////////////////            turma: necessidade.Turma,
            ////////////////////            qtdAula: necessidade.QtdAula,
            ////////////////////            diaDaSemana: necessidade.DiaDaSemana +1, //*** ACRESCENTEI MAIS UM PORQUE NA MATRIZ E ZERO 
            ////////////////////            horario: necessidade.Horario + 1,    //** ACRESCENTEI MAIS UM PORQUE 
            ////////////////////            aulasRestantes: necessidade.QtdAula,
            ////////////////////            listaProfessorAtribuidoTurma : listaProfessorParaTurma                  
            ////////////////////        );
            ////////////////////    }
            ////////////////////}

            ////////////////////// Garantia de que a lista interna está instanciada (mesmo se vazia)
            ////////////////////if (professoresNaoDistribuidos.ListProfessoresNaoDistribuidos == null)
            ////////////////////{
            ////////////////////    professoresNaoDistribuidos.ListProfessoresNaoDistribuidos = new List<ProfessoresNaoDistribuidos>();
            ////////////////////}

            var professorNaoDistribuido = new ProfessoresNaoDistribuidos();

            professorNaoDistribuido.ListaProfessorAtribuidoTurma = string.Empty; // ou valor padrão

            // Verifica se _appState.necessidadesProfessores contém registros
            if (_appState.necessidadesProfessores != null && _appState.necessidadesProfessores.Count > 0)
            {




                // Itera sobre as necessidades e preenche a lista
                foreach (var item in _appState.necessidadesProfessores)
                {
                    var necessidade = item.Value;


                    // Obtém a lista de professores atribuídos à turma
                    var listaProfessorParaTurma = await ObterListaProfessorAtribuidoTurmaAsync(
                        item.Value.Turma.ToString(),
                        idEscola,
                        periodo,
                        ativo,
                        anoLetivo
                    );

                    // Verifica as células vazias na matriz para a turma
                    var informacoesCelulasVazias = await VerificarCelulasVaziasAsync(
                        item.Value.Turma.ToString(),
                        listaProfessorParaTurma.Split(',').ToList() // Converte a string para uma lista
                    );

                    // Cria uma nova instância de ProfessoresNaoDistribuidos para adicionar à lista
                    professorNaoDistribuido = new ProfessoresNaoDistribuidos
                    {
                        Professor_Turma = $"{necessidade.NomeProfessor} - {necessidade.Turma}",
                        Professor = necessidade.NomeProfessor,
                        Turma = necessidade.Turma,
                        QtdAula = necessidade.QtdAula,
                        DiaDaSemana = necessidade.DiaDaSemana + 1, // Ajustado para exibição
                        Horario = necessidade.Horario + 1,        // Ajustado para exibição
                        AulasRestantes = necessidade.QtdAula,
                        ListaProfessorAtribuidoTurma = listaProfessorParaTurma
                    };

                    // Verifica e adiciona informações de células vazias, se existirem
                    if (informacoesCelulasVazias != null && informacoesCelulasVazias.Any())
                    {
                        foreach (var infoCelula in informacoesCelulasVazias)
                        {

                            var professoresComOpcoes = await ObterOpcoesPorProfessoresAsync(
                                string.Join(",", infoCelula.NomesNaoEncontrados),
                                idEscola,
                                periodo,
                                anoLetivo
                            );


                            professorNaoDistribuido.VerificaProfessor_NAO_AtribuidoNaLinha.Add(new ListaProfessoresSim_NaoAtribuidosLinha
                            {
                                DiaSemana = infoCelula.DiaDaSemana,
                                Linha = infoCelula.Linha + 1, // Ajuste para exibição (começa em 1)
                                NaoProfessorLinha = string.Join(", ", infoCelula.NomesNaoEncontrados), // Nomes faltando na linha
                                SimProfessorLinha = string.Join(", ", infoCelula.NomesEncontrados), // Nomes faltando na linha
                                OpcoesNaoProfessoresLinha = professoresComOpcoes

                            });
                        }
                    }

                    // Adiciona o professor à lista final
                    professoresNaoDistribuidos.Professor_Turma_NaoDistribuidosNaLinha.Add(professorNaoDistribuido);
                }
            }
            else
            {
                // Garante que a lista ListProfessoresNaoDistribuidos está instanciada
                professoresNaoDistribuidos.Professor_Turma_NaoDistribuidosNaLinha = new List<ProfessoresNaoDistribuidos>();

                if (professorNaoDistribuido != null)
                {
                    if (string.IsNullOrEmpty(professorNaoDistribuido.ListaProfessorAtribuidoTurma))
                    {
                        professorNaoDistribuido.ListaProfessorAtribuidoTurma = string.Empty; // Define padrão se necessário
                    }
                }

                
            }





            var horariosDto = new HorariosDto
            {
                Turmas = _appState.Turmas,
                MatrizHorariosSegunda = await ConvertMatrixToList(_appState.matrizHorariosSegunda),
                MatrizHorariosTerca = await ConvertMatrixToList(_appState.matrizHorariosTerca),
                MatrizHorariosQuarta = await ConvertMatrixToList(_appState.matrizHorariosQuarta),
                MatrizHorariosQuinta = await ConvertMatrixToList(_appState.matrizHorariosQuinta),
                MatrizHorariosSexta = await ConvertMatrixToList(_appState.matrizHorariosSexta),


                ProfessoresNaoDistribuidos = professoresNaoDistribuidos,

            };


            return Ok(horariosDto);
        }



        [HttpGet("horariosFechamento")]
        public async Task<ActionResult<HorariosDto>> GetHorariosFechamento([FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int ativo, [FromQuery] int anoLetivo, [FromQuery] int QuantidadeHorarios, int Fechamento)
        {
            // obter todas as opcoes dos professores 
            //List<OpcaoHorarioDto> listaOpcaoHorario = await _ProfessorService.SelecionarTodasOpcoesProfessor(anoLetivo, periodo, ativo, idEscola);

            //faz atualizacao das opcoes do cadastroprofessor a tabela de opcoes
            // atualiza as colunas  QuantidadeOpcao1  e QuantidadeOpcao2 
            // await AtualizarQuantidadeOpcoesAsync(anoLetivo, periodo, ativo, idEscola);

            // PROCESSA A PRIMEIRA VEZ COM ZERO PARA PEGAR OS DADOS COM FALHAS NA DISTRIBUICAO
            _appState.Fechamento = 0;


            int OrdenarMaiorNumeroOpcao1OuOpcao2 = 1;
            // obter a lista de professores vamos utilizar logo abaixo.
            List<ProfessorNewCadastroDto> listaProfessor = await _professorNewCadastroService.ObterTodosProfessoresAsync(idEscola, anoLetivo, periodo, OrdenarMaiorNumeroOpcao1OuOpcao2);

            // ** tabela opcoesHorario onde há o cadastro das opcoes do professor **
            int AcumuloCargo = 0;
            int exclusao = await _opcoesProfessorService.ApagarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola, AcumuloCargo);
            await Task.Delay(500); // Pequeno atraso para garantir a propagação
            // vou processar a primeira opcao do professor

            // --primeira vez
            int opcao = 1;
            int registrosCopiados = await _opcoesProfessorService.CopiarOpcoesParaProcessamentoAsync(anoLetivo, periodo, ativo, idEscola, opcao);
            await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);

            //verifica se ha professors fora da grade
            var necessidadesProfessores = _appState.necessidadesProfessores;


            // =============================================================================================

            // ========= O CODIGO QUE SEGUE É PARA MONTAR O RELATORIO ADICIONAL MOSTRANDO
            // ========= OS PROFESSORES QUE NAO ENTRARM NA DISTRIBUICAO 



            // Lista de ProfessoresNaoDistribuidos
            // Instanciação da classe ProfessoresNaoDistribuidos
            var professoresNaoDistribuidos = new ProfessoresNaoDistribuidos(); // Sempre instanciada

 
            var professorNaoDistribuido = new ProfessoresNaoDistribuidos();

            professorNaoDistribuido.ListaProfessorAtribuidoTurma = string.Empty; // ou valor padrão

            // Verifica se _appState.necessidadesProfessores contém registros
            if (_appState.necessidadesProfessores != null && _appState.necessidadesProfessores.Count > 0)
            {
                // Itera sobre as necessidades e preenche a lista
                foreach (var item in _appState.necessidadesProfessores)
                {
                    var necessidade = item.Value;

                    // Obtém a lista de professores atribuídos à turma
                    var listaProfessorParaTurma = await ObterListaProfessorAtribuidoTurmaAsync(
                        item.Value.Turma.ToString(),
                        idEscola,
                        periodo,
                        ativo,
                        anoLetivo
                    );

                    // Verifica as células vazias na matriz para a turma
                    var informacoesCelulasVazias = await VerificarCelulasVaziasAsync(
                        item.Value.Turma.ToString(),
                        listaProfessorParaTurma.Split(',').ToList() // Converte a string para uma lista
                    );

                    // Cria uma nova instância de ProfessoresNaoDistribuidos para adicionar à lista
                    professorNaoDistribuido = new ProfessoresNaoDistribuidos
                    {
                        Professor_Turma = $"{necessidade.NomeProfessor} - {necessidade.Turma}",
                        Professor = necessidade.NomeProfessor,
                        Turma = necessidade.Turma,
                        QtdAula = necessidade.QtdAula,
                        DiaDaSemana = necessidade.DiaDaSemana + 1, // Ajustado para exibição
                        Horario = necessidade.Horario + 1,        // Ajustado para exibição
                        AulasRestantes = necessidade.QtdAula,
                        ListaProfessorAtribuidoTurma = listaProfessorParaTurma
                    };

                    // Verifica e adiciona informações de células vazias, se existirem
                    if (informacoesCelulasVazias != null && informacoesCelulasVazias.Any())
                    {
                        foreach (var infoCelula in informacoesCelulasVazias)
                        {

                            var professoresComOpcoes = await ObterOpcoesPorProfessoresAsync(
                                string.Join(",", infoCelula.NomesNaoEncontrados),
                                idEscola,
                                periodo,
                                anoLetivo
                            );


                            professorNaoDistribuido.VerificaProfessor_NAO_AtribuidoNaLinha.Add(new ListaProfessoresSim_NaoAtribuidosLinha
                            {
                                DiaSemana = infoCelula.DiaDaSemana,
                                Linha = infoCelula.Linha + 1, // Ajuste para exibição (começa em 1)
                                NaoProfessorLinha = string.Join(", ", infoCelula.NomesNaoEncontrados), // Nomes faltando na linha
                                SimProfessorLinha = string.Join(", ", infoCelula.NomesEncontrados), // Nomes faltando na linha
                                OpcoesNaoProfessoresLinha = professoresComOpcoes

                            });
                        }
                    }

                    // Adiciona o professor à lista final
                    professoresNaoDistribuidos.Professor_Turma_NaoDistribuidosNaLinha.Add(professorNaoDistribuido);
                }
            }
            else
            {
                // Garante que a lista ListProfessoresNaoDistribuidos está instanciada
                professoresNaoDistribuidos.Professor_Turma_NaoDistribuidosNaLinha = new List<ProfessoresNaoDistribuidos>();

                if (professorNaoDistribuido != null)
                {
                    if (string.IsNullOrEmpty(professorNaoDistribuido.ListaProfessorAtribuidoTurma))
                    {
                        professorNaoDistribuido.ListaProfessorAtribuidoTurma = string.Empty; // Define padrão se necessário
                    }
                }
            }



            // @@@@@@@@@@@@ PROCESSAR O COM OS DADOS FALTANTES

            // APOS TODO O PROCESSAMENTO PROCESSAR COM OS DADOS FALTANTS
            _appState.Fechamento = Fechamento;
            _appState.necessidadesProfessores = necessidadesProfessores;
            _appState.necessidadesProfessoresFechamento = necessidadesProfessores;
            OrdenarMaiorNumeroOpcao1OuOpcao2 = 1;
            // ** tabela opcoesHorario onde há o cadastro das opcoes do professor **
            var AcumuloCargo_manter = 1;
            exclusao = await _opcoesProfessorService.ApagarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola, AcumuloCargo_manter);
            await Task.Delay(500); // Pequeno atraso para garantir a propagação
          
            await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);



            var ler = _appState.necessidadesProfessoresFechamento;
            var x = _appState.necessidadesProfessores;
            _appState.necessidadesProfessoresFechamento = _appState.necessidadesProfessores;
            await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);
            for (int i = 0; i < 2; i++)
            {
                if(_appState.necessidadesProfessores.Count > 0)
                {
                    _appState.necessidadesProfessoresFechamento = _appState.necessidadesProfessores;
                    await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);
                }
            }

            string Professores_quebraram_Regra_Opcoes = string.Empty;

            //*******************
            // obter as opcoes para verificacao  

            var opcoesProfessor = await _ProfessorService.SelecionarTodasOpcoesProfessor(anoLetivo, periodo, ativo, idEscola);
            var listProfessor = await _ProfessorService.GetProfessoresAsync(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2);

            ////List<Professor> listProfessor = new List<Professor>();
            ////var opcoes = ((List<OpcaoHorarioDto>)opcoesProfessor)
            ////        .Where(x => x.IdProfessor_fk == item.ProfessorId)
            ////        .ToList();

            ////var opcoesProfessorInstancia = new OpcoesProfessor();
            ////string DiasNaoTrabalha = "";
            ////if (opcoes.Any())
            ////{
            ////    foreach (var opcao in opcoes)
            ////    {

            ////        opcoesProfessorInstancia.AdicionarOpcao(opcao.IdProfessor_fk, opcao.Opcao, opcao.Linha, opcao.Coluna, opcao.Tipo);
            ////        DiasNaoTrabalha += string.Join(", ", opcao.OpcaoDaSemana); //,opcao.OpcaoDaSemana + ",";
            ////    }
            ////}


            if (_appState.necessidadesProfessores.Count == 0)
            {

            }

            ////var ProfFechamento =  (_appState.necessidadesProfessores.Count == 0 ? new List<MovimentacaoProfessorFechamento>() : _appState.ListaMovimentacaoProfessorFechamento);
            //_appState.ListaMovimentacaoProfessorFechamento = (_appState.necessidadesProfessores.Count == 0 ? new List<MovimentacaoProfessorFechamento>() : _appState.ListaMovimentacaoProfessorFechamento);

  

            foreach (var item in _appState.ListaMovimentacaoProfessorFechamento)
            {

                // == BLOCO 1
                string professorInserido = item.ProfessorOrigem;
                int linhaProfessorInserido = item.MatrizProfessorOrigemLinha;
                string diaSemanaProfessorInserido = item.MatrizProfessorOrigem.Substring(0, 3).ToUpper() + (linhaProfessorInserido + 1);

                // Obter as opções de bloqueio dos professores com validação robusta
                string listOpcoesprofessorInserido =
                    !string.IsNullOrEmpty(professorInserido) &&
                    _appState.Professores.ContainsKey(professorInserido)
                        ? _appState.Professores[professorInserido].Item3
                        : string.Empty;

                int id = listaProfessor.FirstOrDefault(x => x.NomeProfessor == professorInserido).ProfessorId;
                var opcoes = ((List<OpcaoHorarioDto>)opcoesProfessor)
                        .Where(x => x.IdProfessor_fk == id)
                        .ToList();

                // Verifica se o dia da semana inserido está na lista de opções do professor
                bool existeNaLista = opcoes.Any(opcao => opcao.OpcaoDaSemana == diaSemanaProfessorInserido);
                if (existeNaLista)
                {
                    Professores_quebraram_Regra_Opcoes += professorInserido + " - " + diaSemanaProfessorInserido + ", ";
                }




                // == BLOCO 2
                string professorTroca = item.ProfessorTroca;
                int linhaProfessorTroca = item.MatrizProfessorTrocaLinha;
                string diaSemanaProfessorTroca = item.MatrizProfessorTroca.Substring(0, 3).ToUpper() + (linhaProfessorTroca + 1);


                // Obter as opções de bloqueio dos professores com validação robusta
                string listOpcoesprofessorTroca =
                    !string.IsNullOrEmpty(professorTroca) &&
                    _appState.Professores.ContainsKey(professorTroca)
                        ? _appState.Professores[professorTroca].Item3
                        : string.Empty;

                id = listaProfessor.FirstOrDefault(x => x.NomeProfessor == professorTroca).ProfessorId;
                opcoes = ((List<OpcaoHorarioDto>)opcoesProfessor)
                        .Where(x => x.IdProfessor_fk == id)
                        .ToList();

                // Verifica se o dia da semana inserido está na lista de opções do professor
                existeNaLista = opcoes.Any(opcao => opcao.OpcaoDaSemana == diaSemanaProfessorTroca);
                if (existeNaLista)
                {
                    Professores_quebraram_Regra_Opcoes += professorTroca + " - " + diaSemanaProfessorTroca + ", ";
                }




                // == BLOCO 3

                string professorDestino = item.ProfessorDestino;
                int linhaProfessorDestino = item.MatrizProfessorDestinoLinha;
                string diaSemanaProfessorDestino = item.MatrizProfessorDestino.Substring(0, 3).ToUpper() + (linhaProfessorDestino + 1);

                // Obter as opções de bloqueio dos professores com validação robusta
                string listOpcoesprofessorDestino =
                    !string.IsNullOrEmpty(professorDestino) &&
                    _appState.Professores.ContainsKey(professorDestino)
                        ? _appState.Professores[professorDestino].Item3
                        : string.Empty;

                id = listaProfessor.FirstOrDefault(x => x.NomeProfessor == professorDestino).ProfessorId;
                opcoes = ((List<OpcaoHorarioDto>)opcoesProfessor)
                        .Where(x => x.IdProfessor_fk == id)
                        .ToList();

                // Verifica se o dia da semana inserido está na lista de opções do professor
                existeNaLista = opcoes.Any(opcao => opcao.OpcaoDaSemana == diaSemanaProfessorDestino);
                if (existeNaLista)
                {
                    Professores_quebraram_Regra_Opcoes += professorDestino + " - " + diaSemanaProfessorDestino + ", ";
                }


                // == BLOCO 4

                string professorOrigem = item.ProfessorOrigem;
                int linhaProfessorOrigem = item.MatrizProfessorOrigemLinha;
                string diaSemanaProfessorOrigem = item.MatrizProfessorOrigem.Substring(0, 3).ToUpper() + (linhaProfessorOrigem + 1);


                // Obter as opções de bloqueio dos professores com validação robusta
                string listOpcoesprofessorOrigem =
                    !string.IsNullOrEmpty(professorOrigem) &&
                    _appState.Professores.ContainsKey(professorOrigem)
                        ? _appState.Professores[professorOrigem].Item3
                        : string.Empty;

                id = listaProfessor.FirstOrDefault(x => x.NomeProfessor == professorOrigem).ProfessorId;
                opcoes = ((List<OpcaoHorarioDto>)opcoesProfessor)
                        .Where(x => x.IdProfessor_fk == id)
                        .ToList();

                // Verifica se o dia da semana inserido está na lista de opções do professor
                existeNaLista = opcoes.Any(opcao => opcao.OpcaoDaSemana == diaSemanaProfessorOrigem);
                if (existeNaLista)
                {
                    Professores_quebraram_Regra_Opcoes += professorOrigem + " - " + diaSemanaProfessorOrigem + ", ";
                }


               

            }

            var horariosDto = new HorariosDto
            {
                Turmas = _appState.Turmas,
                MatrizHorariosSegunda = await ConvertMatrixToList(_appState.matrizHorariosSegunda),
                MatrizHorariosTerca = await ConvertMatrixToList(_appState.matrizHorariosTerca),
                MatrizHorariosQuarta = await ConvertMatrixToList(_appState.matrizHorariosQuarta),
                MatrizHorariosQuinta = await ConvertMatrixToList(_appState.matrizHorariosQuinta),
                MatrizHorariosSexta = await ConvertMatrixToList(_appState.matrizHorariosSexta),

                // faz tratamento so deve retornar dados inconsistente se nao conseguir fazer o fechamento.
                ProfessoresNaoDistribuidos = professoresNaoDistribuidos,

                Professores_quebraram_Regra_Opcoes =  Professores_quebraram_Regra_Opcoes
            };

       //  string[][,] matrizes = {
       // _appState.matrizHorariosSegunda,
       // _appState.matrizHorariosTerca,
       // _appState.matrizHorariosQuarta,
       // _appState.matrizHorariosQuinta,
       // _appState.matrizHorariosSexta
       // };
       // // SALVAR A PLANILHA APÓS O FECHAMENTO 
       //var retorno = await _horarioDistribuicaoService.SalvarMatrizComoJson(matrizes, _appState.Turmas, idEscola, periodo, anoLetivo);


            return Ok(horariosDto);
        }





        private async Task<string> ObterListaProfessorAtribuidoTurmaAsync(string turma, int idEscola, string periodo, int ativo, int anoLetivo)
        {
            string query = @"
            SELECT ProfessorId, NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, 
                   AnoLetivo, Periodo, fk_MateriaAulasPorAno, Modalidade
            FROM cadastroprofessor
            WHERE AnoLetivo = @AnoLetivo 
              AND Periodo = @Periodo 
              AND IdEscola = @IdEscola
            ORDER BY NomeProfessor";

            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                // Retorna uma lista de objetos CadastroProfessor
                var result = await connection.QueryAsync<CadastroProfessor>(query, new
                {
                    AnoLetivo = anoLetivo,
                    Periodo = periodo,
                    IdEscola = idEscola
                });

                // Lista para armazenar os nomes dos professores selecionados
                var professoresSelecionados = new List<string>();

                // Filtra linha por linha para verificar se a turma está contida em TurmasAtribuidas
                foreach (var registro in result)
                {
                    if (!string.IsNullOrEmpty(registro.TurmasAtribuidas) &&
                        registro.TurmasAtribuidas.Split(',').Contains(turma))
                    {
                        professoresSelecionados.Add(registro.NomeProfessor);
                    }
                }

                // Retorna os nomes concatenados por vírgula
                return string.Join(",", professoresSelecionados);
            }
        }





        private async Task<List<ProfessorComOpcoes>> ObterOpcoesPorProfessoresAsync(
            string nomesProfessores, int idEscola, string periodo, int anoLetivo)
        {
            // Divide a string de nomes em uma lista
            var nomes = nomesProfessores.Split(',').Select(nome => nome.Trim()).ToList();

            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                // Etapa 1: Busca os IDs dos professores com base nos nomes
                string queryProfessores = @"
            SELECT ProfessorId, NomeProfessor
            FROM cadastroprofessor
            WHERE NomeProfessor IN @Nomes
              AND IdEscola = @IdEscola
              AND Periodo = @Periodo
              AND AnoLetivo = @AnoLetivo";

                var professores = await connection.QueryAsync<ProfessorComOpcoes>(queryProfessores, new
                {
                    Nomes = nomes,
                    IdEscola = idEscola,
                    Periodo = periodo,
                    AnoLetivo = anoLetivo
                });

                // Etapa 2: Busca as opções para cada professor
                string queryOpcoes = @"
            SELECT OpcaoDaSemana
            FROM opcaohorario
            WHERE IdProfessor_fk = @ProfessorId
              AND IdEscola = @IdEscola
              AND Periodo = @Periodo
              AND AnoLetivo = @AnoLetivo
              AND Opcao = 1
            ORDER BY Linha, Coluna";

                foreach (var professor in professores)
                {
                    // Busca as opções para o professor atual
                    var opcoes = await connection.QueryAsync<string>(queryOpcoes, new
                    {
                        ProfessorId = professor.ProfessorId,
                        IdEscola = idEscola,
                        Periodo = periodo,
                        AnoLetivo = anoLetivo
                    });

                    // Monta a lista de opções como uma string separada por vírgulas
                    professor.ListaOpcoes = opcoes != null && opcoes.Any()
                        ? string.Join(",", opcoes)
                        : "Sem opções";
                }

                return professores.ToList();
            }
        }








        private async Task<List<InformacoesCelulaHorario>> VerificarCelulasVaziasAsync(string turma, List<string> professoresSelecionados)
        {
            // Obtém a posição da coluna da turma
            int colunaTurma = _appState.Turmas.IndexOf(turma);
            if (colunaTurma == -1)
            {
                throw new ArgumentException("A turma especificada não foi encontrada.");
            }

            // Converte a lista de professores selecionados em um HashSet para facilitar a busca
            var professoresSet = new HashSet<string>(professoresSelecionados);

            // Lista para armazenar as informações das células vazias
            var informacoesCelulas = new List<InformacoesCelulaHorario>();

            // Matrizes de segunda a sexta com identificação do dia da semana
            var matrizes = new Dictionary<string, string[,]>
            {
                { "Segunda-feira", _appState.matrizHorariosSegunda },
                { "Terça-feira", _appState.matrizHorariosTerca },
                { "Quarta-feira", _appState.matrizHorariosQuarta },
                { "Quinta-feira", _appState.matrizHorariosQuinta },
                { "Sexta-feira", _appState.matrizHorariosSexta }
            };

            foreach (var matriz in matrizes)
            {
                string diaDaSemana = matriz.Key;
                string[,] horarioMatriz = matriz.Value;

                // Percorre as linhas da matriz
                for (int linha = 0; linha < horarioMatriz.GetLength(0); linha++)
                {
                    // Verifica se a célula na coluna da turma está vazia
                    if (horarioMatriz[linha, colunaTurma] == "")
                    {
                        // Obtém os nomes na linha (exceto os vazios)
                        var nomesNaLinha = new List<string>();
                        for (int coluna = 0; coluna < horarioMatriz.GetLength(1); coluna++)
                        {
                            if (!string.IsNullOrEmpty(horarioMatriz[linha, coluna]))
                            {
                                nomesNaLinha.Add(horarioMatriz[linha, coluna]);
                            }
                        }

                        // Verifica os nomes que não estão na linha
                        var nomesNaoEncontrados = professoresSet
                            .Where(professor => !nomesNaLinha.Contains(professor))
                            .ToList();

                        // Verifica os nomes que estão na linha
                        var nomesEncontrados = professoresSet
                            .Where(professor => nomesNaLinha.Contains(professor))
                            .ToList();

                        // Adiciona as informações à lista
                        informacoesCelulas.Add(new InformacoesCelulaHorario
                        {
                            NomeMatriz = matriz.Key,
                            DiaDaSemana = diaDaSemana,
                            Linha = linha,
                            Coluna = colunaTurma,
                            NomesNaLinha = nomesNaLinha,
                            NomesNaoEncontrados = nomesNaoEncontrados,
                            NomesEncontrados = nomesEncontrados
                        });
                    }
                }
            }

            return informacoesCelulas;
        }








        private async Task<List<ListaProfessoresSim_NaoAtribuidosLinha>> LocalizarLinhasEmBrancoAsync(string turma)
        {
            // Determina a coluna correspondente à turma
            int colunaTurma = _appState.Turmas.IndexOf(turma);
            if (colunaTurma == -1)
            {
                throw new ArgumentException("A turma especificada não foi encontrada.");
            }

            // Lista para armazenar as informações das linhas em branco
            var linhasEmBrancoInfo = new List<ListaProfessoresSim_NaoAtribuidosLinha>();

            // Dicionário com as matrizes de segunda a sexta-feira
            var matrizes = new Dictionary<string, string[,]>
                {
                    { "Segunda-feira", _appState.matrizHorariosSegunda },
                    { "Terça-feira", _appState.matrizHorariosTerca },
                    { "Quarta-feira", _appState.matrizHorariosQuarta },
                    { "Quinta-feira", _appState.matrizHorariosQuinta },
                    { "Sexta-feira", _appState.matrizHorariosSexta }
                };

            // Itera sobre cada matriz e verifica as células vazias
            foreach (var matriz in matrizes)
            {
                string diaSemana = matriz.Key;
                string[,] horarioMatriz = matriz.Value;

                // Percorre as linhas da matriz
                for (int linha = 0; linha < horarioMatriz.GetLength(0); linha++)
                {
                    // Verifica se a célula na coluna da turma está vazia
                    if (horarioMatriz[linha, colunaTurma] == "")
                    {
                        // Obtém os professores na linha atual
                        var nomesNaLinha = new List<string>();
                        for (int coluna = 0; coluna < horarioMatriz.GetLength(1); coluna++)
                        {
                            if (!string.IsNullOrEmpty(horarioMatriz[linha, coluna]))
                            {
                                nomesNaLinha.Add(horarioMatriz[linha, coluna]);
                            }
                        }

                        // Registra as informações na lista
                        linhasEmBrancoInfo.Add(new ListaProfessoresSim_NaoAtribuidosLinha
                        {
                            DiaSemana = diaSemana,
                            Linha = linha,
                            NaoProfessorLinha = string.Join(", ", nomesNaLinha)
                        });
                    }
                }
            }

            return linhasEmBrancoInfo;
        }





        private List<int> ObterProfessoresExcluidos(Dictionary<string, NecessidadeProfessor> necessidadesProfessores, List<ProfessorNewCadastroDto> listaProfessor)
        {
            // Lista que irá conter os IDs dos professores excluídos
            List<int> professoresExcluidos = new List<int>();

            // Itera sobre as chaves do Dictionary no formato "ALICE-7W"
            foreach (var chave in necessidadesProfessores.Keys)
            {
                // Separa o nome antes do traço (ex: "ALICE-7W" -> "ALICE")
                string nomeProfessor = chave.Split('-')[0].Trim();

                // Procura o professor na listaProfessor usando o nome
                var professor = listaProfessor.FirstOrDefault(p => p.NomeProfessor.Equals(nomeProfessor, StringComparison.OrdinalIgnoreCase));

                // Se o professor for encontrado, adiciona o IdProfessor na lista de excluídos
                if (professor != null)
                {
                    professoresExcluidos.Add(professor.ProfessorId);
                }
            }

            // Retorna a lista de IDs dos professores excluídos
            return professoresExcluidos;
        }


        private async Task ProcessarNecessidadesProfessoresAsync(int anoLetivo, string periodo, int ativo, int idEscola, List<ProfessorNewCadastroDto> listaProfessor, int maxTentativas = 15, int tentativaAtual = 1, int? professorAnterior = null)
        {
            // Obter a lista de necessidades de professores
            var necessidadesProfessores = _appState.necessidadesProfessores;

            // Verifica se existem necessidades de professores e se ainda não atingiu o número máximo de tentativas
            if (necessidadesProfessores.Count > 0 && tentativaAtual <= maxTentativas)
            {
                // Excluir todos os registros das opções
                int AcumuloCargo = 0;
                await _opcoesProfessorService.ApagarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola, AcumuloCargo);

                // Obter a lista de fk_professores que ainda não foram distribuídos (excluídos)
                List<int> professoresExcluidos = ObterProfessoresExcluidos(necessidadesProfessores, listaProfessor);

                // Pegar o primeiro professor da lista que seja diferente do anterior
                var proximoProfessor = professoresExcluidos.FirstOrDefault(p => p != professorAnterior);

                // Se o próximo professor for o mesmo que o anterior ou se não houver mais professores, encerrar
                if (proximoProfessor == default(int) || proximoProfessor == professorAnterior)
                {
                    return; // Se for o mesmo professor e não houver outro, sair do loop
                }

                // Copiar todas as opções 1, exceto para o professor que foi excluído
                int opcao = 1;
                await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, new List<int> { proximoProfessor }, "NOT IN");

                // Copiar todas as opções 2, apenas para o professor excluído
                opcao = 2;
                await _opcoesProfessorService.CopiarOpcoesParaProcessamentoComExclusaoAsync(anoLetivo, periodo, ativo, idEscola, opcao, new List<int> { proximoProfessor }, "IN");

                // Executar o processo principal
                await _principalService.Executar(idEscola, periodo, ativo, anoLetivo, _appState.OrdenarMaiorNumeroOpcao1OuOpcao2, _appState.QuantidadeHorarios);

                // Chamar recursivamente para processar o próximo professor
                await ProcessarNecessidadesProfessoresAsync(anoLetivo, periodo, ativo, idEscola, listaProfessor, maxTentativas, tentativaAtual + 1, proximoProfessor);
            }
            else
            {
                // Se não houver mais necessidades ou as tentativas acabaram, retornar
                return;
            }
        }

        private async Task AtualizarQuantidadeOpcoesAsync(int anoLetivo, string periodo, int ativo, int idEscola)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                // Passo 1: Zerar os valores de QuantidadeOpcao1 e QuantidadeOpcao2 para todos os professores da escola, ano letivo e período selecionados
                var resetQuery = @"UPDATE CadastroProfessor 
                           SET QuantidadeOpcao1 = 0, 
                               QuantidadeOpcao2 = 0 
                           WHERE IdEscola = @IdEscola AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo";

                await connection.ExecuteAsync(resetQuery, new
                {
                    IdEscola = idEscola,
                    AnoLetivo = anoLetivo,
                    Periodo = periodo
                });

                // Passo 2: Selecionar todos os horários dos professores com base nos parâmetros
                List<OpcaoHorarioDto> listaOpcaoHorario = await _ProfessorService.SelecionarTodasOpcoesProfessor(anoLetivo, periodo, ativo, idEscola);

                // Agrupar por IdProfessor_fk e contar as opções 1 (QuantidadeOpcao1)
                var quantidadeOpcoesPorProfessor = listaOpcaoHorario
                    .GroupBy(o => o.IdProfessor_fk)              // Agrupar por IdProfessor_fk
                    .Select(g => new
                    {
                        IdProfessor = g.Key,
                        QuantidadeOpcao1 = g.Count(o => o.Opcao == 1),  // Contar quantos registros têm Opcao = 1
                        QuantidadeOpcao2 = g.Count(o => o.Opcao == 2)   // Contar quantos registros têm Opcao = 2
                    }).ToList();

                // Passo 3: Fazer o UPDATE combinando as duas contagens (Opcao1 e Opcao2)
                foreach (var professor in quantidadeOpcoesPorProfessor)
                {
                    // Fazer um único UPDATE para atualizar tanto QuantidadeOpcao1 quanto QuantidadeOpcao2
                    var updateQuery = @"UPDATE CadastroProfessor 
                                SET QuantidadeOpcao1 = @QuantidadeOpcao1, 
                                    QuantidadeOpcao2 = @QuantidadeOpcao2 
                                WHERE ProfessorId = @ProfessorId AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo";

                    await connection.ExecuteAsync(updateQuery, new
                    {
                        QuantidadeOpcao1 = professor.QuantidadeOpcao1,
                        QuantidadeOpcao2 = professor.QuantidadeOpcao2,
                        ProfessorId = professor.IdProfessor,
                        AnoLetivo = anoLetivo,
                        Periodo = periodo
                    });
                }
            }
        }




        // Método otimizado com Parallel.For
        private async Task<List<List<string>>> ConvertMatrixToList(string[,] matrix)
        {
            var result = new List<List<string>>(matrix.GetLength(0));

            Parallel.For(0, matrix.GetLength(0), i =>
            {
                var row = new List<string>(matrix.GetLength(1));
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    row.Add(matrix[i, j]);
                }
                result.Add(row);
            });

            return result;
        }

        [HttpGet("ObterMatrizesHorariosAsync")]
        public async Task<HorariosDto> ObterMatrizesHorariosAsync(int idEscola, int anoLetivo, string periodo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    await connection.OpenAsync();

                // Definir o fuso horário do Brasil
                TimeZoneInfo brtZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime dataCadastroAtual = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtZone);

                string query = @"SELECT DiaSemana, Linha, Coluna, Professor, DataCadastro 
                         FROM HorarioDistribuicao
                         WHERE IdEscola = @IdEscola AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo
                         ORDER BY DiaSemana, Linha, Coluna";

                var parametros = new { IdEscola = idEscola, AnoLetivo = anoLetivo, Periodo = periodo };

                var registros = await connection.QueryAsync(query, parametros);

                // Inicializar as listas de matrizes para cada dia da semana
                var matrizSegunda = new List<List<string>>();
                var matrizTerca = new List<List<string>>();
                var matrizQuarta = new List<List<string>>();
                var matrizQuinta = new List<List<string>>();
                var matrizSexta = new List<List<string>>();

                // Preencher as matrizes com os valores obtidos do banco
                foreach (var registro in registros)
                {
                    var linha = Convert.ToInt32(registro.Linha);
                    var coluna = Convert.ToInt32(registro.Coluna);
                    var professor = registro.Professor;
                    var dataCadastroRegistro = registro.DataCadastro ?? dataCadastroAtual;  // Usa a data atual se DataCadastro for nula

                    // Atribuir o valor à matriz correta dependendo do dia da semana
                    switch (registro.DiaSemana)
                    {
                        case "Segunda-feira":
                            InserirValorNaMatriz(matrizSegunda, linha, coluna, professor);
                            break;
                        case "Terça-feira":
                            InserirValorNaMatriz(matrizTerca, linha, coluna, professor);
                            break;
                        case "Quarta-feira":
                            InserirValorNaMatriz(matrizQuarta, linha, coluna, professor);
                            break;
                        case "Quinta-feira":
                            InserirValorNaMatriz(matrizQuinta, linha, coluna, professor);
                            break;
                        case "Sexta-feira":
                            InserirValorNaMatriz(matrizSexta, linha, coluna, professor);
                            break;
                    }
                }

                // Criar o objeto HorariosDto com as matrizes preenchidas
                return new HorariosDto
                {
                    MatrizHorariosSegunda = matrizSegunda,
                    MatrizHorariosTerca = matrizTerca,
                    MatrizHorariosQuarta = matrizQuarta,
                    MatrizHorariosQuinta = matrizQuinta,
                    MatrizHorariosSexta = matrizSexta
                };
            }
        }

        // Função corrigida para inserir valores na matriz
        private void InserirValorNaMatriz(List<List<string>> matriz, int linha, int coluna, string valor)
        {
            // Garante que a linha existe
            while (matriz.Count <= linha)
            {
                matriz.Add(new List<string>());
            }

            // Garante que as colunas existam até a posição correta
            while (matriz[linha].Count <= coluna)
            {
                matriz[linha].Add(string.Empty);
            }

            // Insere o valor na matriz
            matriz[linha][coluna] = valor;
        }






        [HttpGet("AtualizarNomePlanilhaAsync")]
        public async Task AtualizarNomePlanilhaAsync(int id, string novoNomePlanilha)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    await connection.OpenAsync();

                string query = @"UPDATE HorarioDistribuicao
                         SET NomePlanilhaEscola = @NovoNomePlanilha
                         WHERE Id = @Id";

                var parametros = new { Id = id, NovoNomePlanilha = novoNomePlanilha };

                await connection.ExecuteAsync(query, parametros);
            }
        }


        [HttpPost("SalvarHorarios")]
        public async Task<IActionResult> SalvarHorarios(string[][,] matrizes, int idEscola, string periodo, int anoLetivo)
        {
            var result = await _horarioService.SalvarHorarios(matrizes, idEscola, periodo, anoLetivo);

            if (result)
                return Ok(new { success = true, message = "Horários salvos com sucesso!" });

            return BadRequest(new { success = false, message = "Erro ao salvar horários." });
        }

        [HttpPost("SalvarMatrizComoJson")]
        public async Task<IActionResult> SalvarMatrizComoJson([FromBody] string[][,] matrizes, [FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int anoLetivo)
        {
            //criando vazio so para nao dar erros ok
            List<string> Turmas = new List<string>();
            var result = await _horarioService.SalvarMatrizComoJson(matrizes, Turmas,  idEscola, periodo, anoLetivo);

            if (result != null)
            {
                return Ok(new { success = true, message = "Matriz salva com sucesso!" });
            }

            return BadRequest(new { success = false, message = "Erro ao salvar a matriz." });
        }

        //[HttpGet("RecuperarHorarios")]
        //public async Task<IActionResult> RecuperarHorarios([FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int anoLetivo)
        //{
        //    var matrizes = await _horarioService.RecuperarMatrizComoJson(idEscola, periodo, anoLetivo);

        //    if (matrizes == null || matrizes.Length == 0)
        //    {
        //        return Ok(new { success = true, data = new List<List<List<string>>>() }); // Retorna lista vazia
        //    }

        //    // Converter cada matriz para lista de listas
        //    var matrizSegunda = await ConvertMatrixToList(matrizes[0]);
        //    var matrizTerca = await ConvertMatrixToList(matrizes[1]);
        //    var matrizQuarta = await ConvertMatrixToList(matrizes[2]);
        //    var matrizQuinta = await ConvertMatrixToList(matrizes[3]);
        //    var matrizSexta = await ConvertMatrixToList(matrizes[4]);

        //    var horariosDto = new HorariosDto
        //    {
        //        MatrizHorariosSegunda = matrizSegunda,
        //        MatrizHorariosTerca = matrizTerca,
        //        MatrizHorariosQuarta = matrizQuarta,
        //        MatrizHorariosQuinta = matrizQuinta,
        //        MatrizHorariosSexta = matrizSexta
        //    };

        //    return Ok(new { success = true, data = horariosDto });
        //}

        // Método para recuperar a matriz JSON e retornar no formato esperado
        //[HttpGet("RecuperarHorarios")]
        //public async Task<IActionResult> RecuperarHorarios([FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int anoLetivo)
        //{
        //    // Recupera as matrizes do serviço
        //    var matrizes = await _horarioService.RecuperarMatrizComoJson(idEscola, periodo, anoLetivo);

        //    // Se não houver matrizes, retorna um array vazio
        //    if (matrizes == null || matrizes.Length == 0)
        //    {
        //        return Ok(new
        //        {
        //            success = true,
        //            data = new HorariosDto
        //            {
        //                MatrizHorariosSegunda = new List<List<string>>(),
        //                MatrizHorariosTerca = new List<List<string>>(),
        //                MatrizHorariosQuarta = new List<List<string>>(),
        //                MatrizHorariosQuinta = new List<List<string>>(),
        //                MatrizHorariosSexta = new List<List<string>>()
        //            }
        //        });
        //    }

        //    // Converte cada matriz para lista de listas
        //    var matrizSegunda = await ConvertMatrixToList(matrizes[0]);
        //    var matrizTerca = await ConvertMatrixToList(matrizes[1]);
        //    var matrizQuarta = await ConvertMatrixToList(matrizes[2]);
        //    var matrizQuinta = await ConvertMatrixToList(matrizes[3]);
        //    var matrizSexta = await ConvertMatrixToList(matrizes[4]);

        //    // Cria o DTO com as matrizes formatadas
        //    var horariosDto = new HorariosDto
        //    {
        //        MatrizHorariosSegunda = matrizSegunda,
        //        MatrizHorariosTerca = matrizTerca,
        //        MatrizHorariosQuarta = matrizQuarta,
        //        MatrizHorariosQuinta = matrizQuinta,
        //        MatrizHorariosSexta = matrizSexta
        //    };

        //    // Retorna o objeto com sucesso e o DTO no formato correto
        //    return Ok(new { success = true, data = horariosDto });
        //}


        [HttpGet("RecuperarHorarios")] // Novo nome de rota para evitar conflito
        public async Task<ActionResult<HorariosDto>> RecuperarHorarios([FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int anoLetivo)
        {
            // Busca as matrizes diretamente
            var (matrizes, turmas) = await _horarioService.RecuperarMatrizComoJson(idEscola, periodo, anoLetivo);

            // Converte cada matriz para lista de listas
            var matrizSegunda = await ConvertMatrixToList(matrizes[0]);
            var matrizTerca = await ConvertMatrixToList(matrizes[1]);
            var matrizQuarta = await ConvertMatrixToList(matrizes[2]);
            var matrizQuinta = await ConvertMatrixToList(matrizes[3]);
            var matrizSexta = await ConvertMatrixToList(matrizes[4]);

            // Preenche o DTO com as turmas e matrizes (sem valores nulos)
            var horariosDto = new HorariosDto
            {
                Turmas = turmas, // Assumindo que _appState.Turmas nunca será nulo
                MatrizHorariosSegunda = matrizSegunda,
                MatrizHorariosTerca = matrizTerca,
                MatrizHorariosQuarta = matrizQuarta,
                MatrizHorariosQuinta = matrizQuinta,
                MatrizHorariosSexta = matrizSexta
            };

            // Retorna o DTO diretamente
            return Ok(horariosDto);
        }



    }




    public class InformacoesCelulaHorario
    {
        public string NomeMatriz { get; set; } // Nome da matriz ou dia da semana
        public string DiaDaSemana { get; set; } // Ex: "Segunda-feira"
        public int Linha { get; set; } // Linha da matriz onde a célula vazia foi encontrada
        public int Coluna { get; set; } // Coluna da turma
        public List<string> NomesNaLinha { get; set; } = new List<string>(); // Nomes encontrados na linha
        public List<string> NomesNaoEncontrados { get; set; } = new List<string>(); // Nomes faltando
        public List<string> NomesEncontrados { get; set; } = new List<string>(); // Nomes Encontrados
    }


    public class ProfessorComOpcoes
    {
        public int ProfessorId { get; set; }
        public string NomeProfessor { get; set; }
        public string ListaOpcoes { get; set; } // Ex: "SEG1,TER1,SEX1"
    }

}
