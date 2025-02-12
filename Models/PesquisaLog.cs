namespace API_HorarioProfessor.Models
{
    public class PesquisaLog
    {
        public int Id { get; set; }
        public int EscolaId { get; set; }
        public string NomeEscola { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioLogin { get; set; }  // Adicionado o campo UsuarioLogin
        public string Descricao { get; set; }
        public DateTime DataHoraPesquisa { get; set; }
        public int QuantidadePesquisas { get; set; }
    }
}
