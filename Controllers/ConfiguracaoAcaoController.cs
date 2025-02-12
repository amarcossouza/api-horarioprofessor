/*
 * 
 * 
 * 
 Métodos Disponíveis no ConfiguracaoAcaoController
CRUD para ConfiguracaoAcao:

GetConfiguracoesAcao: Obtém todas as configurações.
GetConfiguracaoAcao(int id): Obtém uma configuração específica por ID.
PostConfiguracaoAcao: Cria uma nova configuração.
PutConfiguracaoAcao(int id): Atualiza uma configuração existente.
DeleteConfiguracaoAcao(int id): Exclui uma configuração específica.
Métodos Especiais:

VerificarAcaoAtiva: Verifica se uma ação específica está ativa para um determinado parâmetro.
CopiarDadosEscola: Copia dados de uma escola para outra, usando idEscolaOrigem e idEscolaDestino como parâmetros. 
 * */



using API_HorarioProfessor.Data;
using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace API_HorarioProfessor.Controllers
{
    public class ConfiguracaoAcaoController : Controller
    {


        public IActionResult Index()
        {
            return View();
        }
        private readonly ApplicationDbContext _context;

        public ConfiguracaoAcaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetConfiguracoesAcao")]
        public async Task<ActionResult<IEnumerable<ConfiguracaoAcao>>> GetConfiguracoesAcao()
        {
            return await _context.ConfiguracoesAcao.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfiguracaoAcao>> GetConfiguracaoAcao(int id)
        {
            var configuracaoAcao = await _context.ConfiguracoesAcao.FindAsync(id);

            if (configuracaoAcao == null)
            {
                return NotFound();
            }

            return configuracaoAcao;
        }


        [HttpPost("PostConfiguraAcao")]
        public async Task<ActionResult<ConfiguracaoAcao>> PostConfiguracaoAcao(ConfiguracaoAcao configuracaoAcao)
        {
            _context.ConfiguracoesAcao.Add(configuracaoAcao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConfiguracaoAcao", new { id = configuracaoAcao.Id }, configuracaoAcao);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConfiguracaoAcao(int id, ConfiguracaoAcao configuracaoAcao)
        {
            if (id != configuracaoAcao.Id)
            {
                return BadRequest();
            }

            _context.Entry(configuracaoAcao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConfiguracaoAcaoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool ConfiguracaoAcaoExists(int id)
        {
            return _context.ConfiguracoesAcao.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfiguracaoAcao(int id)
        {
            var configuracaoAcao = await _context.ConfiguracoesAcao.FindAsync(id);
            if (configuracaoAcao == null)
            {
                return NotFound();
            }

            _context.ConfiguracoesAcao.Remove(configuracaoAcao);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("verificar")]
        public async Task<IActionResult> VerificarAcaoAtiva([FromQuery] string acao, [FromQuery] string parametro)
        {
            var configuracaoAcao = await _context.ConfiguracoesAcao
                .FirstOrDefaultAsync(c => c.Acao == acao && c.Parametro == parametro && c.Ativo);

            if (configuracaoAcao == null)
            {
                return Ok(false); // Retorna falso se a configuração não existir ou não estiver ativa
            }

            return Ok(true); // Retorna verdadeiro se a configuração estiver ativa
        }

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

                    // Passo 2: Inserir os dados em CadastroProfessor com o novo IdEscola
                    var insertCadastroProfessorQuery = @"
            INSERT INTO CadastroProfessor (NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo, IdEscola)
            VALUES (@NomeProfessor, @TurmasAtribuidas, @Materia, @QuantidadeAula, @AnoLetivo, @Periodo, @IdEscolaDestino)";

                    foreach (var cadastro in cadastroProfessorList)
                    {
                        await connection.ExecuteAsync(insertCadastroProfessorQuery, new
                        {
                            cadastro.NomeProfessor,
                            cadastro.TurmasAtribuidas,
                            cadastro.Materia,
                            cadastro.QuantidadeAula,
                            cadastro.AnoLetivo,
                            cadastro.Periodo,
                            IdEscolaDestino = idEscolaDestino
                        });
                    }

                    // Passo 3: Selecionar os dados de OpcaoHorario
                    var opcaoHorarioQuery = "SELECT * FROM OpcaoHorario WHERE IdEscola = @IdEscolaOrigem";
                    var opcaoHorarioList = await connection.QueryAsync<OpcaoHorarioDto>(opcaoHorarioQuery, new { IdEscolaOrigem = idEscolaOrigem });

                    // Passo 4: Inserir os dados em OpcaoHorario com o novo IdEscola
                    var insertOpcaoHorarioQuery = @"
            INSERT INTO OpcaoHorario (IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, HoraInativado, Periodo, AnoLetivo, IdEscola)
            VALUES (@IdProfessor_fk, @Linha, @Coluna, @Opcao, @OpcaoDaSemana, @Tipo, @Sigla, @Ativo, @DataCadastro, @HoraCadastro, @DataInativado, @HoraInativado, @Periodo, @AnoLetivo, @IdEscolaDestino)";

                    foreach (var opcaoHorario in opcaoHorarioList)
                    {
                        await connection.ExecuteAsync(insertOpcaoHorarioQuery, new
                        {
                            opcaoHorario.IdProfessor_fk,
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
