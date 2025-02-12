using API_HorarioProfessor.Data;
using API_HorarioProfessor.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;


namespace API_HorarioProfessor.Services.CoreServices.MetodosAuxiliares
{
    public class GetMaterialAulasPorAno
    {
        private readonly ApplicationDbContext _context;
        public GetMaterialAulasPorAno(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<MateriaAulasPorAno>> ObterMateriaAulasPorAnoAsync(int anoLetivo, int escolaId)
        //{
        //    using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
        //    {
        //        if (connection.State == ConnectionState.Closed)
        //        {
        //            await connection.OpenAsync();
        //        }

        //        var query = @"SELECT Id, AnoLetivo, EscolaId, DataCriacao, TipoEsTipoEcolacola, Descricao, Sigla, TodosOsAnos, QuantidaAulas
        //              FROM MateriaAulasPorAno
        //              WHERE EscolaId = @EscolaId
        //              AND AnoLetivo = @AnoLetivo";


        //        var resultados = await connection.QueryAsync<MateriaAulasPorAno>(query, new
        //        {
        //            EscolaId = escolaId,
        //            AnoLetivo = anoLetivo
        //        });

        //        return resultados;
        //    }
        //}


        //////public async Task<IEnumerable<MateriaAulasPorAno>> ObterMateriaAulasPorAnoAsync(int anoLetivo,string Periodo, int escolaId)
        //////{
        //////    using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
        //////    {
        //////        if (connection.State == ConnectionState.Closed)
        //////        {
        //////            await connection.OpenAsync();
        //////        }

        //////        var query = @"SELECT Id, AnoLetivo, EscolaId, DataCriacao, TipoEscola, Descricao,ModalidadeEscolar,  Sigla, TodosOsAnos, QuantidadeAulas
        //////              FROM MateriaAulasPorAno
        //////              WHERE EscolaId = @EscolaId
        //////              AND AnoLetivo = @AnoLetivo";
        //////        // AND Periodo = @Periodo";

        //////        var resultados = await connection.QueryAsync<MateriaAulasPorAno>(query, new
        //////        {
        //////            EscolaId = escolaId,
        //////            AnoLetivo = anoLetivo
        //////            //--Periodo = Periodo
        //////        });

        //////        // Se não houver resultados, retorna uma lista vazia para evitar exceção
        //////        return resultados?.ToList() ?? new List<MateriaAulasPorAno>();
        //////    }
        //////}



        ////////////public async Task<IEnumerable<MateriaAulasPorAno>> ObterMateriaAulasPorAnoAsync(int anoLetivo, string periodo, int escolaId)
        ////////////{
        ////////////    using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
        ////////////    {
        ////////////        if (connection.State == ConnectionState.Closed)
        ////////////        {
        ////////////            await connection.OpenAsync();
        ////////////        }

        ////////////        // Primeira query para obter ModalidadesEscolares distintas do CadastroProfessor
        ////////////        var queryCad = @"SELECT  DISTINCT Modalidade
        ////////////                      FROM CadastroProfessor
        ////////////                      WHERE IdEscola = @EscolaId
        ////////////                      AND AnoLetivo = @AnoLetivo
        ////////////                      AND Periodo = @Periodo";

        ////////////        var modalidadesEscolares = await connection.QueryAsync<string>(queryCad, new
        ////////////        {
        ////////////            EscolaId = escolaId,
        ////////////            AnoLetivo = anoLetivo,
        ////////////            Periodo = periodo
        ////////////        });

        ////////////        // Converte as modalidadesEscolares para uma lista para uso posterior
        ////////////        var listaModalidadesEscolares = modalidadesEscolares.ToList();

        ////////////        if (!listaModalidadesEscolares.Any())
        ////////////        {
        ////////////            // Se não houver ModalidadeEscolar correspondente no CadastroProfessor, retorne uma lista vazia
        ////////////            return new List<MateriaAulasPorAno>();
        ////////////        }

        ////////////        // Segunda query para obter dados de MateriaAulasPorAno usando as modalidades obtidas
        ////////////        var query = @"SELECT Id, AnoLetivo,Periodo, EscolaId, DataCriacao, TipoEscola, Descricao, ModalidadeEscolar, 
        ////////////                          Sigla, TodosOsAnos, QuantidadeAulas
        ////////////                   FROM MateriaAulasPorAno
        ////////////                   WHERE EscolaId = @EscolaId
        ////////////                   AND AnoLetivo = @AnoLetivo
        ////////////                   AND Periodo = @periodo
        ////////////                   AND ModalidadeEscolar IN  @ModalidadesEscolares";

        ////////////        var resultados = await connection.QueryAsync<MateriaAulasPorAno>(query, new
        ////////////        {
        ////////////            EscolaId = escolaId,
        ////////////            AnoLetivo = anoLetivo,
        ////////////            ModalidadesEscolares = listaModalidadesEscolares
        ////////////        });

        ////////////        // Retorne os resultados como uma lista, ou uma lista vazia caso não haja resultados
        ////////////        return resultados?.ToList() ?? new List<MateriaAulasPorAno>();
        ////////////    }
        ////////////}

        public async Task<IEnumerable<MateriaAulasPorAno>> ObterMateriaAulasPorAnoAsync(int anoLetivo, string periodo, int escolaId)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                // Primeira query para obter ModalidadesEscolares distintas do CadastroProfessor
                var queryCad = @"SELECT DISTINCT Modalidade
                         FROM CadastroProfessor
                         WHERE IdEscola = @EscolaId
                         AND AnoLetivo = @AnoLetivo
                         AND Periodo = @Periodo";

                var modalidadesEscolares = await connection.QueryAsync<string>(queryCad, new
                {
                    EscolaId = escolaId,
                    AnoLetivo = anoLetivo,
                    Periodo = periodo
                });

                // Converte para lista para uso posterior
                var listaModalidadesEscolares = modalidadesEscolares.ToList();

                if (!listaModalidadesEscolares.Any())
                {
                    // Retorna uma lista vazia se não houver modalidades
                    return new List<MateriaAulasPorAno>();
                }

                // Ajuste da query usando string.Join para formar corretamente o IN
                var parametrosIN = string.Join(",", listaModalidadesEscolares.Select(m => $"'{m}'"));

                var query = $@"SELECT Id, AnoLetivo, Periodo, EscolaId, DataCriacao, TipoEscola, Descricao, ModalidadeEscolar, 
                              Sigla, TodosOsAnos, QuantidadeAulas
                       FROM MateriaAulasPorAno
                       WHERE EscolaId = @EscolaId
                       AND AnoLetivo = @AnoLetivo
                       AND Periodo = @Periodo
                       AND ModalidadeEscolar IN ({parametrosIN})";

                var resultados = await connection.QueryAsync<MateriaAulasPorAno>(query, new
                {
                    EscolaId = escolaId,
                    AnoLetivo = anoLetivo,
                    Periodo = periodo
                });

                // Retorna os resultados ou uma lista vazia
                return resultados?.ToList() ?? new List<MateriaAulasPorAno>();
            }
        }





    }
}
