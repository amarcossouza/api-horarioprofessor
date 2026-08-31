using API_HorarioProfessor.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using API_HorarioProfessor.Data;
using Microsoft.EntityFrameworkCore;

namespace API_HorarioProfessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MateriaAulasPorAnoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MateriaAulasPorAnoController> _logger;
        private readonly IConfiguration _configuration;

        public MateriaAulasPorAnoController(ApplicationDbContext context,
                                            ILogger<MateriaAulasPorAnoController> logger,
                                            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        // Método para listar todos os registros
        [HttpGet("listarPorEscola")]
        public async Task<IActionResult> ListarPorEscola([FromQuery] int escolaId)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }

                    // Consulta para buscar registros com base no EscolaId
                    var query = @"SELECT Id, AnoLetivo, EscolaId, DataCriacao, TipoEscola, Descricao, Sigla, TodosOsAnos, QuantidadeAulas
                          FROM hprof_materiaaulasporano
                          WHERE EscolaId = @EscolaId";

                    var resultados = await connection.QueryAsync<MateriaAulasPorAno>(query, new { EscolaId = escolaId });

                    return Ok(new { Success = true, Data = resultados });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao listar MateriaAulasPorAno: {ex.Message}");
                return BadRequest(new { message = $"Erro ao listar MateriaAulasPorAno: {ex.Message}" });
            }
        }


        [HttpPost("inserir")]
        public async Task<IActionResult> InserirMateriaAulasPorAno([FromBody] MateriaAulasPorAno materiaAulas)
        {
            try
            {
                // Definindo o fuso horário de Brasília (BRT)
                TimeZoneInfo brtZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime nowInBrazil = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtZone);

                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }

                    // Ajustando a DataCriacao para o horário de Brasília
                    materiaAulas.DataCriacao = nowInBrazil;

                    var query = @"INSERT INTO hprof_materiaaulasporano (AnoLetivo, EscolaId, DataCriacao, TipoEscola, Descricao, Sigla, TodosOsAnos, QuantidadeAulas)
                          VALUES (@AnoLetivo, @EscolaId, @DataCriacao, @TipoEscola, @Descricao, @Sigla, @TodosOsAnos, @QuantidadeAulas)";

                    await connection.ExecuteAsync(query, materiaAulas);

                    return Ok(new { Success = true, Message = "Registro inserido com sucesso." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }


        [HttpPut("atualizar/{id}")]
        public async Task<IActionResult> AtualizarMateriaAulasPorAno(int id, [FromBody] MateriaAulasPorAno materiaAulas)
        {
            try
            {
                // Verifica se o ID na URL corresponde ao ID no corpo da requisição
                if (id != materiaAulas.Id)
                {
                    return BadRequest(new { Success = false, Message = "O ID na URL deve corresponder ao ID no corpo da requisição." });
                }

                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        await connection.OpenAsync();
                    }

                    // Atualizar os dados no banco de dados
                    var query = @"UPDATE hprof_materiaaulasporano SET
                            AnoLetivo = @AnoLetivo,
                            EscolaId = @EscolaId,
                            DataCriacao = @DataCriacao,
                            TipoEscola = @TipoEscola,
                            Descricao = @Descricao,
                            Sigla = @Sigla,
                            TodosOsAnos = @TodosOsAnos,
                            QuantidadeAulas = @QuantidadeAulas
                          WHERE Id = @Id";

                    var result = await connection.ExecuteAsync(query, new
                    {
                        Id = materiaAulas.Id,
                        AnoLetivo = materiaAulas.AnoLetivo,
                        EscolaId = materiaAulas.EscolaId,
                        DataCriacao = materiaAulas.DataCriacao,
                        TipoEscola = materiaAulas.TipoEscola,
                        Descricao = materiaAulas.Descricao,
                        Sigla = materiaAulas.Sigla,
                        TodosOsAnos = materiaAulas.TodosOsAnos,
                        QuantidadeAulas = materiaAulas.QuantidadeAulas
                    });

                    if (result > 0)
                    {
                        return Ok(new { Success = true, Message = "Registro atualizado com sucesso." });
                    }
                    else
                    {
                        return NotFound(new { Success = false, Message = "Registro não encontrado." });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }




        // Método para excluir um registro
        [HttpDelete("excluir/{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    var query = "DELETE FROM hprof_materiaaulasporano WHERE Id = @Id";
                    await connection.ExecuteAsync(query, new { Id = id });

                    return Ok(new { Success = true, Message = "Registro excluído com sucesso." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir MateriaAulasPorAno: {ex.Message}");
                return BadRequest(new { message = $"Erro ao excluir MateriaAulasPorAno: {ex.Message}" });
            }
        }

        // Método para buscar um registro por ID
        [HttpGet("buscar/{id}")]
        public async Task<ActionResult<MateriaAulasPorAno>> GetById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    var query = "SELECT * FROM hprof_materiaaulasporano WHERE Id = @Id";
                    var result = await connection.QueryFirstOrDefaultAsync<MateriaAulasPorAno>(query, new { Id = id });

                    if (result == null)
                        return NotFound(new { message = "Registro não encontrado." });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar MateriaAulasPorAno: {ex.Message}");
                return BadRequest(new { message = $"Erro ao buscar MateriaAulasPorAno: {ex.Message}" });
            }
        }
    }
}
