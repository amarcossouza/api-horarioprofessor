using API_HorarioProfessor.Data;
using API_HorarioProfessor.Services.CoreServices.Classes;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace API_HorarioProfessor.Services.CoreServices.MetodosAuxiliares
{
    public class TurmaPadraoEscolaService
    {
        private readonly ApplicationDbContext _context;
        public TurmaPadraoEscolaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TurmaPadraoEscola>> ObterTurmasAsync(int anoLetivo, string periodo, int idEscola)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"SELECT TurmaId, IdEscola, Periodo, AnoLetivo, Turmas, Polivalente
                          FROM TurmaPadraoEscola
                          WHERE IdEscola = @IdEscola
                          AND Periodo = @Periodo
                          AND AnoLetivo = @AnoLetivo
                          Order by Turmas";

                var turmas = await connection.QueryAsync<TurmaPadraoEscola>(query, new
                {
                    IdEscola = idEscola,
                    Periodo = periodo,
                    AnoLetivo = anoLetivo
                });

                return turmas;
            }
        }
    }
}
