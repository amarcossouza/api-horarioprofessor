namespace API_HorarioProfessor.Models
{
    public class ProfessorInsercaoPossibilidade
    {
        //public string MatrizOrigem { get; set; } // Nome da matriz onde o professor será deslocado (se necessário)
        //public int LinhaOrigem { get; set; } // Linha de onde o professor será deslocado
        //public int ColunaOrigem { get; set; } // Coluna de onde o professor será deslocado
        //public string ProfessorDeslocado { get; set; } // Nome do professor que será deslocado (ex.: Anderson)

        //public string MatrizDestino { get; set; } // Nome da matriz onde o professor faltante será inserido
        //public int LinhaDestino { get; set; } // Linha onde o professor faltante será inserido
        //public int ColunaDestino { get; set; } // Coluna onde o professor faltante será inserido
        //public string ProfessorInserido { get; set; } // Nome do professor que será inserido (ex.: Luis)
        //public int LinhaProfessorInserido { get; set; }
        //public int ColunaProfessorInserido { get; set; }

        //public bool InsercaoValida { get; set; } // Indica se a inserção foi validada
        //public bool RealocacaoValida { get; set; } // Indica se a realocação do professor foi validada
        //public bool FormaDobradinha { get; set; } // Indica se a realocação resulta em uma dobradinha válida

        //// **Novas Propriedades**
        //public bool TrocaOrigemParaDestinoValida { get; set; } // Valida troca na origem para o destino
        //public bool TrocaDestinoParaOrigemValida { get; set; } // Valida troca no destino para a origem



        // **Origem: Local onde o professor será deslocado**
        public string MatrizOrigem { get; set; } // Nome da matriz de origem (ex.: "Quinta-feira")
        public int LinhaOrigem { get; set; } // Linha de origem (ex.: 0)
        public int ColunaOrigem { get; set; } // Coluna de origem (ex.: 6)
        public string ProfessorOrigem { get; set; } // Nome do professor na origem (ex.: "Anderson")
        public string TurmaOrigem { get; set; } // Nome da turma na coluna de origem (ex.: "7A")

        // **Destino: Local para onde o professor será realocado**
        public string MatrizDestino { get; set; } // Nome da matriz de destino (ex.: "Segunda-feira")
        public int LinhaDestino { get; set; } // Linha de destino (ex.: 2)
        public int ColunaDestino { get; set; } // Coluna de destino (ex.: 6)
        public string ProfessorDestino { get; set; } // Nome do professor atual no destino (se houver)
        public string TurmaDestino { get; set; } // Nome da turma na coluna de destino (ex.: "7B")

        // **Professor Inserido: Novo professor que será colocado no destino**
        public string ProfessorInserido { get; set; } // Nome do professor inserido (ex.: "Luis")
        public int LinhaProfessorInserido { get; set; } // Linha onde o professor será inserido (ex.: 2)
        public int ColunaProfessorInserido { get; set; } // Coluna onde o professor será inserido (ex.: 6)
        public string TurmaProfessorInserido { get; set; } // Nome da turma onde o professor será inserido (ex.: "7C")

        // **Validações e Regras**
        public bool InsercaoValida { get; set; } // Indica se a inserção no destino é válida
        public bool RealocacaoValida { get; set; } // Indica se a realocação da origem é válida
        public bool FormaDobradinha { get; set; } // Indica se a realocação resulta em dobradinha válida

        // **Trocas**
        public bool TrocaOrigemParaDestinoValida { get; set; } // Indica se a troca origem → destino é válida
        public bool TrocaDestinoParaOrigemValida { get; set; } // Indica se a troca destino → origem é válida

        // **Depuração e Acompanhamento**
        public string EtapaProcessamento { get; set; } // Etapa do processamento (ex.: "Horizontal", "Vertical")

  

    }


}
