using Microsoft.AspNetCore.Mvc;
using projeto_escola_2024_Matriz.Models;
using projeto_escola_2024_Matriz.Services;
using System.Collections.Generic;

namespace projeto_escola_2024_Matriz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatrizesController : ControllerBase
    {
        private readonly IMatrizesService _matrizesService;

        public MatrizesController(IMatrizesService matrizesService)
        {
            _matrizesService = matrizesService;
        }

        [HttpGet]
        public ActionResult<object> GetMatrizes()
        {
            var matrizes = _matrizesService.ObterMatrizes();

            var response = new
            {
                MatrizHorariosSegunda = _matrizesService.ConverterParaListas(matrizes.MatrizHorariosSegunda),
                MatrizHorariosTerca = _matrizesService.ConverterParaListas(matrizes.MatrizHorariosTerca),
                MatrizHorariosQuarta = _matrizesService.ConverterParaListas(matrizes.MatrizHorariosQuarta),
                MatrizHorariosQuinta = _matrizesService.ConverterParaListas(matrizes.MatrizHorariosQuinta),
                MatrizHorariosSexta = _matrizesService.ConverterParaListas(matrizes.MatrizHorariosSexta)
            };

            return Ok(response);
        }
    }
}
