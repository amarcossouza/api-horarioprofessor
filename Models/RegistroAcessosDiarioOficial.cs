using API_HorarioProfessor.Models;

namespace API_HorarioProfessor.Models
{
    public class RegistroAcessosDiarioOficial
    {
        public int EscolaId { get; set; }
        public string NomeEscola { get; set; }
        public string UsuarioLogin { get; set; }
        public int QuantidadePesquisas { get; set; }
    }
}
