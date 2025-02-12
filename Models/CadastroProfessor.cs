namespace API_HorarioProfessor.Models
{
    public class CadastroProfessor
    {
        public int ProfessorId { get; set; }
        public int IdEscola { get; set; }
        public string NomeProfessor { get; set; }
        public string TurmasAtribuidas { get; set; }
        public string Materia { get; set; }
        public int QuantidadeAula { get; set; }
        public int AnoLetivo { get; set; } // Adicionado AnoLetivo
        public string Periodo { get; set; } // Adicionado Periodo (Manha, Tarde, Noite)    }
    }
}

