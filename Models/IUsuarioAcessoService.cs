using API_HorarioProfessor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_HorarioProfessor.Services
{
    public interface IUsuarioAcessoService
    {
        Task<IEnumerable<UsuarioAcesso>> GetAllUsuariosAcessoAsync();
        Task<UsuarioAcesso> GetUsuarioAcessoByIdAsync(int id);
        Task AddUsuarioAcessoAsync(UsuarioAcesso usuarioAcesso);
        Task UpdateUsuarioAcessoAsync(UsuarioAcesso usuarioAcesso);
        Task DeleteUsuarioAcessoAsync(int id);
        Task<UsuarioAcesso> AuthenticateAsync(string usuario, string senha);
    }
}
