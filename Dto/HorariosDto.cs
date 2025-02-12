using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services.CoreServices;

namespace API_HorarioProfessor.DTO
{
    public class HorariosDto
    {
        public List<string> Turmas { get; set; }
        public List<List<string>> MatrizHorariosSegunda { get; set; }
        public List<List<string>> MatrizHorariosTerca { get; set; }
        public List<List<string>> MatrizHorariosQuarta { get; set; }
        public List<List<string>> MatrizHorariosQuinta { get; set; }
        public List<List<string>> MatrizHorariosSexta { get; set; }

        // Adicionando a lista de ProfessoresNãoDistribuídos
        public ProfessoresNaoDistribuidos ProfessoresNaoDistribuidos { get; set; }
        public string Professores_quebraram_Regra_Opcoes { get; set; } = string.Empty;
    }
}
