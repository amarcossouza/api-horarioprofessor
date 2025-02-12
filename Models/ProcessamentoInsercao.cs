namespace API_HorarioProfessor.Models
{
    public class ProcessamentoInsercao
    {
        public ProfessorInsercaoPossibilidade PossibilidadeVertical { get; set; }
        public List<ProfessorInsercaoPossibilidade> PossibilidadesHorizontal { get; set; } = new List<ProfessorInsercaoPossibilidade>();
     
    }
}
