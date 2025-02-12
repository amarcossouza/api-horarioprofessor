namespace API_HorarioProfessor.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public int EscolaId { get; set; }
        public string NomeUsuario { get; set; }
        public string Senha { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now; // Data de cadastro padrão

        //public Escola Escola { get; set; } // Relação FK com Escola
    }
}
