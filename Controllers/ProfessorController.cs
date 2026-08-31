using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using API_HorarioProfessor.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using API_HorarioProfessor.Classes;
using API_HorarioProfessor.Services.ServicesCore;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessorController : ControllerBase
    {
        private readonly IProfessorService _professorService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParametroDiarioOficialService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOpcoesProfessorService _opcoesProfessorService;

        public ProfessorController(IProfessorService professorService,
                                   ApplicationDbContext context,
                                   ILogger<ParametroDiarioOficialService> logger,
                                   IConfiguration configuration,
                                   IOpcoesProfessorService opcoesProfessorService)
        {
            _professorService = professorService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _opcoesProfessorService = opcoesProfessorService; 
        }

   

        //[HttpGet()]
        //public async Task<ActionResult<IEnumerable<Professor>>> Get()
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Professor>>> Get([FromQuery] int idEscola, [FromQuery] string periodo, [FromQuery] int anoLetivo, [FromQuery] int ativo)
        {
            try
            {
                //int idEscola = 1;
                //string periodo = "Manha";
                //int anoLetivo = 2024;
                //int ativo = 1;
                int IdProfessor_fk = 1;

                var professorCadastro = await BuscaCadastroProfessor(idEscola, periodo, anoLetivo) ?? new List<CadastroProfessor>();
                //var opcoesProfessor = await SelecionarOpcoesProfessor(anoLetivo, periodo, ativo, idEscola);// 
                var opcoesProfessor = await SelecionarOpcoesPorFiltros(anoLetivo, periodo, ativo, idEscola);

                List<Professor> listProfessor = new List<Professor>();

                foreach (var item in professorCadastro)
                {//(List<OpcoesProfessor>)
                    var opcoes = ((List<OpcaoHorarioDto> )opcoesProfessor)
                        .Where(x => x.IdProfessor_fk == item.ProfessorId)
                        .ToList();

                    var opcoesProfessorInstancia = new OpcoesProfessor();
                    string DiasNaoTrabalha = "";
                    if (opcoes.Any())
                    {
                        foreach (var opcao in opcoes)
                        {
                            
                            opcoesProfessorInstancia.AdicionarOpcao(opcao.IdProfessor_fk, opcao.Opcao, opcao.Linha, opcao.Coluna, opcao.Tipo);
                            DiasNaoTrabalha += string.Join(", ", opcao.OpcaoDaSemana); //,opcao.OpcaoDaSemana + ",";
                         }
                    }
                    //else
                    //{
                    //    // Adiciona opções vazias se não houver opções encontradas
                    //    opcoesProfessorInstancia = new OpcoesProfessor();
                    //    opcoesProfessorInstancia.AdicionarOpcao(item.ProfessorId, 1, 1, 1, "");
                    //    opcoesProfessorInstancia.AdicionarOpcao(item.ProfessorId, 2, 1, 1, "");
                    //}

                    var professor = new Professor(
                        item.ProfessorId,
                        item.NomeProfessor,
                        item.QuantidadeAula,
                        new Dictionary<string, List<string>> { { item.Materia, new List<string>(item.TurmasAtribuidas.Split(',')) } },
                        DiasNaoTrabalha,
                        "", // NomeAtividadeDescolar
                        true, // DobrarAula
                        "", // Restricoes
                        "", // AtividadeAtpc
                        "", // DiasAtpc
                        false, // Prioridade
                        opcoesProfessorInstancia // OpcoesProfessor
                    );

                    listProfessor.Add(professor);
                }

                if (!listProfessor.Any())
                {
                    listProfessor.Add(new Professor(
                        0, "", 0, new Dictionary<string, List<string>>(), "", "", false, "", "", "", false, new OpcoesProfessor()
                    ));
                }

                return Ok(listProfessor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao buscar dados do Professor: {ex.Message}" });
            }
        }

        ////[HttpPost("api/opcoesProfessor/inserir")]
        ////public async Task<IActionResult> InserirOpcoesProfessor(
        ////   [FromBody] OpcoesProfessorDto opcoesProfessorDto,
        ////   [FromQuery] int OpcaoProfessor,
        ////   [FromQuery] string Periodo,
        ////   [FromQuery] int AnoLetivo,
        ////   [FromQuery] int IdEscola)
        ////{
        ////    try
        ////    {
        ////        TimeZoneInfo brtZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        ////        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtZone);

        ////        using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
        ////        {
        ////            if (connection.State == ConnectionState.Closed)
        ////            {
        ////                await connection.OpenAsync();
        ////            }

        ////            // Atualizar registros antigos para inativo
        ////            var updateQuery = @"UPDATE opcaohorario
        ////                SET Ativo = FALSE,
        ////                DataInativado = @DataInativado, 
        ////                HoraInativado = @HoraInativado
        ////                WHERE IdProfessor_fk = @IdProfessor_fk 
        ////                AND IdEscola = @IdEscola
        ////                AND Opcao = @Opcao 
        ////                AND Periodo = @Periodo
        ////                AND AnoLetivo = @AnoLetivo";


        ////            await connection.ExecuteAsync(updateQuery, new
        ////            {
        ////                IdProfessor_fk = opcoesProfessorDto.IdProfessor_fk,
        ////                Opcao = OpcaoProfessor,
        ////                Periodo = Periodo,
        ////                IdEscola = IdEscola,
        ////                AnoLetivo = AnoLetivo,
        ////                DataInativado = now.Date,
        ////                HoraInativado = now.TimeOfDay
        ////            });

        ////            // Inserir novo registro
        ////            var insertQuery = @"INSERT INTO opcaohorario (IdEscola, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro)
        ////                VALUES (@IdEscola, @Periodo, @AnoLetivo, @IdProfessor_fk, @Linha, @Coluna, @Opcao, @OpcaoDaSemana, @Tipo, @Sigla, TRUE, @DataCadastro, @HoraCadastro)";

        ////            foreach (var opcao in opcoesProfessorDto.Opcoes)
        ////            {
        ////                await connection.ExecuteAsync(insertQuery, new
        ////                {
        ////                    IdEscola = IdEscola,
        ////                    Periodo = Periodo,
        ////                    AnoLetivo = AnoLetivo,
        ////                    IdProfessor_fk = opcoesProfessorDto.IdProfessor_fk,
        ////                    Linha = opcao.Linha,
        ////                    Coluna = opcao.Coluna,
        ////                    Opcao = opcao.Opcao,
        ////                    OpcaoDaSemana = opcao.OpcaoDaSemana,
        ////                    Tipo = (opcao.Tipo == "Remove" ? "" : opcao.Tipo),
        ////                    Sigla = (opcao.Sigla == "Remove" ? "" : opcao.Sigla),
        ////                    DataCadastro = now.Date,
        ////                    HoraCadastro = now.TimeOfDay
        ////                });
        ////            }
        ////        }

        ////        return Ok(new { Success = true, Message = "Opções inseridas com sucesso." });
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        return BadRequest(new { Success = false, Message = ex.Message });
        ////    }
        ////}


        [HttpPost("api/opcoesProfessor/inserir")]
        public async Task<IActionResult> InserirOpcoesProfessor(
    [FromBody] OpcoesProfessorDto opcoesProfessorDto,
    [FromQuery] int OpcaoProfessor,
    [FromQuery] string Periodo,
    [FromQuery] int AnoLetivo,
    [FromQuery] int IdEscola)
        {
            try
            {
                // Configura o fuso horário para o Brasil
                TimeZoneInfo brtZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtZone);

                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }

                    // Apagar registros antigos para este professor, escola, opção, período e ano letivo
                    var deleteOldRecordsQuery = @"DELETE FROM hprof_opcaohorario
                                          WHERE IdProfessor_fk = @IdProfessor_fk 
                                          AND IdEscola = @IdEscola
                                          AND Opcao = @Opcao 
                                          AND Periodo = @Periodo
                                          AND AnoLetivo = @AnoLetivo";

                    await connection.ExecuteAsync(deleteOldRecordsQuery, new
                    {
                        IdProfessor_fk = opcoesProfessorDto.IdProfessor_fk,
                        Opcao = OpcaoProfessor,
                        Periodo = Periodo,
                        IdEscola = IdEscola,
                        AnoLetivo = AnoLetivo
                    });

                    // Inserir novos registros
                    var insertQuery = @"INSERT INTO hprof_opcaohorario (IdEscola, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro)
                                VALUES (@IdEscola, @Periodo, @AnoLetivo, @IdProfessor_fk, @Linha, @Coluna, @Opcao, @OpcaoDaSemana, @Tipo, @Sigla, TRUE, @DataCadastro, @HoraCadastro)";

                    foreach (var opcao in opcoesProfessorDto.Opcoes)
                    {
                        await connection.ExecuteAsync(insertQuery, new
                        {
                            IdEscola = IdEscola,
                            Periodo = Periodo,
                            AnoLetivo = AnoLetivo,
                            IdProfessor_fk = opcoesProfessorDto.IdProfessor_fk,
                            Linha = opcao.Linha,
                            Coluna = opcao.Coluna,
                            Opcao = opcao.Opcao,
                            OpcaoDaSemana = opcao.OpcaoDaSemana,
                            Tipo = (opcao.Tipo == "Remove" ? "" : opcao.Tipo),
                            Sigla = (opcao.Sigla == "Remove" ? "" : opcao.Sigla),
                            DataCadastro = now.Date,
                            HoraCadastro = now.TimeOfDay
                        });
                    }
                }

                return Ok(new { Success = true, Message = "Opções inseridas com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }




        [HttpPost("api/opcoesProfessor/ExcluirTodasOpcoes")]
        public async Task<IActionResult> ExcluirTodasOpcoes(
            [FromBody] OpcoesProfessorDto opcoesProfessorDto,
            [FromQuery] int OpcaoProfessor,
            [FromQuery] string Periodo,
            [FromQuery] int AnoLetivo,
            [FromQuery] int IdEscola)
        {
            try
            {
                // Configura o fuso horário para o Brasil
                TimeZoneInfo brtZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtZone);

                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }

                    // Apagar registros antigos para este professor, escola, opção, período e ano letivo
                    var deleteOldRecordsQuery = @"DELETE FROM hprof_opcaohorario
                                          WHERE IdProfessor_fk = @IdProfessor_fk 
                                          AND IdEscola = @IdEscola                                          
                                          AND Periodo = @Periodo
                                          AND AnoLetivo = @AnoLetivo
                                          AND Opcao in (1, 2)";

                    // Executa a consulta de exclusão
                    await connection.ExecuteAsync(deleteOldRecordsQuery, new
                    {
                        IdProfessor_fk = opcoesProfessorDto.IdProfessor_fk,
                        IdEscola = opcoesProfessorDto.IdEscola,
                        Periodo = Periodo,
                        AnoLetivo = AnoLetivo
                    });
                }

                return Ok(new { Success = true, Message = "Opções 1 e 2 excluídas com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = $"Erro ao excluir as opções: {ex.Message}" });
            }
        }







        [HttpGet("api/opcoesProfessor/selecionar")] 
        public async Task<IActionResult> SelecionarOpcoesProfessor([FromQuery] int anoLetivo, [FromQuery] string periodo, [FromQuery] int ativo, [FromQuery] int idEscola)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }
                    //--AND IdProfessor = @IdProfessor
                    var query = @"SELECT Id, IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                          Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                          DataInativado, HoraInativado 
                          FROM hprof_opcaohorario
                          WHERE IdEscola = @IdEscola                    
                          AND Periodo = @Periodo 
                          AND AnoLetivo = @AnoLetivo 
                          AND Ativo = @Ativo";

                    // Correto mapeamento de parâmetros
                    var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, new
                    {
                        IdEscola = idEscola,
                        //IdProfessor = idProfessor,
                        Periodo = periodo,
                        AnoLetivo = anoLetivo,
                        Ativo = ativo
                    });

                    return Ok(new { Success = true, Data = opcoes });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }


        [HttpGet("api/opcoesProfessor/selecionarPorFiltros")]
        public async Task<IEnumerable<OpcaoHorarioDto>> SelecionarOpcoesPorFiltros(int anoLetivo, string periodo, int ativo, int idEscola)
        {
            return await  _opcoesProfessorService.SelecionarOpcoesPorFiltrosAsync(anoLetivo, periodo, ativo, idEscola);


            //using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            //{
            //    if (connection.State == System.Data.ConnectionState.Closed)
            //    {
            //        await connection.OpenAsync();
            //    }

            //    var query = @"SELECT Id, IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
            //          Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
            //          DataInativado, HoraInativado, AnoLetivo, Periodo, IdEscola 
            //          FROM opcaohorario
            //          WHERE AnoLetivo = @AnoLetivo 
            //          AND Periodo = @Periodo 
            //          AND Ativo = @Ativo 
            //          AND IdEscola = @IdEscola";

            //    var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, new
            //    {
            //        AnoLetivo = anoLetivo,
            //        Periodo = periodo,
            //        Ativo = ativo,
            //        IdEscola = idEscola
            //    });

            //    return opcoes.AsList();
            //}
        }


        //========================================================================================
        // novo metodo para juntar cadastro com as opcoes 


        [HttpGet("getProfessoresComOpcoes")]
        public async Task<IActionResult> GetProfessoresComOpcoes(int idEscola, string periodo, int anoLetivo, int ativo)
        {
            try
            {
                var listaCadastroProfessores = await BuscaCadastroProfessor(idEscola, periodo, anoLetivo);
                if (listaCadastroProfessores == null || listaCadastroProfessores.Count == 0)
                {
                    return NotFound(new { message = "Informações do Professor não foram encontradas." });
                }

                var resultado = new List<ProfessorDetalhado>();

                foreach (var cadastro in listaCadastroProfessores)
                {
                    var opcoesProfessor = await SelecionarOpcoesProfessor(cadastro.ProfessorId, idEscola, periodo,anoLetivo, ativo);

                    var professor = new ProfessorDetalhado
                    {
                        Id = cadastro.ProfessorId,
                        IdEscola = cadastro.IdEscola,
                        NomeProfessor = cadastro.NomeProfessor,
                        TurmasAtribuidas = cadastro.TurmasAtribuidas,
                        Materia = cadastro.Materia,
                        QuantidadeAula = cadastro.QuantidadeAula,
                        AnoLetivo = cadastro.AnoLetivo,
                        Periodo = cadastro.Periodo,
                        OpcoesProfessor = new OpcoesProfessor()
                    };

                    if (opcoesProfessor != null)
                    {
                        foreach (var opcao in opcoesProfessor)
                        {
                            professor.OpcoesProfessor.AdicionarOpcao(opcao.Id, 1, opcao.Linha, opcao.Coluna, opcao.Tipo);
                        }
                    }

                    resultado.Add(professor);
                }

                return Ok(new { Success = true, Data = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao buscar dados do Professor: {ex.Message}" });
            }
        }

        private async Task<List<CadastroProfessor>> BuscaCadastroProfessor(int idEscola, string periodo, int anoLetivo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"SELECT ProfessorId, IdEscola, NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo 
                        FROM hprof_cadastroprofessor
                        WHERE IdEscola = @IdEscola 
                          AND Periodo = @Periodo 
                          AND AnoLetivo = @AnoLetivo";

                var parametros = new
                {
                    IdEscola = idEscola,
                    Periodo = periodo,
                    AnoLetivo = anoLetivo,
                };

                var cadastroProfessores = await connection.QueryAsync<CadastroProfessor>(query, parametros);
                return cadastroProfessores.AsList();
            }
        }

        private async Task<List<OpcaoHorarioDto>> SelecionarOpcoesProfessor(int idProfessor, int idEscola, string periodo, int anoLetivo, int ativo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"Id, IdEscola, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, 
                              OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, 
                                HoraInativado 
                        FROM opcaohorario
                        WHERE IdEscola = @IdEscola 
                        AND IdProfessor = @IdProfessor 
                        AND Periodo = @Periodo 
                        AND AnoLetivo = @AnoLetivo 
                        AND Ativo = @Ativo";

                var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, new
                {
                    IdEscola = idEscola,
                    IdProfessor = idProfessor,
                    Periodo = periodo,
                    AnoLetivo = anoLetivo,
                    Ativo = ativo
                });

                return opcoes.AsList();
            }
        }
    }

}
