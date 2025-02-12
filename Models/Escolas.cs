namespace API_HorarioProfessor.Models
{
    public class Escola
    {
        public int EscolaId { get; set; }
        public string CNPJ { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string Bairro { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Diretor { get; set; }
        public string ViceDiretor { get; set; }
        public string Coordenador { get; set; }
        public string Contato { get; set; }
        public string Telefone { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public int QuantidadeTurmas { get; set; }
        public int QuantidadeTurnos { get; set; }
        public string TipoEscola { get; set; }
        public string UsuarioAcesso { get; set; }
        public string Senha { get; set; }
        public int QuantidadeHorarios { get; set; }
        public int QuantidadeHorariosManha { get; set; }
        public int QuantidadeHorariosTarde { get; set; }
        public int QuantidadeHorariosNoite { get; set; }


    }
}
