using API_HorarioProfessor.Controllers;
using API_HorarioProfessor.Data;
using API_HorarioProfessor.DTO;
using API_HorarioProfessor.Services.ServicesCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_HorarioProfessor.Services.CoreServices.MetodosAuxiliares
{
    public class HorarioDistribuicaoService
    {
        private readonly HorarioService _horarioService;

        public HorarioDistribuicaoService(HorarioService horarioService)
        {
            _horarioService = horarioService;
        }

        // Método original para salvar os horários
        public async Task<IActionResult> SalvarHorarios(string[][,] matrizes, int idEscola, string periodo, int anoLetivo)
        {
            var result = await _horarioService.SalvarHorarios(matrizes, idEscola, periodo, anoLetivo);

            if (result)
                return new OkObjectResult(new { success = true, message = "Horários salvos com sucesso!" });

            return new BadRequestObjectResult(new { success = false, message = "Erro ao salvar horários." });
        }

        // Método para salvar as matrizes como JSON
        public async Task<IActionResult> SalvarMatrizComoJson(string[][,] matrizes,List<string> turmas, int idEscola, string periodo, int anoLetivo)
        {
            var result = await _horarioService.SalvarMatrizComoJson(matrizes, turmas,  idEscola, periodo, anoLetivo);

            if (result)
                return new OkObjectResult(new { success = true, message = "Matriz salva como JSON com sucesso!" });

            return new BadRequestObjectResult(new { success = false, message = "Erro ao salvar a matriz como JSON." });
        }

        // Método para recuperar as matrizes salvas como JSON
        public async Task<IActionResult> RecuperarMatrizComoJson(int idEscola, string periodo, int anoLetivo)
        {
            var (matriz, turmas) = await _horarioService.RecuperarMatrizComoJson(idEscola, periodo, anoLetivo);

            // Se não houver dados, retorna um array vazio
            if (matriz != null)
                return new OkObjectResult(new { success = true, matriz });

            return new OkObjectResult(new { success = true, matriz = new string[0][,] }); // Array vazio
        }

        public bool VerificarMatrizesPreenchidas(string[][,] matrizes)
        {
            // Percorre cada matriz no array de matrizes
            foreach (var matriz in matrizes)
            {
                // Percorre cada célula da matriz
                for (int linha = 0; linha < matriz.GetLength(0); linha++)
                {
                    for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                    {
                        // Verifica se a célula está vazia ("")
                        if (matriz[linha, coluna] == "")
                        {
                            return false; // Retorna falso assim que encontrar uma célula vazia
                        }
                    }
                }
            }

            return true; // Retorna verdadeiro se não encontrar nenhuma célula vazia
        }



    }

}
