using API_HorarioProfessor.Data;
using API_HorarioProfessor.Dto;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services
{
    public class ProfessorNewCadastroService : IProfessorNewCadastroService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        public ProfessorNewCadastroService(ApplicationDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        public async Task<List<ProfessorNewCadastroDto>> ObterTodosProfessoresAsync(int idEscola, int Anoletivo, string Periodo, int OrdenarMaiorNumeroOpcao1OuOpcao2 )
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query =
                @" SELECT ProfessorId, IdEscola, NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo, QuantidadeOpcao1, QuantidadeOpcao2  " +
                " FROM CadastroProfessor  " +
                " where idEscola = @idEscola and AnoLetivo = @AnoLetivo and Periodo = @Periodo " +
                " order by NomeProfessor";
                var professores = await connection.QueryAsync<ProfessorNewCadastroDto>(query, new { idEscola = idEscola, AnoLetivo = Anoletivo, Periodo = Periodo });

                /*
                1.Ordenação crescente(A - Z) por nome como critério de desempate:
                csharp
                Copiar código
                var professoresOrdenados = professores
                    .OrderByDescending(p => p.QuantidadeOpcao1)   // Maior número de bloqueios primeiro
                    .ThenByDescending(p => p.QuantidadeAula)      // Maior número de turmas em caso de empate
                    .ThenBy(p => p.NomeProfessor)                 // Desempate por ordem alfabética crescente (A-Z)
                    .ToList();
                */
                /*
                Ordenação decrescente(Z-A) por nome como critério de desempate:
                csharp
                Copiar código
                var professoresOrdenados = professores
                    .OrderByDescending(p => p.QuantidadeOpcao1)   // Maior número de bloqueios primeiro
                    .ThenByDescending(p => p.QuantidadeAula)      // Maior número de turmas em caso de empate
                    .ThenByDescending(p => p.NomeProfessor)       // Desempate por ordem alfabética decrescente (Z-A)
                    .ToList();
                */
                /*
                 ordenacao pela quantidade de QuantidadeOpcao1 e turma / QuantidadeAula
                var professoresOrdenados = professores
                .OrderByDescending(p => p.QuantidadeOpcao1)   // Maior número de bloqueios primeiro
                .ThenByDescending(p => p.QuantidadeAula)      // Maior número de turmas em caso de empate
                .ToList();
                */
                List<ProfessorNewCadastroDto> professoresOrdenados = new List<ProfessorNewCadastroDto>();


                if (OrdenarMaiorNumeroOpcao1OuOpcao2 == 1)
                {
                    //aqui estamos utilizando ordenacao por 
                    // quantidadeopcao1 + quantidadeAula (e ordenando dentro quantidade de aulas ordem alfabetica como desempate
                    professoresOrdenados = professores
                    .OrderByDescending(p => p.QuantidadeAula)   // Maior número de bloqueios primeiro
                    //.ThenByDescending(p => p.QuantidadeOpcao1)      // Maior número de turmas em caso de empate
                    //.ThenBy(p => p.NomeProfessor)                 // Desempate por nome alfabético
                    .ToList();

                    /*
                     backup do codigo acima 
                     professoresOrdenados = professores
                    .OrderByDescending(p => p.QuantidadeOpcao1)   // Maior número de bloqueios primeiro
                    .ThenByDescending(p => p.QuantidadeAula)      // Maior número de turmas em caso de empate
                    .ThenBy(p => p.NomeProfessor)                 // Desempate por nome alfabético
                    .ToList();
                     * */

                }

                if (OrdenarMaiorNumeroOpcao1OuOpcao2 == 2)
                {
                    //aqui estamos utilizando ordenacao por 
                    // quantidadeopcao1 + quantidadeAula (e ordenando dentro quantidade de aulas ordem alfabetica como desempate
                    professoresOrdenados = professores
                    .OrderByDescending(p => p.QuantidadeAula)   // Maior número de bloqueios primeiro
                    //.ThenByDescending(p => p.QuantidadeOpcao2)      // Maior número de turmas em caso de empate
                    //.ThenBy(p => p.NomeProfessor)                 // Desempate por nome alfabético
                    .ToList();
                }

                return professoresOrdenados;

                //return professores.AsList();
            }
        }

        public async Task<ProfessorNewCadastroDto> ObterProfessorPorIdAsync(int professorId)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var query = @"SELECT * FROM CadastroProfessor WHERE ProfessorId = @ProfessorId";
                return await connection.QuerySingleOrDefaultAsync<ProfessorNewCadastroDto>(query, new { ProfessorId = professorId });
            }
        }

        public async Task AdicionarProfessorAsync(ProfessorNewCadastroDto professor)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var query = @"INSERT INTO CadastroProfessor (IdEscola, NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo)
                              VALUES (@IdEscola, @NomeProfessor, @TurmasAtribuidas, @Materia, @QuantidadeAula, @AnoLetivo, @Periodo)";
                await connection.ExecuteAsync(query, professor);
            }
        }

        public async Task AtualizarProfessorAsync(ProfessorNewCadastroDto professor)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var query = @"UPDATE CadastroProfessor SET 
                                IdEscola = @IdEscola,
                                NomeProfessor = @NomeProfessor,
                                TurmasAtribuidas = @TurmasAtribuidas,
                                Materia = @Materia,
                                QuantidadeAula = @QuantidadeAula,
                                AnoLetivo = @AnoLetivo,
                                Periodo = @Periodo
                              WHERE ProfessorId = @ProfessorId";
                await connection.ExecuteAsync(query, professor);
            }
        }

        public async Task DeletarProfessorAsync(int professorId)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var query = @"DELETE FROM CadastroProfessor WHERE ProfessorId = @ProfessorId";
                await connection.ExecuteAsync(query, new { ProfessorId = professorId });
            }
        }
    }
}
