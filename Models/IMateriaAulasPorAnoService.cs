namespace API_HorarioProfessor.Models
{
    public interface IMateriaAulasPorAnoService
    {
        Task<IEnumerable<MateriaAulasPorAno>> GetAllMateriaAulasAsync();
        Task<MateriaAulasPorAno> GetMateriaAulaByIdAsync(int id);
        Task AddMateriaAulaAsync(MateriaAulasPorAno materiaAula);
        Task UpdateMateriaAulaAsync(int id, MateriaAulasPorAno materiaAula);
        Task DeleteMateriaAulaAsync(int id);
    }
}
