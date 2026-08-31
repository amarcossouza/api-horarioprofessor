using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API_HorarioProfessor.Models
{
    [Table("hprof_parametrodiariooficial")]
    public class ParametroDiarioOficial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Para indicar que é autoincremento
        public int Codigo { get; set; }

        [Required]
        public int CodigoEscola { get; set; }

        [Required]
        public string Tipo { get; set; }

        [Required]
        public string RF { get; set; }

        [Required]
        [Column("Informacao_Pesquisada")] // Especificando o nome da coluna no banco de dados
        public string Informacao_Pesquisada { get; set; }

        [Required]
        [Column("Situacao_Pesquisa")] // Especificando o nome da coluna no banco de dados
        public string Situacao_Pesquisa { get; set; }
    }
}
