namespace API_HorarioProfessor.Models
{
    public class MovimentacaoProfessorFechamento
    {
        // Informações do professor inserido
        public string ProfessorInseridoTurma { get; set; } // Ex: "9X"
        public string ProfessorInserido { get; set; } // Ex: "Margareth"
        public string MatrizProfessorInserido { get; set; } // Ex: "terça-feira"
        public int MatrizProfessorInseridoLinha { get; set; } // Ex: 3

        // Informações do professor de troca
        public string ProfessorTrocaTurma { get; set; }
        public string ProfessorTroca { get; set; } // Ex: "Uissa"
        public string MatrizProfessorTroca { get; set; } // Ex: "segunda-feira"
        public int MatrizProfessorTrocaLinha { get; set; } // Ex: 0

        // Informações do professor movimentado (destino)
        public string ProfessorDestinoTurma { get; set; } // Ex: "8Y"
        public string ProfessorDestino { get; set; } // Ex: "Uissa"
        public string MatrizProfessorDestino { get; set; } // Ex: "terça-feira"
        public int MatrizProfessorDestinoLinha { get; set; } // Ex: 4

        // Informações do professor movimentado (origem)
        public string ProfessorOrigemTurma { get; set; }
        public string ProfessorOrigem { get; set; } // Ex: "Zilrene"
        public string MatrizProfessorOrigem { get; set; } // Ex: "segunda-feira"
        public int MatrizProfessorOrigemLinha { get; set; } // Ex: 0
    }
}
