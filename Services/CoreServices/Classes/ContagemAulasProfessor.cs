namespace API_HorarioProfessor.Services.CoreServices.Classes
{
    public class ContagemAulasProfessor
    {
        public string NomeProfessor { get; set; }
        public string Turma { get; set; }
        public int QtdAulasAgendadas { get; set; }
        public int QtdAulasRestantes { get; set; }
        public string SiglaMateria { get; set; }
        public int AnoEscolar { get; set; }

        public ContagemAulasProfessor(string nomeProfessor, string turma, int qtdAulasAgendadas, int qtdAulasRestantes, string siglaMateria, int anoEscolar)
        {
            NomeProfessor = nomeProfessor;
            Turma = turma;
            QtdAulasAgendadas = qtdAulasAgendadas;
            QtdAulasRestantes = qtdAulasRestantes;
            SiglaMateria = siglaMateria;
            AnoEscolar = anoEscolar;
        }
    }
}
