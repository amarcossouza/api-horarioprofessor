using API_HorarioProfessor.Data;
using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services.CoreServices;
using API_HorarioProfessor.Services.CoreServices.Classes;
using API_HorarioProfessor.Services.ServicesCore;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API_HorarioProfessor.Services
{
    public class ProfessorService : IProfessorService
    {
        /*        public int Id { get; set; }
        public string Nome { get; set; }
        public int AulasPorTurma { get; set; }
        public Dictionary<string, List<string>> DistribuicaoTurmas { get; set; }
        public string DiasNaoTrabalha { get; set; }
        public string NomeAtividadeDescolar { get; set; }
        public bool DobrarAula { get; set; }
        public string Restricoes { get; set; }
        public string AtividadeAtpc { get; set; }
        public string DiasAtpc { get; set; }
        public bool Prioridade { get; set; }*/

        private readonly IOpcoesProfessorService _opcoesProfessorService;
        private readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        public ProfessorService(IOpcoesProfessorService opcoesProfessorService, 
                                ApplicationDbContext context,
                                IServiceScopeFactory scopeFactory
                                )
        {
            _opcoesProfessorService = opcoesProfessorService;
            _context = context;
            _scopeFactory = scopeFactory;
        }

        // 

        //public Task<List<(string Nome, int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)>> GetProfessoresAsync()
        public async Task<List<Professor>> GetProfessoresAsync(int idEscola, string periodo, int ativo, int anoLetivo, int OrdenarMaiorNumeroOpcao1OuOpcao2)
        {
            // Configurações iniciais
            //int anoLetivo = 2024;
            //string periodo = "Manha";
            //int ativo = 1;
            //int idEscola = 1;


            // assume o valor 1 quando nao houver passado parametro
            if (OrdenarMaiorNumeroOpcao1OuOpcao2 == 0)
               OrdenarMaiorNumeroOpcao1OuOpcao2 = 1;


                // Inicializa a lista de professores
                List<Professor> listaprofessor = new List<Professor>();

            // Obter as opções de horário e os professores cadastrados
            var listaOpcaoHorario = await (new OpcoesProfessorService(_context, _scopeFactory).SelecionarOpcoesProcessamentoPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola));
           
            var obterprofessorCadastrado = await (new ProfessorNewCadastroService(_context, _scopeFactory).ObterTodosProfessoresAsync(idEscola, anoLetivo, periodo, OrdenarMaiorNumeroOpcao1OuOpcao2));

            // Iterando sobre a lista de professores cadastrados
            foreach (var professor in obterprofessorCadastrado)
            {
                // Cria uma nova instância de OpcoesProfessor para armazenar as opções deste professor
                var opcoesProfessor = new OpcoesProfessor();
                foreach (var opcao in listaOpcaoHorario.Where(o => o.IdProfessor_fk == professor.ProfessorId))
                {
                    //***** atencao ******  28/10/2024 ********************************************************************
                    //***** sempre gravar as opcoes na coluna opcao1  **********************************************

                    // nos estamos estamos trabalhando com a tabela intermediaria OpcaoHorarioProcessamento
                    // ela é responsavel por guardar a opcao vigente na hora de processar 
                    // e aqui a a variavel professsor somente processa a coluna opcao1 por isto estamos sempre
                    // gravando na opcao1
                    //**************************************


                    opcao.Opcao = 1;
                    opcoesProfessor.AdicionarOpcao(professor.ProfessorId, opcao.Opcao, opcao.Linha, opcao.Coluna, opcao.Tipo);
                }

                // Extraia a informação de 'DiasNaoTrabalha' da primeira opção (opcao1)
                string diasNaoTrabalha = ""; // Valor padrão caso não encontre
                var primeiraOpcao = string.Join(",", opcoesProfessor.Opcoes1.Keys); //opcoesProfessor.Opcoes1.ToString().FirstOrDefault(); // Primeiro item das opções
                //string chavesConcatenadas = string.Join(",", opcoesProfessor.Opcoes1.Keys);
                if (primeiraOpcao != null)
                {
                    diasNaoTrabalha = primeiraOpcao.ToString(); // Ajuste conforme a estrutura do seu dado
                }

                // Cria a instância de Professor com as informações combinadas
                var professorParaAdicionar = new Professor(
                    professor.ProfessorId, // Id do professor
                    professor.NomeProfessor, // Nome do professor
                    professor.QuantidadeAula, // Aulas por turma
                    new Dictionary<string, List<string>> // Distribuição de turmas (pode precisar de ajuste conforme seus dados)
                    {
                { professor.Materia, professor.TurmasAtribuidas.Split(',').ToList() }
                    },
                    diasNaoTrabalha, // Preenchido com a informação de opcao1
                    "", // NomeAtividadeDescolar (preencher conforme necessário)
                    false, // DobrarAula (preencher conforme necessário)
                    "", // Restrições (preencher conforme necessário)
                    "", // AtividadeAtpc (preencher conforme necessário)
                    "", // DiasAtpc (preencher conforme necessário)
                    false, // Prioridade (preencher conforme necessário)
                    opcoesProfessor

                );

                // Adiciona o professor à lista
                listaprofessor.Add(professorParaAdicionar);
            }



            return listaprofessor;




            //aqui
            // Exemplo de como adicionar um professor e suas opções de horário
            //    public void AdicionarOpcao(int idProfessor, int opcaoSet, int linha, int coluna, string tipo)
            var professorOpcoes1 = new OpcoesProfessor();
            //SEG1,TER5,TER6,QUA1,QUI5,QUI6
            //professorOpcoes1.AdicionarOpcao(1, 1, 1, 1, "Bloqueio");
            //professorOpcoes1.AdicionarOpcao(1, 1, 5, 2, "Acumulo Cargo");
            //professorOpcoes1.AdicionarOpcao(1, 1, 6, 2, "Hora Livre");
            //professorOpcoes1.AdicionarOpcao(1, 1, 1, 3, "Hora Livre");
            //professorOpcoes1.AdicionarOpcao(1, 1, 5, 4, "Hora Livre");
            //professorOpcoes1.AdicionarOpcao(1, 1, 6, 4, "ATPC");

            //var professores = new List<Professor> // List<(string Nome, int AulasPorTurma, Dictionary<string, List<string>> DistribuicaoTurmas, string DiasNaoTrabalha, string NomeAtividadeDescolar, bool DobrarAula, string Restricoes, string AtividadeAtpc, string DiasAtpc, bool Prioridade)>
            //{

            //    new Professor(1,"ALESSANDRA", 12, new Dictionary<string, List<string>> { { "INF", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,TER5,TER6,QUA1,QUI5,QUI6", "", true, "", "", "", false),
            //    new Professor(2,"ANDRE", 24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "QUI6,SEX6", "", true, "", "", "", false),
            //    new Professor(3,"DENISE", 24, new Dictionary<string, List<string>> { { "HIS", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "SEG1,SEX1", "", true, "", "", "", false),
            //    new Professor(4,"DIOGO", 24, new Dictionary<string, List<string>> { { "ING", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEX6", "", true, "", "", "", false),
            //    new Professor(5,"ALICE", 18, new Dictionary<string, List<string>> { { "GEO", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "SEX5,SEX6", "", true, "", "", "", false),
            //    new Professor(6,"LARISSA", 24, new Dictionary<string, List<string>> { { "EDU", new List<string> { "7W", "7X", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG6,QUI6", "", true, "", "", "", false),
            //    new Professor(7,"MARISTELA", 15, new Dictionary<string, List<string>> { { "MAT", new List<string> { "9W", "9X", "9Y" } } }, "QUA1,SEX1", "", true, "", "", "", false),
            //    new Professor(8,"IVAN", 25, new Dictionary<string, List<string>> { { "MAT", new List<string> { "6W", "6X", "6Y", "7W", "7X" } } }, "TER1,QUI1", "", true, "", "", "", false),
            //    new Professor(9,"JOAO", 12, new Dictionary<string, List<string>> { { "EDU", new List<string> { "6W", "6X", "6Y", "7Y" } } }, "", "", true, "", "", "", false),
            //    new Professor(10,"JULIA", 10, new Dictionary<string, List<string>> { { "POR", new List<string> { "7Y", "8Y" } } }, "TER6,SEX6", "", true, "", "", "", false),
            //    new Professor(11,"KATIA", 24, new Dictionary<string, List<string>> { { "ART", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "TER1,QUI1", "", true, "", "", "", false),
            //    new Professor(12,"LUIS", 25, new Dictionary<string, List<string>> { { "POR", new List<string> { "6W", "6X", "6Y", "7W", "7X" } } }, "QUI1,QUI2", "", true, "", "", "", false),
            //    new Professor(13,"RAQUEL", 20, new Dictionary<string, List<string>> { { "MAT", new List<string> { "7Y", "8Y", "8W", "8X" } } }, "SEG5,SEG6,TER5,TER6,QUI5,QUI6,", "", true, "", "", "", false),
            //    new Professor(14,"ROBERTO", 12, new Dictionary<string, List<string>> { { "LEI", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "", "", false, "", "", "", false),
            //    new Professor(15,"VITOR", 18, new Dictionary<string, List<string>> { { "HIS", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEX1", "", true, "", "", "", false),
            //    new Professor(16,"SANDRO", 25, new Dictionary<string, List<string>> { { "POR", new List<string> { "8W", "8X", "9Y", "9X", "9W" } } }, "SEX5,SEX6", "", true, "", "", "", false),
            //    new Professor(17,"PAULO", 24, new Dictionary<string, List<string>> { { "CIE", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,SEG2", "", true, "", "", "", false),
            //    new Professor(18,"ZILDA", 24, new Dictionary<string, List<string>> { { "GEO", new List<string> { "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "QUA1,QUA2", "", true, "", "", "", false),

            //    // Adicione os outros professores da mesma maneira
            //};



            //Professores = new Dictionary<string, List<(int, Dictionary<string, List<string>>, string, string, bool, string, string, string, bool, OpcoesProfessor)>>();

            //// Adicionando os dados
            //var professores = new List<Professor>
            //{
            //    new Professor(1, "ALESSANDRA", 12,
            //        new Dictionary<string, List<string>> { { "INF", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" } } }, "SEG1,TER5,TER6,QUA1,QUI5,QUI6", "", true, "", "", "", false, new OpcoesProfessor()),
            //    new Professor(2, "ANDRE", 24,
            //        new Dictionary<string, List<string>> { { "CIE", new List<string> { "6W", "6X", "6Y", "7W", "7X", "7Y" } } }, "QUI6,SEX6", "", true, "", "", "", false, new OpcoesProfessor())
            //};


            // return Task.FromResult(professores);
            //return professores;
        }



        public async Task<List<OpcaoHorarioDto>> SelecionarOpcoesPorFiltros(int anoLetivo, string periodo, bool ativo, int idEscola, int idProfessor)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"SELECT Id, IdEscola, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                      Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, HoraInativado
                      FROM OpcaoHorario
                      WHERE AnoLetivo = @AnoLetivo 
                      AND Periodo = @Periodo 
                      AND Ativo = @Ativo 
                      AND IdEscola = @IdEscola
                      AND IdProfessor_fk = @IdProfessor";

                var parameters = new
                {
                    AnoLetivo = anoLetivo,
                    Periodo = periodo,
                    Ativo = ativo,
                    IdEscola = idEscola,
                    IdProfessor = idProfessor
                };

                // Para depuração
                var sqlWithParameters = BuildSqlWithParameters(query, parameters);
                //Console.WriteLine("SQL Executed: " + sqlWithParameters);

                var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, parameters);

                return opcoes.AsList();
            }
        }

        // Método para construção da string SQL para depuração
        private string BuildSqlWithParameters(string sql, object parameters)
        {
            var properties = parameters.GetType().GetProperties();
            foreach (var property in properties)
            {
                var parameterName = "@" + property.Name;
                var parameterValue = property.GetValue(parameters)?.ToString() ?? "NULL";

                // Adiciona aspas para strings
                if (property.PropertyType == typeof(string))
                {
                    parameterValue = $"'{parameterValue}'";
                }

                sql = sql.Replace(parameterName, parameterValue);
            }

            return sql;
        }




        public async Task<List<OpcaoHorarioDto>> SelecionarTodasOpcoesProfessor(int anoLetivo, string periodo, int ativo, int idEscola)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"SELECT Id, IdEscola, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                      Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, HoraInativado
                      FROM OpcaoHorario
                      WHERE AnoLetivo = @AnoLetivo 
                      AND Periodo = @Periodo 
                      AND Ativo = @Ativo 
                      AND IdEscola = @IdEscola";
   
                var parameters = new
                {
                    AnoLetivo = anoLetivo,
                    Periodo = periodo,
                    Ativo = ativo,
                    IdEscola = idEscola
                };

                // Para depuração
                var sqlWithParameters = BuildSqlWithParameters(query, parameters);
                //Console.WriteLine("SQL Executed: " + sqlWithParameters);

                var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, parameters);

                return opcoes.AsList();
            }
        }



    }
}
