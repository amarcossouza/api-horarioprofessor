namespace API_HorarioProfessor.Services.CoreServices.Classes
{
    public class TrocaProposta
    {
        public string Professor { get; set; }
        public int DiaOriginal { get; set; }
        public int HorarioOriginal { get; set; }
        public int DiaNovo { get; set; }
        public int HorarioNovo { get; set; }
        public int TurmaIndex { get; set; }

        public TrocaProposta(string professor, int diaOriginal, int horarioOriginal, int diaNovo, int horarioNovo, int turmaIndex)
        {
            Professor = professor;
            DiaOriginal = diaOriginal;
            HorarioOriginal = horarioOriginal;
            DiaNovo = diaNovo;
            HorarioNovo = horarioNovo;
            TurmaIndex = turmaIndex;
        }
    }
}
