namespace API_HorarioProfessor.Models
{
    public interface IEscolaService
    {
        Task<IEnumerable<Escola>> GetAllEscolasAsync();
        Task<Escola> GetEscolaByIdAsync(int id);
        Task AddEscolaAsync(Escola escola);
        Task UpdateEscolaAsync(Escola escola);
        Task DeleteEscolaAsync(int id);
        Task<IEnumerable<UsuarioAcesso>> GetCustomUserAccessQueryAsync(int escolaId); // Novo método
        Task<IEnumerable<UsuarioAcesso>> GetCustomUserAllAccessQuery(string usuario, string senha);
    }
}
