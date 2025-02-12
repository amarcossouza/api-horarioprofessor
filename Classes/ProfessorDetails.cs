namespace API_HorarioProfessor.Classes
{
    public class ProfessorDetails
    {
        public int AulasPorTurma { get; set; }
        public Dictionary<string, List<string>> DistribuicaoTurmas { get; set; }
        public string DiasNaoTrabalha { get; set; }
        public string NomeAtividadeDescolar { get; set; }
        public bool DobrarAula { get; set; }
        public string Restricoes { get; set; }
        public string AtividadeAtpc { get; set; }
        public string DiasAtpc { get; set; }
        public bool Prioridade { get; set; }
        public OpcoesProfessor OpcoesProfessor { get; set; }
    }
}
