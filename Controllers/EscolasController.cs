using API_HorarioProfessor.Data;
using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EscolasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEscolaService _escolaService;

        public EscolasController(IEscolaService escolaService, ApplicationDbContext context)
        {
            _escolaService = escolaService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Escola>>> GetAll()
        {
            try
            {
                var escolas = await _escolaService.GetAllEscolasAsync();
                return Ok(escolas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao buscar escolas: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Escola>> Get(int id)
        {
            try
            {
                var escola = await _escolaService.GetEscolaByIdAsync(id);
                if (escola == null)
                {
                    return NotFound(new { message = "Escola não encontrada." });
                }
                return Ok(escola);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao buscar escola: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(Escola escola)
        {
            try
            {
                await _escolaService.AddEscolaAsync(escola);
                return CreatedAtAction(nameof(Get), new { id = escola.EscolaId }, escola);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao criar escola: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Escola escola)
        {
            if (id != escola.EscolaId)
            {
                return BadRequest(new { message = "ID da escola não coincide." });
            }

            try
            {
                await _escolaService.UpdateEscolaAsync(escola);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao atualizar escola: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var escola = await _escolaService.GetEscolaByIdAsync(id);
                if (escola == null)
                {
                    return NotFound(new { message = "Escola não encontrada." });
                }

                await _escolaService.DeleteEscolaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao deletar escola: {ex.Message}" });
            }
        }

        // Novo método
        [HttpGet("custom-user-access-query")]
        public async Task<ActionResult<IEnumerable<UsuarioAcesso>>> GetCustomUserAccessQuery(int escolaId)
        {
            try
            {
                var result = await _escolaService.GetCustomUserAccessQueryAsync(escolaId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao buscar dados de acesso do usuário: {ex.Message}" });
            }
        }

        // Novo método
        [HttpGet("custom-user-all_access-query")]
        public async Task<ActionResult<IEnumerable<UsuarioAcesso>>> GetCustomUserAllAccessQuery(string usuario, string senha)
        {
            try
            {
                var result = await _escolaService.GetCustomUserAllAccessQuery(usuario, senha);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao buscar todos os dados de acesso do usuário: {ex.Message}" });
            }
        }


        //[HttpPost("api/configuracaoSistema/copiarDadosEscola")]
        //public async Task<IActionResult> CopiarDadosEscola([FromQuery] int idEscolaOrigem, [FromQuery] int idEscolaDestino)
        //{
        //    try
        //    {
        //        using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //        {
        //            if (connection.State == ConnectionState.Closed)
        //            {
        //                await connection.OpenAsync();
        //            }

        //            // Passo 1: Selecionar os dados de CadastroProfessor
        //            var cadastroProfessorQuery = "SELECT * FROM CadastroProfessor WHERE IdEscola = @IdEscolaOrigem";
        //            var cadastroProfessorList = await connection.QueryAsync<CadastroProfessor>(cadastroProfessorQuery, new { IdEscolaOrigem = idEscolaOrigem });

        //            // Passo 2: Inserir os dados em CadastroProfessor com o novo IdEscola
        //            var insertCadastroProfessorQuery = @"
        //    INSERT INTO CadastroProfessor (NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo, IdEscola)
        //    VALUES (@NomeProfessor, @TurmasAtribuidas, @Materia, @QuantidadeAula, @AnoLetivo, @Periodo, @IdEscolaDestino)";

        //            foreach (var cadastro in cadastroProfessorList)
        //            {
        //                await connection.ExecuteAsync(insertCadastroProfessorQuery, new
        //                {
        //                    cadastro.NomeProfessor,
        //                    cadastro.TurmasAtribuidas,
        //                    cadastro.Materia,
        //                    cadastro.QuantidadeAula,
        //                    cadastro.AnoLetivo,
        //                    cadastro.Periodo,
        //                    IdEscolaDestino = idEscolaDestino
        //                });
        //            }

        //            // Passo 3: Selecionar os dados de OpcaoHorario
        //            var opcaoHorarioQuery = "SELECT * FROM OpcaoHorario WHERE IdEscola = @IdEscolaOrigem";
        //            var opcaoHorarioList = await connection.QueryAsync<OpcaoHorarioDto>(opcaoHorarioQuery, new { IdEscolaOrigem = idEscolaOrigem });

        //            // Passo 4: Inserir os dados em OpcaoHorario com o novo IdEscola
        //            var insertOpcaoHorarioQuery = @"
        //    INSERT INTO OpcaoHorario (IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, HoraInativado, Periodo, AnoLetivo, IdEscola)
        //    VALUES (@IdProfessor_fk, @Linha, @Coluna, @Opcao, @OpcaoDaSemana, @Tipo, @Sigla, @Ativo, @DataCadastro, @HoraCadastro, @DataInativado, @HoraInativado, @Periodo, @AnoLetivo, @IdEscolaDestino)";

        //            foreach (var opcaoHorario in opcaoHorarioList)
        //            {
        //                await connection.ExecuteAsync(insertOpcaoHorarioQuery, new
        //                {
        //                    opcaoHorario.IdProfessor_fk,
        //                    opcaoHorario.Linha,
        //                    opcaoHorario.Coluna,
        //                    opcaoHorario.Opcao,
        //                    opcaoHorario.OpcaoDaSemana,
        //                    opcaoHorario.Tipo,
        //                    opcaoHorario.Sigla,
        //                    opcaoHorario.Ativo,
        //                    opcaoHorario.DataCadastro,
        //                    opcaoHorario.HoraCadastro,
        //                    opcaoHorario.DataInativado,
        //                    opcaoHorario.HoraInativado,
        //                    opcaoHorario.Periodo,
        //                    opcaoHorario.AnoLetivo,
        //                    IdEscolaDestino = idEscolaDestino
        //                });
        //            }

        //            return Ok(new { Success = true, Message = "Dados copiados com sucesso!" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.Message });
        //    }
        //}

        [HttpPost("api/configuracaoSistema/copiarDadosEscola")]
        public async Task<IActionResult> CopiarDadosEscola([FromQuery] int idEscolaOrigem, [FromQuery] int idEscolaDestino)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }

                    // Passo 1: Selecionar os dados de CadastroProfessor
                    var cadastroProfessorQuery = "SELECT * FROM CadastroProfessor WHERE IdEscola = @IdEscolaOrigem";
                    var cadastroProfessorList = await connection.QueryAsync<CadastroProfessor>(cadastroProfessorQuery, new { IdEscolaOrigem = idEscolaOrigem });

                    // Dicionário para mapear o Id original para o novo Id inserido
                    var professorIdMap = new Dictionary<int, int>();

                    // Passo 2: Inserir os dados em CadastroProfessor com o novo IdEscola e armazenar o novo Id
                    var insertCadastroProfessorQuery = @"
                INSERT INTO CadastroProfessor (NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo, IdEscola)
                VALUES (@NomeProfessor, @TurmasAtribuidas, @Materia, @QuantidadeAula, @AnoLetivo, @Periodo, @IdEscolaDestino);
                SELECT LAST_INSERT_ID();"; // Captura o novo Id inserido

                    foreach (var cadastro in cadastroProfessorList)
                    {
                        var novoIdProfessor = await connection.ExecuteScalarAsync<int>(insertCadastroProfessorQuery, new
                        {
                            cadastro.NomeProfessor,
                            cadastro.TurmasAtribuidas,
                            cadastro.Materia,
                            cadastro.QuantidadeAula,
                            cadastro.AnoLetivo,
                            cadastro.Periodo,
                            IdEscolaDestino = idEscolaDestino
                        });

                        // Mapeia o Id original para o novo Id
                        professorIdMap[cadastro.ProfessorId] = novoIdProfessor;
                    }

                    // Passo 3: Selecionar os dados de OpcaoHorario
                    var opcaoHorarioQuery = "SELECT * FROM OpcaoHorario WHERE IdEscola = @IdEscolaOrigem";
                    var opcaoHorarioList = await connection.QueryAsync<OpcaoHorarioDto>(opcaoHorarioQuery, new { IdEscolaOrigem = idEscolaOrigem });

                    // Passo 4: Inserir os dados em OpcaoHorario com o novo IdEscola e o novo IdProfessor_fk
                    var insertOpcaoHorarioQuery = @"
                INSERT INTO OpcaoHorario (IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, HoraInativado, Periodo, AnoLetivo, IdEscola)
                VALUES (@IdProfessor_fk, @Linha, @Coluna, @Opcao, @OpcaoDaSemana, @Tipo, @Sigla, @Ativo, @DataCadastro, @HoraCadastro, @DataInativado, @HoraInativado, @Periodo, @AnoLetivo, @IdEscolaDestino)";

                    foreach (var opcaoHorario in opcaoHorarioList)
                    {
                        // Verifica se o Id do professor original tem um novo Id mapeado
                        if (professorIdMap.TryGetValue(opcaoHorario.IdProfessor_fk, out var novoIdProfessor))
                        {
                            await connection.ExecuteAsync(insertOpcaoHorarioQuery, new
                            {
                                IdProfessor_fk = novoIdProfessor,
                                opcaoHorario.Linha,
                                opcaoHorario.Coluna,
                                opcaoHorario.Opcao,
                                opcaoHorario.OpcaoDaSemana,
                                opcaoHorario.Tipo,
                                opcaoHorario.Sigla,
                                opcaoHorario.Ativo,
                                opcaoHorario.DataCadastro,
                                opcaoHorario.HoraCadastro,
                                opcaoHorario.DataInativado,
                                opcaoHorario.HoraInativado,
                                opcaoHorario.Periodo,
                                opcaoHorario.AnoLetivo,
                                IdEscolaDestino = idEscolaDestino
                            });
                        }
                        else
                        {
                            // Se o mapeamento falhar, lança uma exceção para que você saiba que algo deu errado
                            throw new Exception($"Erro ao mapear IdProfessor_fk {opcaoHorario.IdProfessor_fk} para o novo Id.");
                        }
                    }

                    return Ok(new { Success = true, Message = "Dados copiados com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }



    }
}
