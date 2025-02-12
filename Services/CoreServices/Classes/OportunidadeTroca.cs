namespace API_HorarioProfessor.Services.CoreServices.Classes
{
    public class OportunidadeTroca
    {
        public string Professor { get; set; }
        public int Dia { get; set; }
        public int Horario { get; set; }
        public int TurmaIndex { get; set; }
        public string ConfirmaRelacionamentoParaTroca { get; set; }
        public int IndexMatriz { get; set; }

        public OportunidadeTroca(string professor, int dia, int horario, int turmaIndex, string confirmaRelacionamentoParaTroca, int indexMatriz)
        {
            Professor = professor;
            Dia = dia;
            Horario = horario;
            TurmaIndex = turmaIndex;
            ConfirmaRelacionamentoParaTroca = confirmaRelacionamentoParaTroca;
            IndexMatriz = indexMatriz;
        }
    }
}
