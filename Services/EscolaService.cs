using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services
{
    public class EscolaService : IEscolaService
    {
        private readonly ApplicationDbContext _context;

        public EscolaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Escola>> GetAllEscolasAsync()
        {
            return await _context.Escolas.ToListAsync();
        }

        public async Task<Escola> GetEscolaByIdAsync(int id)
        {
            return await _context.Escolas.FirstOrDefaultAsync(e => e.EscolaId == id);
        }

        public async Task AddEscolaAsync(Escola escola)
        {
            _context.Escolas.Add(escola);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEscolaAsync(Escola escola)
        {
            _context.Escolas.Update(escola);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEscolaAsync(int id)
        {
            var escola = await _context.Escolas.FindAsync(id);
            _context.Escolas.Remove(escola);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UsuarioAcesso>> GetCustomUserAccessQueryAsync(int escolaId)
        {                   // select '0' UsuarioAcessoId, EscolaId, UsuarioAcesso Usuario, Senha, now() DataCadastro from hprof_escolas
            var result = await _context.UsuariosAcesso
                .FromSqlRaw(@"SELECT UsuarioAcessoId, EscolaId, Usuario, Senha, DataCadastro FROM hprof_usuariosacesso WHERE EscolaId = {0}
                              UNION
                              SELECT 0 UsuarioAcessoId, EscolaId, UsuarioAcesso AS Usuario, Senha, now() DataCadastro FROM hprof_escolas WHERE EscolaId = {0}
                              ORDER BY EscolaId", escolaId)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<UsuarioAcesso>> GetCustomUserAllAccessQuery(string usuario, string senha)
        {                   // select '0' UsuarioAcessoId, EscolaId, UsuarioAcesso Usuario, Senha, now() DataCadastro from hprof_escolas
            var result = await _context.UsuariosAcesso
                .FromSqlRaw(@"SELECT UsuarioAcessoId, EscolaId, Usuario, Senha, DataCadastro FROM hprof_usuariosacesso WHERE usuario = {0} and Senha = {1}
                              UNION
                              SELECT 0 UsuarioAcessoId, EscolaId, UsuarioAcesso AS Usuario, Senha, now() DataCadastro FROM hprof_escolas WHERE UsuarioAcesso = {0} and Senha = {1}
                              ORDER BY EscolaId", usuario,senha)
                .ToListAsync();

            return result;
        }
    }
}
