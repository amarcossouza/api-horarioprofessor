namespace API_HorarioProfessor.Dto
{
    public class OpcaoHorarioDto
    {
        //Id, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna,
        //Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro,
        //HoraCadastro, DataInativado, HoraInativado
        public int Id { get; set; }
        public int IdEscola { get; set; }
        public int IdProfessor_fk { get; set; }
        public int AnoLetivo { get; set; }
        public string Periodo { get; set; }
        public int Linha { get; set; }
        public int Coluna { get; set; }
        public string OpcaoDaSemana { get; set; }  // Atualizado
        public int Opcao { get; set; }
        public string Tipo { get; set; }
        public string Sigla { get; set; }
        public int Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public TimeSpan HoraCadastro { get; set; }
        public DateTime? DataInativado { get; set; }
        public TimeSpan? HoraInativado { get; set; }
    }
}
