namespace API_HorarioProfessor.Models
{
    public class MateriaAulasPorAno
    {
        public int Id { get; set; } // Id único e auto incrementado
        public int AnoLetivo { get; set; } // Ano Letivo
        public string Periodo { get; set; }
        public int EscolaId { get; set; } // Id da Escola
        public DateTime DataCriacao { get; set; } // Data de Criação
        public string TipoEscola { get; set; } = "";// Tipo da Escola (máx. 20 caracteres)
        public string Descricao { get; set; } // Descrição (máx. 20 caracteres)
        public string ModalidadeEscolar { get; set; } = "";
        public string Sigla { get; set; } = ""; // Sigla (máx. 3 caracteres)
        public int TodosOsAnos { get; set; } // Indica se aplica a todos os anos (usando booleano)
        public int QuantidadeAulas { get; set; } // Quantidade de Aulas
    }
}
