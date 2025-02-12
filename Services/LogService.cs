using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API_HorarioProfessor.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void RegistrarLog(int escolaId, string nomeEscola, int usuarioId, string usuarioLogin, string descricao, int quantidadePesquisas)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                var query = @"INSERT INTO PesquisaLog (EscolaId, NomeEscola, UsuarioId, UsuarioLogin, Descricao, DataHoraPesquisa, QuantidadePesquisas) 
                              VALUES (@EscolaId,@NomeEscola, @UsuarioId, @UsuarioLogin, @Descricao, @DataHoraPesquisa, @QuantidadePesquisas)";

                var parameters = new
                {
                    EscolaId = escolaId,
                    NomeEscola = nomeEscola,
                    UsuarioId = usuarioId,
                    UsuarioLogin = usuarioLogin, // Incluído o campo UsuarioLogin
                    Descricao = descricao,
                    DataHoraPesquisa = DateTime.Now,
                    QuantidadePesquisas = quantidadePesquisas
                };

                connection.Execute(query, parameters);
            }
        }

        public List<PesquisaLog> ObterLogsPorEscola(int escolaId)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                var query = @"SELECT * FROM PesquisaLog WHERE EscolaId = @EscolaId";
                return connection.Query<PesquisaLog>(query, new { EscolaId = escolaId }).ToList();
            }
        }

        public List<RegistroAcessosDiarioOficial> ContarConsultasTodasEscolas()
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                var query = @"
            SELECT 
                EscolaId, 
                NomeEscola, 
                usuarioLogin, 
                sum(QuantidadePesquisas) as QuantidadePesquisas
            FROM 
                PesquisaLog 
            GROUP BY 
                EscolaId, 
                NomeEscola, 
                usuarioLogin,
                QuantidadePesquisas";

                return connection.Query<RegistroAcessosDiarioOficial>(query).ToList();
            }
        }

    }
}
