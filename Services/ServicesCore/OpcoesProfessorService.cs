using API_HorarioProfessor.Data;
using API_HorarioProfessor.Dto;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace API_HorarioProfessor.Services.ServicesCore
{
    public class OpcoesProfessorService : IOpcoesProfessorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        public OpcoesProfessorService(ApplicationDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        public async Task<IEnumerable<OpcaoHorarioDto>> SelecionarOpcoesPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"SELECT Id, IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                      Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                      DataInativado, HoraInativado, AnoLetivo, Periodo, IdEscola 
                      FROM hprof_opcaohorario
                      WHERE AnoLetivo = @AnoLetivo 
                      AND Periodo = @Periodo 
                      AND Ativo = @Ativo 
                      AND IdEscola = @IdEscola
                      AND Sigla<> 'REMOVE'";

                var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, new
                {
                    AnoLetivo = anoLetivo,
                    Periodo = periodo,
                    Ativo = ativo,
                    IdEscola = idEscola
                });

                return opcoes.AsList();
            }
        }

        public async Task<IEnumerable<OpcaoHorarioDto>> SelecionarOpcoesProcessamentoPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var query = @"SELECT Id, IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                      Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                      DataInativado, HoraInativado, AnoLetivo, Periodo, IdEscola 
                      FROM hprof_opcaohorarioprocessamento
                      WHERE AnoLetivo = @AnoLetivo 
                      AND Periodo = @Periodo 
                      AND Ativo = @Ativo 
                      AND IdEscola = @IdEscola";

                var opcoes = await connection.QueryAsync<OpcaoHorarioDto>(query, new
                {
                    AnoLetivo = anoLetivo,
                    Periodo = periodo,
                    Ativo = ativo,
                    IdEscola = idEscola
                });

                return opcoes.AsList();
            }
        }


        //////public async Task<int> ApagarOpcoesPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola)
        //////{
        //////    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //////    {
        //////        if (connection.State == System.Data.ConnectionState.Closed)
        //////        {
        //////            await connection.OpenAsync();
        //////        }

        //////        var query = @"DELETE FROM opcaohorarioprocessamento
        //////              WHERE AnoLetivo = @AnoLetivo
        //////              AND Periodo = @Periodo
        //////              AND Ativo = @Ativo
        //////              AND IdEscola = @IdEscola";


        //////        var result = await connection.ExecuteAsync(query, new
        //////        {
        //////            AnoLetivo = anoLetivo,
        //////            Periodo = periodo,
        //////            Ativo = ativo,
        //////            IdEscola = idEscola
        //////        });

        //////        return result; // Retorna o número de registros apagados
        //////    }
        //////}

        public async Task<int> ApagarOpcoesPorFiltrosAsync(int anoLetivo, string periodo, int ativo, int idEscola, int AcumuloCargo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var query = @"
                    DELETE FROM hprof_opcaohorarioprocessamento
                    WHERE AnoLetivo = @AnoLetivo
                    AND Periodo = @Periodo
                    AND Ativo = @Ativo
                    AND IdEscola = @IdEscola";

                        if (AcumuloCargo == 1)
                        {
                            //query += " AND Sigla <> 'AC'";
                            query += " AND Sigla <> 'ATPC'";

                        }

                        // Executa a operação de exclusão dentro da transação
                        var result = await connection.ExecuteAsync(query, new
                        {
                            AnoLetivo = anoLetivo,
                            Periodo = periodo,
                            Ativo = ativo,
                            IdEscola = idEscola
                        }, transaction: transaction);

                        // Confirma a transação
                        await transaction.CommitAsync();

                        return result; // Retorna o número de registros apagados
                    }
                    catch
                    {
                        // Reverte a transação em caso de erro
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }




        //////public async Task<int> ApagarOpcoesPorFiltrosComProfessoresAsync(int anoLetivo, string periodo, int ativo, int idEscola, List<int> listaProfessores)
        //////{
        //////    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //////    {
        //////        if (connection.State == System.Data.ConnectionState.Closed)
        //////        {
        //////            await connection.OpenAsync();
        //////        }

        //////        // Concatena os IDs dos professores na lista para o uso na cláusula IN
        //////        var professoresIds = listaProfessores != null && listaProfessores.Any()
        //////            ? string.Join(",", listaProfessores)
        //////            : "NULL";  // Caso a lista esteja vazia ou nula, define como "NULL"

        //////        // Query de DELETE com a adição da lista de professores
        //////        var query = $@"
        //////    DELETE FROM opcaohorarioprocessamento
        //////    WHERE AnoLetivo = @AnoLetivo
        //////    AND Periodo = @Periodo
        //////    AND Ativo = @Ativo
        //////    AND IdEscola = @IdEscola
        //////    AND IdProfessor_fk IN ({professoresIds})";  // Adiciona o filtro para professores

        //////        var result = await connection.ExecuteAsync(query, new
        //////        {
        //////            AnoLetivo = anoLetivo,
        //////            Periodo = periodo,
        //////            Ativo = ativo,
        //////            IdEscola = idEscola
        //////        });

        //////        return result; // Retorna o número de registros apagados
        //////    }
        //////}


        public async Task<int> ApagarOpcoesPorFiltrosComProfessoresAsync(int anoLetivo, string periodo, int ativo, int idEscola, List<int> listaProfessores)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Concatena os IDs dos professores para a cláusula IN
                        var professoresIds = listaProfessores != null && listaProfessores.Any()
                            ? string.Join(",", listaProfessores)
                            : "NULL"; // Define como "NULL" se a lista estiver vazia ou nula

                        // Query de DELETE com a lista de professores
                        var query = $@"
                    DELETE FROM hprof_opcaohorarioprocessamento
                    WHERE AnoLetivo = @AnoLetivo
                    AND Periodo = @Periodo
                    AND Ativo = @Ativo
                    AND IdEscola = @IdEscola
                    AND IdProfessor_fk IN ({professoresIds})";

                        // Executa a operação de exclusão dentro da transação
                        var result = await connection.ExecuteAsync(query, new
                        {
                            AnoLetivo = anoLetivo,
                            Periodo = periodo,
                            Ativo = ativo,
                            IdEscola = idEscola
                        }, transaction: transaction);

                        // Confirma a transação
                        await transaction.CommitAsync();

                        return result; // Retorna o número de registros apagados
                    }
                    catch
                    {
                        // Reverte a transação em caso de erro
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }




        //////public async Task<int> CopiarOpcoesParaProcessamentoAsync(int anoLetivo, string periodo, int ativo, int idEscola, int opcao)
        //////{
        //////    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //////    {
        //////        if (connection.State == System.Data.ConnectionState.Closed)
        //////        {
        //////            await connection.OpenAsync();
        //////        }

        //////        // Consulta para inserir na tabela OpcaoHorarioProcessamento os dados da tabela OpcaoHorario
        //////        var query = @"
        //////    INSERT INTO opcaohorarioprocessamento (IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana,
        //////        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
        //////        DataInativado, HoraInativado, AnoLetivo, Periodo)
        //////    SELECT IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
        //////        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
        //////        DataInativado, HoraInativado, AnoLetivo, Periodo
        //////    FROM opcaohorario
        //////    WHERE AnoLetivo = @AnoLetivo 
        //////    AND Periodo = @Periodo 
        //////    AND Ativo = @Ativo 
        //////    AND IdEscola = @IdEscola
        //////    AND OPCAO = @opcao
        //////    AND Sigla<> 'REMOVE'";

        //////        var result = await connection.ExecuteAsync(query, new
        //////        {
        //////            AnoLetivo = anoLetivo,
        //////            Periodo = periodo,
        //////            Ativo = ativo,
        //////            IdEscola = idEscola,
        //////            opcao = opcao
        //////        });

        //////        return result; // Retorna o número de registros copiados
        //////    }
        //////}


        public async Task<int> CopiarOpcoesParaProcessamentoAsync(int anoLetivo, string periodo, int ativo, int idEscola, int opcao)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                 }

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Consulta para inserir na tabela OpcaoHorarioProcessamento os dados da tabela OpcaoHorario
                        var query = @"
                    INSERT INTO hprof_opcaohorarioprocessamento (IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana,
                        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                        DataInativado, HoraInativado, AnoLetivo, Periodo)
                    SELECT IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                        DataInativado, HoraInativado, AnoLetivo, Periodo
                    FROM hprof_opcaohorario
                    WHERE AnoLetivo = @AnoLetivo 
                    AND Periodo = @Periodo 
                    AND Ativo = @Ativo 
                    AND IdEscola = @IdEscola
                    AND Opcao = @Opcao
                    AND Sigla <> 'REMOVE'";

                        // Executa a operação de inserção dentro da transação
                        var result = await connection.ExecuteAsync(query, new
                        {
                            AnoLetivo = anoLetivo,
                            Periodo = periodo,
                            Ativo = ativo,
                            IdEscola = idEscola,
                            Opcao = opcao
                        }, transaction: transaction);

                        // Confirma a transação
                        await transaction.CommitAsync();

                        return result; // Retorna o número de registros copiados
                    }
                    catch
                    {
                        // Reverte a transação em caso de erro
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }






        //////public async Task<int> CopiarOpcoesParaProcessamentoComExclusaoAsync(
        //////int anoLetivo, string periodo, int ativo, int idEscola, int opcao, List<int> listaProfessoresExcluidos, string filtroProfessores)
        //////    {
        //////        using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //////        {
        //////            if (connection.State == System.Data.ConnectionState.Closed)
        //////            {
        //////                await connection.OpenAsync();
        //////            }

        //////            // Verifica se há professores a serem incluídos/excluídos na lista e prepara o filtro
        //////            var professoresExcluidos = listaProfessoresExcluidos != null && listaProfessoresExcluidos.Any()
        //////                ? string.Join(",", listaProfessoresExcluidos)  // Concatena os valores da lista separados por vírgula
        //////                : "NULL";  // Caso a lista esteja vazia ou nula

        //////            // Define a operação do filtro (IN ou NOT IN)
        //////            string condicaoFiltroProfessores = filtroProfessores == "IN"
        //////                ? $"IdProfessor_fk IN ({professoresExcluidos})"
        //////                : $"IdProfessor_fk NOT IN ({professoresExcluidos})";

        //////            // Consulta para inserir na tabela OpcaoHorarioProcessamento os dados da tabela OpcaoHorario
        //////            var query = $@"
        //////    INSERT INTO opcaohorarioprocessamento (IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana,
        //////        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
        //////        DataInativado, HoraInativado, AnoLetivo, Periodo)
        //////    SELECT IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
        //////        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
        //////        DataInativado, HoraInativado, AnoLetivo, Periodo
        //////    FROM opcaohorario
        //////    WHERE AnoLetivo = @AnoLetivo 
        //////    AND Periodo = @Periodo 
        //////    AND Ativo = @Ativo 
        //////    AND IdEscola = @IdEscola
        //////    AND Opcao = @Opcao
        //////    AND Sigla <> 'REMOVE'
        //////    AND {condicaoFiltroProfessores}";  // Usa o filtro dinamicamente (IN ou NOT IN)

        //////        var result = await connection.ExecuteAsync(query, new
        //////        {
        //////            AnoLetivo = anoLetivo,
        //////            Periodo = periodo,
        //////            Ativo = ativo,
        //////            IdEscola = idEscola,
        //////            Opcao = opcao
        //////        });

        //////        return result; // Retorna o número de registros copiados
        //////    }
        //////}


        public async Task<int> CopiarOpcoesParaProcessamentoComExclusaoAsync(
    int anoLetivo, string periodo, int ativo, int idEscola, int opcao, List<int> listaProfessoresExcluidos, string filtroProfessores)
        {
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Configuração do filtro para professores excluídos
                        var professoresExcluidos = listaProfessoresExcluidos != null && listaProfessoresExcluidos.Any()
                            ? string.Join(",", listaProfessoresExcluidos)
                            : "NULL";

                        string condicaoFiltroProfessores = filtroProfessores == "IN"
                            ? $"IdProfessor_fk IN ({professoresExcluidos})"
                            : $"IdProfessor_fk NOT IN ({professoresExcluidos})";

                        // Consulta de exclusão antes de inserção (caso necessário)
                        var queryExclusao = @"
                    DELETE FROM hprof_opcaohorarioprocessamento
                    WHERE AnoLetivo = @AnoLetivo
                    AND Periodo = @Periodo
                    AND IdEscola = @IdEscola
                    AND Opcao = @Opcao";

                        await connection.ExecuteAsync(queryExclusao, new
                        {
                            AnoLetivo = anoLetivo,
                            Periodo = periodo,
                            IdEscola = idEscola,
                            Opcao = opcao
                        }, transaction: transaction);

                        // Inserção dos dados na tabela OpcaoHorarioProcessamento
                        var queryInsercao = $@"
                    INSERT INTO hprof_opcaohorarioprocessamento (IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana,
                        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                        DataInativado, HoraInativado, AnoLetivo, Periodo)
                    SELECT IdEscola, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, 
                        Tipo, Sigla, Ativo, DataCadastro, HoraCadastro, 
                        DataInativado, HoraInativado, AnoLetivo, Periodo
                    FROM hprof_opcaohorario
                    WHERE AnoLetivo = @AnoLetivo 
                    AND Periodo = @Periodo 
                    AND Ativo = @Ativo 
                    AND IdEscola = @IdEscola
                    AND Opcao = @Opcao
                    AND Sigla <> 'REMOVE'
                    AND {condicaoFiltroProfessores}";

                        var result = await connection.ExecuteAsync(queryInsercao, new
                        {
                            AnoLetivo = anoLetivo,
                            Periodo = periodo,
                            Ativo = ativo,
                            IdEscola = idEscola,
                            Opcao = opcao
                        }, transaction: transaction);

                        // Confirma a transação
                        await transaction.CommitAsync();
                        return result;
                    }
                    catch
                    {
                        // Reverte a transação em caso de erro
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }



    }
}
