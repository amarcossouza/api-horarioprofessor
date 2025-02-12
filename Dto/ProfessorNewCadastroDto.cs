namespace API_HorarioProfessor.Dto
{
    public class ProfessorNewCadastroDto
    {
        public int ProfessorId { get; set; }
        public int IdEscola { get; set; }
        public string NomeProfessor { get; set; }
        public string TurmasAtribuidas { get; set; }
        public string Materia { get; set; }
        public int QuantidadeAula { get; set; }
        public int AnoLetivo { get; set; }
        public string Periodo { get; set; }
        public int QuantidadeOpcao1 { get; set; }
        public int QuantidadeOpcao2 { get; set; }
    }
}
