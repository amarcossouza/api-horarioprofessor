using System.Collections.Generic;
using System.Threading.Tasks;
using API_HorarioProfessor.DTO;
using API_HorarioProfessor.Models;

namespace API_HorarioProfessor.Services
{
    public interface IParametroDiarioOficialService
    {
        /// <summary>
        /// Obtém todos os parâmetros.
        /// </summary>
        /// <returns>Uma lista de todos os parâmetros.</returns>
        Task<IEnumerable<ParametroDiarioOficial>> GetParametrosAsync();

        /// <summary>
        /// Obtém um parâmetro pelo seu ID.
        /// </summary>
        /// <param name="id">O ID do parâmetro.</param>
        /// <returns>O parâmetro correspondente ao ID.</returns>
        Task<ParametroDiarioOficial> GetParametroByIdAsync(int id);

        /// <summary>
        /// Adiciona um novo parâmetro.
        /// </summary>
        /// <param name="parametro">O parâmetro a ser adicionado.</param>
        Task AddParametroAsync(ParametroDiarioOficial parametro);

        /// <summary>
        /// Atualiza um parâmetro existente.
        /// </summary>
        /// <param name="parametro">O parâmetro a ser atualizado.</param>
        //Task UpdateParametroAsync(ParametroDiarioOficial parametro);

        /// <summary>
        /// Remove um parâmetro pelo seu ID.
        /// </summary>
        /// <param name="id">O ID do parâmetro a ser removido.</param>
        Task DeleteParametroAsync(int id);

        /// <summary>
        /// Testa a conexão com o banco de dados.
        /// </summary>
        /// <returns>Uma mensagem indicando o sucesso ou falha da conexão.</returns>
        Task<string> TestarConexaoAsync();

        /// <summary>
        /// Obtém parâmetros baseados no código da escola.
        /// </summary>
        /// <param name="codigoEscola">O código da escola.</param>
        /// <returns>Uma lista de parâmetros correspondentes ao código da escola.</returns>
        Task<IEnumerable<ParametroDiarioOficial>> GetParametrosByCodigoEscolaAsync(int codigoEscola);
    }
}
