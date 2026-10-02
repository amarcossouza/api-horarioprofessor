using API_HorarioProfessor.Dto;

namespace API_HorarioProfessor.Services;

public interface IGradeOrigemSnapshotService
{
    Task<GradeOrigemSnapshotDto> ObterAsync(
        int idEscola,
        int anoLetivo,
        string periodo,
        int quantidadeHorarios);
}
