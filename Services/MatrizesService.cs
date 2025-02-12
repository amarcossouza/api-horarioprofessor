using projeto_escola_2024_Matriz.Models;
using System.Collections.Generic;

namespace projeto_escola_2024_Matriz.Services
{
    public class MatrizesService : IMatrizesService
    {
        //INICIAR AS MATRIZES
        public string[,] MatrizHorariosSegunda { get; set; }
        public string[,] MatrizHorariosTerca { get; set; }
        public string[,] MatrizHorariosQuarta { get; set; }
        public string[,] MatrizHorariosQuinta { get; set; }
        public string[,] MatrizHorariosSexta { get; set; }

        public List<List<string>> ConverterParaListas(string[,] matriz)
        {
            throw new NotImplementedException();
        }

        public MatrizesHorariosDto ObterMatrizes()
        {
            // Inicialize as matrizes com os valores desejados

            // Inicializa as matrizes com valores de exemplo
            MatrizHorariosSegunda = InicializarMatriz(6, 4);
            MatrizHorariosTerca = InicializarMatriz(6, 4);
            MatrizHorariosQuarta = InicializarMatriz(6, 4);
            MatrizHorariosQuinta = InicializarMatriz(6, 4);
            MatrizHorariosSexta = InicializarMatriz(6, 4);

            string[][,] matrizes = { MatrizHorariosSegunda, MatrizHorariosTerca, MatrizHorariosQuarta, MatrizHorariosQuinta, MatrizHorariosSexta };

            matrizes = new Horarios().ObterTodasMatrizesPreenchidas(matrizes);

            // Preencher matrizes com dados de exemplo
            PreencherMatrizComDados(MatrizHorariosSegunda);
            PreencherMatrizComDados(MatrizHorariosTerca);
            PreencherMatrizComDados(MatrizHorariosQuarta);
            PreencherMatrizComDados(MatrizHorariosQuinta);
            PreencherMatrizComDados(MatrizHorariosSexta);

            return new MatrizesHorariosDto
            {
                MatrizHorariosSegunda = MatrizHorariosSegunda,
                MatrizHorariosTerca = MatrizHorariosTerca,
                MatrizHorariosQuarta = MatrizHorariosQuarta,
                MatrizHorariosQuinta = MatrizHorariosQuinta,
                MatrizHorariosSexta = MatrizHorariosSexta
            };
        }

        private string[,] InicializarMatriz(int linhas, int colunas)
        {
            string[,] matriz = new string[linhas, colunas];
            for (int i = 0; i < linhas; i++)
            {
                for (int j = 0; j < colunas; j++)
                {
                    matriz[i, j] = "";
                }
            }
            return matriz;
        }

        private void PreencherMatrizComDados(string[,] matriz)
        {
            // Exemplo de preenchimento da matriz com dados
            for (int i = 0; i < matriz.GetLength(0); i++)
            {
                for (int j = 0; j < matriz.GetLength(1); j++)
                {
                    matriz[i, j] = $"Aula {i + 1}-{j + 1}"; // Exemplo de preenchimento com dados
                }
            }
        }
    }

    public class Horarios
    {
        public string[][,] ObterTodasMatrizesPreenchidas(string[][,] matrizes)
        {
            string[][,] todasMatrizes = new string[5][,];

            todasMatrizes[0] = PreencherMatriz(matrizes[0]);
            todasMatrizes[1] = PreencherMatriz(matrizes[1]);
            todasMatrizes[2] = PreencherMatriz(matrizes[2]);
            todasMatrizes[3] = PreencherMatriz(matrizes[3]);
            todasMatrizes[4] = PreencherMatriz(matrizes[4]);

            return todasMatrizes;
        }

        private string[,] PreencherMatriz(string[,] dados)
        {
            int linhas = dados.GetLength(0);
            int colunas = dados.GetLength(1);
            string[,] matriz = new string[linhas, colunas];

            for (int i = 0; i < linhas; i++)
            {
                for (int j = 0; j < colunas; j++)
                {
                    matriz[i, j] = dados[i, j];
                }
            }

            return matriz;
        }
    }
}




    ////public class Horarios
    ////{    
    ////        public string[,] MatrizHorariosSegunda { get; private set; }
    ////        public string[,] MatrizHorariosTerca { get; private set; }
    ////        public string[,] MatrizHorariosQuarta { get; private set; }
    ////        public string[,] MatrizHorariosQuinta { get; private set; }
    ////        public string[,] MatrizHorariosSexta { get; private set; }

    ////        public string[][,] matrizes Horarios(string[][,] matrizes)
    ////        {
    ////            MatrizHorariosSegunda = matrizes[0];
    ////            MatrizHorariosTerca = matrizes[1];
    ////            MatrizHorariosQuarta = matrizes[2];
    ////            MatrizHorariosQuinta = matrizes[3];
    ////            MatrizHorariosSexta = matrizes[4];

    ////            //return matrizes;
    ////        }

    ////        public List<string[,]> ObterTodasMatrizesPreenchidas()
    ////        {
    ////            List<string[,]> todasMatrizes = new List<string[,]>
    ////        {
    ////            PreencherMatriz(MatrizHorariosSegunda),
    ////            PreencherMatriz(MatrizHorariosTerca),
    ////            PreencherMatriz(MatrizHorariosQuarta),
    ////            PreencherMatriz(MatrizHorariosQuinta),
    ////            PreencherMatriz(MatrizHorariosSexta)
    ////        };

    ////            return todasMatrizes;
    ////        }

    ////        private string[,] PreencherMatriz(string[,] dados)
    ////        {
    ////            int linhas = dados.GetLength(0);
    ////            int colunas = dados.GetLength(1);
    ////            string[,] matriz = new string[linhas, colunas];

    ////            for (int i = 0; i < linhas; i++)
    ////            {
    ////                for (int j = 0; j < colunas; j++)
    ////                {
    ////                    matriz[i, j] = dados[i, j];
    ////                }
    ////            }

    ////            return matriz;
    ////        }


    ////        // Exemplo de uso
    ////        //public class ExemploDeUso
    ////        //{
    ////        public void UsarHorarios()
    ////        {
    ////            string[][,] matrizes;
    ////            // Dados para segunda-feira
    ////            string[,] dadosSegunda = new string[,]
    ////            {
    ////            { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    ////            { "ELENICE", "JOAO", "LUIS", "ANDRE", "IVAN", "RAQUEL", "LARISSA", "ZILDA", "JULIA", "DIOGO", "ROBERTO", "SAMIR" },
    ////            { "ALESSANDRA", "ANDRE", "ROBERTO", "LUIS", "ELENICE", "DENISE", "VITOR", "LARISSA", "JULIA", "MARISTELA", "SAMIR", "ZILDA" },
    ////            { "ANDRE", "IVAN", "DENISE", "LARISSA", "LUIS", "JULIA", "KATIA", "RAQUEL", "PAULO", "ZILDA", "MARISTELA", "SAMIR" },
    ////            { "DENISE", "ELENICE", "IVAN", "ANDRE", "LUIS", "JULIA", "ZILDA", "SAMIR", "RAQUEL", "DIOGO", "MARISTELA", "VITOR" },
    ////            { "DENISE", "KATIA", "ANDRE", "LUIS", "IVAN", "ELENICE", "PAULO", "VITOR", "LARISSA", "ZILDA", "SAMIR", "MARISTELA" },
    ////            { "IVAN", "LUIS", "ANDRE", "ELENICE", "DENISE", "KATIA", "PAULO", "ROBERTO", "VITOR", "SAMIR", "DIOGO", "MARISTELA" }
    ////            };


    ////            // Dados para terça-feira
    ////            string[,] dadosTerca = new string[,]
    ////             {
    ////            { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    ////            { "ANDRE", "DENISE", "DIOGO", "ELENICE", "LARISSA", "ALESSANDRA", "PAULO", "VITOR", "RAQUEL", "MARISTELA", "ZILDA", "SAMIR" },
    ////            { "ANDRE", "DENISE", "DIOGO", "IVAN", "LUIS", "ELENICE", "SAMIR", "ZILDA", "LARISSA", "KATIA", "MARISTELA", "PAULO" },
    ////            { "DENISE", "ANDRE", "ELENICE", "IVAN", "LARISSA", "JULIA", "RAQUEL", "KATIA", "VITOR", "MARISTELA", "DIOGO", "SAMIR" },
    ////            { "DENISE", "LUIS", "ELENICE", "KATIA", "ANDRE", "DIOGO", "RAQUEL", "SAMIR", "ALESSANDRA", "PAULO", "LARISSA", "ZILDA" },
    ////            { "LUIS", "JOAO", "ANDRE", "DENISE", "IVAN", "ELENICE", "ZILDA", "PAULO", "KATIA", "LARISSA", "SAMIR", "DIOGO" },
    ////            { "LUIS", "IVAN", "ANDRE", "DENISE", "KATIA", "DIOGO", "SAMIR", "PAULO", "LARISSA", "VITOR", "ZILDA", "MARISTELA" }
    ////             };

    ////            // Dados para quarta-feira
    ////            string[,] dadosQuarta = new string[,]
    ////            {
    ////            { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    ////            { "ELENICE", "LUIS", "IVAN", "ANDRE", "DENISE", "ROBERTO", "DIOGO", "PAULO", "RAQUEL", "KATIA", "VITOR", "SAMIR" },
    ////            { "LUIS", "IVAN", "DENISE", "ANDRE", "LARISSA", "KATIA", "DIOGO", "PAULO", "RAQUEL", "ALESSANDRA", "SAMIR", "VITOR" },
    ////            { "IVAN", "ELENICE", "LUIS", "KATIA", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "ZILDA", "MARISTELA", "ALESSANDRA", "LARISSA" },
    ////            { "IVAN", "ELENICE", "LUIS", "DENISE", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "ZILDA", "VITOR", "PAULO", "KATIA" },
    ////            { "JOAO", "IVAN", "KATIA", "LUIS", "ROBERTO", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "PAULO", "LARISSA", "ZILDA" },
    ////            { "JOAO", "LUIS", "IVAN", "ROBERTO", "DENISE", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "PAULO", "VITOR", "KATIA" }
    ////            };

    ////            // Dados para quinta-feira
    ////            string[,] dadosQuinta = new string[,]
    ////            {
    ////            { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    ////            { "ROBERTO", "ALESSANDRA", "JOAO", "DENISE", "DIOGO", "ANDRE", "SAMIR", "RAQUEL", "JULIA", "LARISSA", "PAULO", "VITOR" },
    ////            { "KATIA", "ROBERTO", "IVAN", "ALESSANDRA", "DENISE", "JULIA", "RAQUEL", "SAMIR", "ZILDA", "LARISSA", "PAULO", "DIOGO" },
    ////            { "LUIS", "DENISE", "IVAN", "LARISSA", "ELENICE", "JOAO", "ZILDA", "RAQUEL", "PAULO", "VITOR", "KATIA", "ALESSANDRA" },
    ////            { "DIOGO", "KATIA", "ALESSANDRA", "IVAN", "LUIS", "DENISE", "VITOR", "LARISSA", "ZILDA", "ROBERTO", "SAMIR", "PAULO" },
    ////            { "ELENICE", "IVAN", "LUIS", "DIOGO", "KATIA", "DENISE", "LARISSA", "ZILDA", "ROBERTO", "SAMIR", "MARISTELA", "PAULO" },
    ////            { "JOAO", "DIOGO", "LUIS", "IVAN", "ELENICE", "DENISE", "ROBERTO", "ZILDA", "JULIA", "SAMIR", "VITOR", "MARISTELA" }
    ////            };


    ////            // Dados para sexta-feira
    ////            string[,] dadosSexta = new string[,]
    ////             {
    ////            { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    ////            { "JOAO", "LUIS", "ANDRE", "IVAN", "ELENICE", "KATIA", "PAULO", "DENISE", "RAQUEL", "SAMIR", "DIOGO", "MARISTELA" },
    ////            { "JOAO", "LUIS", "ANDRE", "IVAN", "DENISE", "ELENICE", "RAQUEL", "PAULO", "SAMIR", "VITOR", "KATIA", "JULIA" },
    ////            { "JOAO", "LUIS", "DENISE", "IVAN", "ELENICE", "ANDRE", "PAULO", "KATIA", "RAQUEL", "DIOGO", "VITOR", "SAMIR" },
    ////            { "JOAO", "ANDRE", "LUIS", "DENISE", "ELENICE", "IVAN", "KATIA", "RAQUEL", "PAULO", "SAMIR", "VITOR", "DIOGO" },
    ////            { "ANDRE", "DENISE", "IVAN", "ELENICE", "LUIS", "JOAO", "PAULO", "RAQUEL", "KATIA", "SAMIR", "VITOR", "DIOGO" },
    ////            { "ANDRE", "DENISE", "IVAN", "ELENICE", "LUIS", "JOAO", "PAULO", "RAQUEL", "KATIA", "SAMIR", "VITOR", "DIOGO" }
    ////            };

    ////            Horarios horarios = new Horarios(dadosSegunda, dadosTerca, dadosQuarta, dadosQuinta, dadosSexta);

    ////            List<string[,]> todasMatrizesPreenchidas = horarios.ObterTodasMatrizesPreenchidas();

    ////            // Exemplo de como acessar as matrizes preenchidas
    ////            foreach (var matriz in todasMatrizesPreenchidas)
    ////            {
    ////                // Implementação para usar as matrizes preenchidas
    ////            }
    ////            //}
    ////        }


    ////    }
    ////}




    //public class Horarios
    //{
    //    public string[,] MatrizHorariosSegunda { get; private set; }
    //    public string[,] MatrizHorariosTerca { get; private set; }
    //    public string[,] MatrizHorariosQuarta { get; private set; }
    //    public string[,] MatrizHorariosQuinta { get; private set; }
    //    public string[,] MatrizHorariosSexta { get; private set; }

    //    public Horarios(string[,] dadosSegunda, string[,] dadosTerca, string[,] dadosQuarta, string[,] dadosQuinta, string[,] dadosSexta)
    //    {
    //        MatrizHorariosSegunda = PreencherMatriz(dadosSegunda);
    //        MatrizHorariosTerca = PreencherMatriz(dadosTerca);
    //        MatrizHorariosQuarta = PreencherMatriz(dadosQuarta);
    //        MatrizHorariosQuinta = PreencherMatriz(dadosQuinta);
    //        MatrizHorariosSexta = PreencherMatriz(dadosSexta);
    //    }

    //    private string[,] PreencherMatriz(string[,] dados)
    //    {
    //        int linhas = dados.GetLength(0);
    //        int colunas = dados.GetLength(1);
    //        string[,] matriz = new string[linhas, colunas];

    //        for (int i = 0; i < linhas; i++)
    //        {
    //            for (int j = 0; j < colunas; j++)
    //            {
    //                matriz[i, j] = dados[i, j];
    //            }
    //        }

    //        return matriz;
    //    }



    //    public static void Main()
    //    {
    //        string[,] dadosSegunda = new string[,]
    //        {
    //        { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    //        { "ELENICE", "JOAO", "LUIS", "ANDRE", "IVAN", "RAQUEL", "LARISSA", "ZILDA", "JULIA", "DIOGO", "ROBERTO", "SAMIR" },
    //        { "ALESSANDRA", "ANDRE", "ROBERTO", "LUIS", "ELENICE", "DENISE", "VITOR", "LARISSA", "JULIA", "MARISTELA", "SAMIR", "ZILDA" },
    //        { "ANDRE", "IVAN", "DENISE", "LARISSA", "LUIS", "JULIA", "KATIA", "RAQUEL", "PAULO", "ZILDA", "MARISTELA", "SAMIR" },
    //        { "DENISE", "ELENICE", "IVAN", "ANDRE", "LUIS", "JULIA", "ZILDA", "SAMIR", "RAQUEL", "DIOGO", "MARISTELA", "VITOR" },
    //        { "DENISE", "KATIA", "ANDRE", "LUIS", "IVAN", "ELENICE", "PAULO", "VITOR", "LARISSA", "ZILDA", "SAMIR", "MARISTELA" },
    //        { "IVAN", "LUIS", "ANDRE", "ELENICE", "DENISE", "KATIA", "PAULO", "ROBERTO", "VITOR", "SAMIR", "DIOGO", "MARISTELA" }
    //        };

    //        string[,] dadosTerca = new string[,]
    //        {
    //        { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    //        { "ANDRE", "DENISE", "DIOGO", "ELENICE", "LARISSA", "ALESSANDRA", "PAULO", "VITOR", "RAQUEL", "MARISTELA", "ZILDA", "SAMIR" },
    //        { "ANDRE", "DENISE", "DIOGO", "IVAN", "LUIS", "ELENICE", "SAMIR", "ZILDA", "LARISSA", "KATIA", "MARISTELA", "PAULO" },
    //        { "DENISE", "ANDRE", "ELENICE", "IVAN", "LARISSA", "JULIA", "RAQUEL", "KATIA", "VITOR", "MARISTELA", "DIOGO", "SAMIR" },
    //        { "DENISE", "LUIS", "ELENICE", "KATIA", "ANDRE", "DIOGO", "RAQUEL", "SAMIR", "ALESSANDRA", "PAULO", "LARISSA", "ZILDA" },
    //        { "LUIS", "JOAO", "ANDRE", "DENISE", "IVAN", "ELENICE", "ZILDA", "PAULO", "KATIA", "LARISSA", "SAMIR", "DIOGO" },
    //        { "LUIS", "IVAN", "ANDRE", "DENISE", "KATIA", "DIOGO", "SAMIR", "PAULO", "LARISSA", "VITOR", "ZILDA", "MARISTELA" }
    //        };

    //        string[,] dadosQuarta = new string[,]
    //        {
    //        { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    //        { "ELENICE", "LUIS", "IVAN", "ANDRE", "DENISE", "ROBERTO", "DIOGO", "PAULO", "RAQUEL", "KATIA", "VITOR", "SAMIR" },
    //        { "LUIS", "IVAN", "DENISE", "ANDRE", "LARISSA", "KATIA", "DIOGO", "PAULO", "RAQUEL", "ALESSANDRA", "SAMIR", "VITOR" },
    //        { "IVAN", "ELENICE", "LUIS", "KATIA", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "ZILDA", "MARISTELA", "ALESSANDRA", "LARISSA" },
    //        { "IVAN", "ELENICE", "LUIS", "DENISE", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "ZILDA", "VITOR", "PAULO", "KATIA" },
    //        { "JOAO", "IVAN", "KATIA", "LUIS", "ROBERTO", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "PAULO", "LARISSA", "ZILDA" },
    //        { "JOAO", "LUIS", "IVAN", "ROBERTO", "DENISE", "ANDRE", "RAQUEL", "SAMIR", "DIOGO", "PAULO", "VITOR", "KATIA" }
    //        };

    //        string[,] dadosQuinta = new string[,]
    //        {
    //        { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    //        { "ROBERTO", "ALESSANDRA", "JOAO", "DENISE", "DIOGO", "ANDRE", "SAMIR", "RAQUEL", "JULIA", "LARISSA", "PAULO", "VITOR" },
    //        { "KATIA", "ROBERTO", "IVAN", "ALESSANDRA", "DENISE", "JULIA", "RAQUEL", "SAMIR", "ZILDA", "LARISSA", "PAULO", "DIOGO" },
    //        { "LUIS", "DENISE", "IVAN", "LARISSA", "ELENICE", "JOAO", "ZILDA", "RAQUEL", "PAULO", "VITOR", "KATIA", "ALESSANDRA" },
    //        { "DIOGO", "KATIA", "ALESSANDRA", "IVAN", "LUIS", "DENISE", "VITOR", "LARISSA", "ZILDA", "ROBERTO", "SAMIR", "PAULO" },
    //        { "ELENICE", "IVAN", "LUIS", "DIOGO", "KATIA", "DENISE", "LARISSA", "ZILDA", "ROBERTO", "SAMIR", "MARISTELA", "PAULO" },
    //        { "JOAO", "DIOGO", "LUIS", "IVAN", "ELENICE", "DENISE", "ROBERTO", "ZILDA", "JULIA", "SAMIR", "VITOR", "MARISTELA" }
    //        };

    //        string[,] dadosSexta = new string[,]
    //        {
    //        { "6W", "6X", "6Y", "7W", "7X", "7Y", "8W", "8X", "8Y", "9W", "9X", "9Y" },
    //        { "JOAO", "LUIS", "ANDRE", "IVAN", "ELENICE", "KATIA", "PAULO", "DENISE", "RAQUEL", "SAMIR", "DIOGO", "MARISTELA" },
    //        { "JOAO", "LUIS", "ANDRE", "IVAN", "DENISE", "ELENICE", "RAQUEL", "PAULO", "SAMIR", "VITOR", "KATIA", "JULIA" },
    //        { "JOAO", "LUIS", "DENISE", "IVAN", "ELENICE", "ANDRE", "PAULO", "KATIA", "RAQUEL", "DIOGO", "VITOR", "SAMIR" },
    //        { "JOAO", "ANDRE", "LUIS", "DENISE", "ELENICE", "IVAN", "KATIA", "RAQUEL", "PAULO", "SAMIR", "VITOR", "DIOGO" },
    //        { "ANDRE", "DENISE", "IVAN", "ELENICE", "LUIS", "JOAO", "PAULO", "RAQUEL", "KATIA", "SAMIR", "VITOR", "DIOGO" },
    //        { "ANDRE", "DENISE", "IVAN", "ELENICE", "LUIS", "JOAO", "PAULO", "RAQUEL", "KATIA", "SAMIR", "VITOR", "DIOGO" }
    //        };

    //        Horarios horarios = new Horarios(dadosSegunda, dadosTerca, dadosQuarta, dadosQuinta, dadosSexta);

    //        ImprimirHorarios("Segunda", horarios.MatrizHorariosSegunda);
    //        ImprimirHorarios("Terça", horarios.MatrizHorariosTerca);
    //        ImprimirHorarios("Quarta", horarios.MatrizHorariosQuarta);
    //        ImprimirHorarios("Quinta", horarios.MatrizHorariosQuinta);
    //        ImprimirHorarios("Sexta", horarios.MatrizHorariosSexta);
    //    }

    //    public static void ImprimirHorarios(string dia, string[,] matriz)
    //    {
    //        Console.WriteLine($"Horários de {dia}:");

    //        for (int i = 0; i < matriz.GetLength(0); i++)
    //        {
    //            for (int j = 0; j < matriz.GetLength(1); j++)
    //            {
    //                Console.Write(matriz[i, j] + "\t");
    //            }
    //            Console.WriteLine();
    //        }

    //        Console.WriteLine();
    //    }

    //}


//}