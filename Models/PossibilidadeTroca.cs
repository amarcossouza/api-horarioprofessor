namespace API_HorarioProfessor.Models
{
    public class PossibilidadeTroca
    {
        public string MatrizOrigem { get; set; } // Nome da matriz do professor de origem
        public string MatrizDestino { get; set; } // Nome da matriz do professor de destino
        public int LinhaOrigem { get; set; } // Linha da origem
        public int ColunaOrigem { get; set; } // Coluna da origem
        public string ProfessorOrigem { get; set; } // Nome do professor na origem
        public int LinhaDestino { get; set; } // Linha do destino
        public int ColunaDestino { get; set; } // Coluna do destino
        public string ProfessorDestino { get; set; } // Nome do professor no destino
        public bool TrocaOrigemParaDestinoValida { get; set; } // Troca válida da origem para o destino
        public bool TrocaDestinoParaOrigemValida { get; set; } // Troca válida do destino para a origem
        public bool FormaDobradinha { get; set; } // Indica se é uma troca que forma dobradinha
    }
}
