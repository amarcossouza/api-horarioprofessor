namespace API_HorarioProfessor.Models
{
    public class UsuarioAcesso
    {
        public int UsuarioAcessoId { get; set; }
        public int EscolaId { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public DateTime DataCadastro { get; set; }

       // public Escola Escola { get; set; }
    }
}
