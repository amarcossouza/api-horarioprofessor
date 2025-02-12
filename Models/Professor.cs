using API_HorarioProfessor.Services.CoreServices.Classes;
using System.Collections.Generic;

namespace API_HorarioProfessor.Models
{
    public class Professor
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int AulasPorTurma { get; set; }
        public Dictionary<string, List<string>> DistribuicaoTurmas { get; set; }
        public string DiasNaoTrabalha { get; set; }
        public string NomeAtividadeDescolar { get; set; }
        public bool DobrarAula { get; set; }
        public string Restricoes { get; set; }
        public string AtividadeAtpc { get; set; }
        public string DiasAtpc { get; set; }
        public bool Prioridade { get; set; }
        public OpcoesProfessor OpcoesProfessor { get; set; } // Adicionando OpcoesProfessor aqui

        // Construtor com todos os parâmetros
        public Professor(int id, string nome, int aulasPorTurma, Dictionary<string, List<string>> distribuicaoTurmas, string diasNaoTrabalha, string nomeAtividadeDescolar, bool dobrarAula, string restricoes, string atividadeAtpc, string diasAtpc, bool prioridade, OpcoesProfessor opcoesProfessor)
        {
            Id = id;
            Nome = nome;
            AulasPorTurma = aulasPorTurma;
            DistribuicaoTurmas = distribuicaoTurmas;
            DiasNaoTrabalha = diasNaoTrabalha;
            NomeAtividadeDescolar = nomeAtividadeDescolar;
            DobrarAula = dobrarAula;
            Restricoes = restricoes;
            AtividadeAtpc = atividadeAtpc;
            DiasAtpc = diasAtpc;
            Prioridade = prioridade;
            OpcoesProfessor = opcoesProfessor;  // Associando as opções do professor aqui
        }
    }
}
