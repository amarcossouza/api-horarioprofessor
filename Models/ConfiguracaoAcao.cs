namespace API_HorarioProfessor.Models
{
    public class ConfiguracaoAcao
    {
        public int Id { get; set; }
        public string Acao { get; set; }  // Nome da ação (ex: "CopiarDados", "BloquearUsuario", etc.)
        public string Parametro { get; set; }  // Parâmetro específico da ação (ex: "IdUsuario", "IdEscola", etc.)
        public bool Ativo { get; set; }  // Se a ação está ativa ou não
        public DateTime DataCriacao { get; set; }  // Data de criação da configuração
        public DateTime? DataExecucao { get; set; }  // Data de última execução da ação, se aplicável
        public string Descricao { get; set; }  // Descrição da ação ou observações
    }
}
