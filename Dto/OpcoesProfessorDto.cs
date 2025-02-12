namespace API_HorarioProfessor.Dto
{
    public class OpcoesProfessorDto
    {
        //Id, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, DataInativado, HoraInativado
        public int AnoLetivo { get; set; }  
        public int IdProfessor_fk { get; set; }  // Id do Professor
        public int IdEscola { get; set; }     // Id da Escola
        public string Periodo { get; set; }   // Período: Manhã, Tarde, Noite
        public List<OpcaoHorarioDto> Opcoes { get; set; } // Lista de Opções de Horário
        public int Opcao { get; set; }

        // Campos adicionais que podem ser úteis:
        public bool Ativo { get; set; }       // Indica se o registro está ativo
        public DateTime DataCadastro { get; set; }  // Data de cadastro
        public TimeSpan HoraCadastro { get; set; }  // Hora de cadastro
        public DateTime? DataInativado { get; set; } // Data de inativação, se houver
        public TimeSpan? HoraInativado { get; set; } // Hora de inativação, se houver
    }
}
