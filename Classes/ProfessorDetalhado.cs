namespace API_HorarioProfessor.Classes
{
    public class ProfessorDetalhado
    {
        public int Id { get; set; }
        public int IdEscola { get; set; }
        public string NomeProfessor { get; set; }
        public string TurmasAtribuidas { get; set; }
        public string Materia { get; set; }
        public int QuantidadeAula { get; set; }
        public int AnoLetivo { get; set; }
        public string Periodo { get; set; }
        public OpcoesProfessor OpcoesProfessor { get; set; }
    }
}
