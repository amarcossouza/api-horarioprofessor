using System;
using System.Collections.Generic;

public class OpcaoHorario
{
    public int Linha { get; set; }
    public int Coluna { get; set; }
    public string Abreviacao { get; set; }
    public string Tipo { get; set; }
    public string Sigla { get; set; } // Nova propriedade para a sigla
    public int IdProfessor { get; set; } // Nova propriedade para o IdProfessor
}

public class OpcoesProfessor
{
    public int Id { get; set; }
    public Dictionary<string, OpcaoHorario> Opcoes1 { get; set; }
    public Dictionary<string, OpcaoHorario> Opcoes2 { get; set; }

    public OpcoesProfessor()
    {
        Opcoes1 = new Dictionary<string, OpcaoHorario>();
        Opcoes2 = new Dictionary<string, OpcaoHorario>();
    }

    public void AdicionarOpcao(int idProfessor, int opcaoSet, int linha, int coluna, string tipo)
    {
        string abreviacao = ObterAbreviacao(coluna, linha);
        string sigla = ObterSigla(tipo); // Método para obter a sigla
        var opcao = new OpcaoHorario
        {
            Linha = linha,
            Coluna = coluna,
            Abreviacao = abreviacao,
            Tipo = tipo,
            Sigla = sigla,
            IdProfessor = idProfessor
        };

        if (opcaoSet == 1)
        {
            Opcoes1[abreviacao] = opcao;
        }
        else if (opcaoSet == 2)
        {
            Opcoes2[abreviacao] = opcao;
        }
        else
        {
            throw new ArgumentException("O valor de opcaoSet deve ser 1 ou 2.");
        }
    }

    public OpcaoHorario ObterOpcao(int opcaoSet, string abreviacao)
    {
        if (opcaoSet == 1 && Opcoes1.ContainsKey(abreviacao))
        {
            return Opcoes1[abreviacao];
        }
        else if (opcaoSet == 2 && Opcoes2.ContainsKey(abreviacao))
        {
            return Opcoes2[abreviacao];
        }

        return null;
    }

    private string ObterAbreviacao(int coluna, int linha)
    {
        string[] diasSemana = { "SEG", "TER", "QUA", "QUI", "SEX" };
        return diasSemana[coluna - 1] + linha;
    }

    private string ObterSigla(string tipo)
    {
        return tipo switch
        {
            "Acumulo Cargo" => "AC",
            "Bloqueio" => "BL",
            "ATPC" => "ATPC",
            "Hora Livre" => "HL",
            _ => "ND" // Não Definido
        };
    }
}

////public class Program
////{
////    public static void Main()
////    {
////        var professorOpcoes = new OpcoesProfessor();
////        professorOpcoes.AdicionarOpcao(1, 1, 1, 1, "Acumulo Cargo");
////        professorOpcoes.AdicionarOpcao(1, 1, 3, 2, "Bloqueio");
////        professorOpcoes.AdicionarOpcao(1, 1, 2, 4, "Acumulo Cargo");
////        professorOpcoes.AdicionarOpcao(1, 2, 5, 5, "ATPC");
////        professorOpcoes.AdicionarOpcao(1, 2, 1, 1, "Hora Livre");

////        // Exibindo as opções armazenadas
////        ExibirOpcoes(professorOpcoes.Opcoes1);
////        ExibirOpcoes(professorOpcoes.Opcoes2);
////    }

////    private static void ExibirOpcoes(Dictionary<string, OpcaoHorario> opcoes)
////    {
////        foreach (var opcao in opcoes.Values)
////        {
////            Console.WriteLine($"IdProfessor: {opcao.IdProfessor}, Linha: {opcao.Linha}, Coluna: {opcao.Coluna}, Abreviação: {opcao.Abreviacao}, Tipo: {opcao.Tipo}, Sigla: {opcao.Sigla}");
////        }
////    }
//}
