using API_HorarioProfessor.Controllers;
using API_HorarioProfessor.Services.CoreServices;
using System.Collections.Generic;

namespace API_HorarioProfessor.Models
{
    public class ProfessoresNaoDistribuidos
    {
        public string Professor_Turma { get; set; } = string.Empty;
        public string Professor { get; set; } = string.Empty;
        public string Turma { get; set; } = string.Empty;
        public int QtdAula { get; set; }
        public int DiaDaSemana { get; set; } // 1 = Segunda-feira, 2 = Terça-feira, etc.
        public int Horario { get; set; } // Horário em que a aula deve ser agendada
        public int AulasRestantes { get; set; }

        public string ListaProfessorAtribuidoTurma { get; set; } = string.Empty;

   
        // Lista de professores não distribuídos
        public List<ProfessoresNaoDistribuidos> Professor_Turma_NaoDistribuidosNaLinha { get; set; } = new List<ProfessoresNaoDistribuidos>();

        // Nova propriedade para armazenar as informações da pesquisa por linha
        //public List<ListaProfessoresSim_NaoAtribuidosLinha> VerificaProfessor_Atribuido_NaLinha { get; set; } = new List<ListaProfessoresSim_NaoAtribuidosLinha>();

        public List<ListaProfessoresSim_NaoAtribuidosLinha> VerificaProfessor_NAO_AtribuidoNaLinha { get; set; } = new List<ListaProfessoresSim_NaoAtribuidosLinha>();




        // Construtor principal
        public ProfessoresNaoDistribuidos(string professor_Turma, string professor, string turma, int qtdAula, int diaDaSemana, int horario, int aulasRestantes, string listaProfessorAtribuidoTurma)
        {
            Professor_Turma = professor_Turma;
            Professor = professor;
            Turma = turma;
            QtdAula = qtdAula;
            DiaDaSemana = diaDaSemana;
            Horario = horario;
            AulasRestantes = aulasRestantes;
            ListaProfessorAtribuidoTurma = listaProfessorAtribuidoTurma;
            
        }

        // Construtor vazio para inicializar a classe sem parâmetros
        public ProfessoresNaoDistribuidos()
        {
        }

        // Método para adicionar um professor à lista
        public void AdicionarProfessor(string professor_Turma, string professor, string turma, int qtdAula, int diaDaSemana, int horario, int aulasRestantes, string listaProfessorAtribuidoTurma)
        {
            var professorNaoDistribuido = new ProfessoresNaoDistribuidos(professor_Turma, professor, turma, qtdAula, diaDaSemana, horario, aulasRestantes, listaProfessorAtribuidoTurma);
            Professor_Turma_NaoDistribuidosNaLinha.Add(professorNaoDistribuido);
        }

        // Método para obter todos os professores não distribuídos
        public List<ProfessoresNaoDistribuidos> ObterTodos()
        {
            return Professor_Turma_NaoDistribuidosNaLinha;
        }
    }
}

// Classe auxiliar para armazenar as informações por linha
public class ListaProfessoresSim_NaoAtribuidosLinha
{
    public string DiaSemana { get; set; } // Ex: "Segunda-feira"
    public int Linha { get; set; } // Linha pesquisada
    public string NaoProfessorLinha { get; set; } // Professores Nao encontrados na linha
    public string SimProfessorLinha { get; set; } // Professores sim encontrados na linha

    public List<ProfessorComOpcoes> OpcoesNaoProfessoresLinha { get; set; } = new List<ProfessorComOpcoes>(); // Detalhes das opções dos professores não encontrados
}