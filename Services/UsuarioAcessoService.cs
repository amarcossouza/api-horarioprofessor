using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services
{
    public class UsuarioAcessoService : IUsuarioAcessoService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioAcessoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UsuarioAcesso>> GetAllUsuariosAcessoAsync()
        {
            return await _context.UsuariosAcesso.ToListAsync();
        }

        public async Task<UsuarioAcesso> GetUsuarioAcessoByIdAsync(int id)
        {
            return await _context.UsuariosAcesso.FirstOrDefaultAsync(u => u.UsuarioAcessoId == id);
        }

        public async Task AddUsuarioAcessoAsync(UsuarioAcesso usuarioAcesso)
        {
            _context.UsuariosAcesso.Add(usuarioAcesso);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUsuarioAcessoAsync(UsuarioAcesso usuarioAcesso)
        {
            _context.UsuariosAcesso.Update(usuarioAcesso);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUsuarioAcessoAsync(int id)
        {
            var usuarioAcesso = await _context.UsuariosAcesso.FindAsync(id);
            _context.UsuariosAcesso.Remove(usuarioAcesso);
            await _context.SaveChangesAsync();
        }

        public async Task<UsuarioAcesso> AuthenticateAsync(string usuario, string senha)
        {
            return await _context.UsuariosAcesso.FirstOrDefaultAsync(u => u.Usuario == usuario && u.Senha == senha);
        }
    }
}
