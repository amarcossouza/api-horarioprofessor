using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector; // Adicione esta linha para usar MySqlConnection

namespace API_HorarioProfessor.Services
{
    public class ParametroDiarioOficialService : IParametroDiarioOficialService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParametroDiarioOficialService> _logger; // Adicionando suporte para logging

        public ParametroDiarioOficialService(ApplicationDbContext context, ILogger<ParametroDiarioOficialService> logger)
        {
            _context = context;
            _logger = logger; // Inicializando o logger
        }

        public async Task<IEnumerable<Models.ParametroDiarioOficial>> GetParametrosAsync()
        {
            return await _context.ParametrosDiarioOficial.ToListAsync();
        }

        public async Task<Models.ParametroDiarioOficial> GetParametroByIdAsync(int id)
        {
            return await _context.ParametrosDiarioOficial.FindAsync(id);
        }

        public async Task AddParametroAsync(Models.ParametroDiarioOficial parametro)
        {
            _context.ParametrosDiarioOficial.Add(parametro);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteParametroAsync(int id)
        {
            var parametro = await _context.ParametrosDiarioOficial.FindAsync(id);
            if (parametro != null)
            {
                _context.ParametrosDiarioOficial.Remove(parametro);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> TestarConexaoAsync()
        {
            try
            {
                // Obtenha a string de conexão do seu ApplicationDbContext
                var connectionString = _context.Database.GetDbConnection().ConnectionString;

                // Tente abrir uma conexão com o banco de dados
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return "Conexão bem-sucedida"; // Conexão bem-sucedida
                }
            }
            catch (Exception ex)
            {
                // Registrar o erro para análise e retornar a mensagem do erro
                _logger.LogError($"Erro ao tentar conectar: {ex.Message}");
                return $"Falha na conexão: {ex.Message}"; // Retornar a mensagem do erro
            }


        }

        public async Task<IEnumerable<Models.ParametroDiarioOficial>> GetParametrosByCodigoEscolaAsync(int codigoEscola)
        {
            var parametro =  await _context.ParametrosDiarioOficial
                .Where(p => p.CodigoEscola == codigoEscola)
                .ToListAsync();
            return parametro;
        }
    }
}
