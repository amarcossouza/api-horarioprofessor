using projeto_escola_2024_Matriz.Models;
using System.Collections.Generic;

namespace projeto_escola_2024_Matriz.Services
{
    public interface IMatrizesService
    {
        MatrizesHorariosDto ObterMatrizes();
        List<List<string>> ConverterParaListas(string[,] matriz);
    }
}
