using API_HorarioProfessor.Data;
using API_HorarioProfessor.DTO;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API_HorarioProfessor.Services.ServicesCore
{
    public class HorarioService
    {
        private readonly ApplicationDbContext _context;

        public HorarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SalvarHorarios(string[][,] matrizes, int idEscola, string periodo, int anoLetivo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    await connection.OpenAsync();

                var diasDaSemana = new[] { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira" };
                var nomePlanilhaAutomatico = $"Planilha_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}";

                // Capturar a data e a hora uma única vez
                var dataCadastro = DateTime.Now.Date;  // Data atual
                var horaCadastro = DateTime.Now.TimeOfDay;  // Hora atual

                for (int diaIndex = 0; diaIndex < diasDaSemana.Length; diaIndex++)
                {
                    var matriz = matrizes[diaIndex];
                    string diaSemana = diasDaSemana[diaIndex];

                    for (int linha = 0; linha < matriz.GetLength(0); linha++)
                    {
                        for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                        {
                            string professor = matriz[linha, coluna];

                            string query = @"INSERT INTO HorarioDistribuicao 
                                     (IdEscola, AnoLetivo, Periodo, NomePlanilha, DiaSemana, Linha, Coluna, Professor, DataCadastro, HoraCadastro) 
                                     VALUES (@IdEscola, @AnoLetivo, @Periodo, @NomePlanilha, @DiaSemana, @Linha, @Coluna, @Professor, @DataCadastro, @HoraCadastro);";

                            await connection.ExecuteAsync(query, new
                            {
                                IdEscola = idEscola,
                                AnoLetivo = anoLetivo,
                                Periodo = periodo,
                                NomePlanilha = nomePlanilhaAutomatico,
                                DiaSemana = diaSemana,
                                Linha = linha,
                                Coluna = coluna,
                                Professor = professor,
                                DataCadastro = dataCadastro,
                                HoraCadastro = horaCadastro  // Usando a mesma data e hora capturadas
                            });
                        }
                    }
                }
                return true;
            }
        }



        //////////public async Task<bool> SalvarMatrizComoJson(string[][,] matrizes, List<string> turmas, int idEscola, string periodo, int anoLetivo)
        //////////{
        //////////    using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
        //////////    {
        //////////        if (connection.State == ConnectionState.Closed)
        //////////            await connection.OpenAsync();

        //////////        // Converter cada matriz string[,] para string[][]
        //////////        var matrizesJagged = matrizes.Select(ConverterParaJaggedArray).ToArray();

        //////////        // Serializar as matrizes como JSON
        //////////        var matrizJson = JsonSerializer.Serialize(matrizesJagged);

        //////////        // Serializar as turmas como JSON
        //////////        var turmasJson = JsonSerializer.Serialize(turmas);

        //////////        var nomePlanilhaAutomatico = $"Planilha_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}";
        //////////        var dataCadastro = DateTime.Now.Date;
        //////////        var horaCadastro = DateTime.Now.TimeOfDay;


        //////////        // Verifica se a matriz já existe no banco de dados
        //////////        string queryVerificar = @"SELECT COUNT(MatrizHorarios) 
        //////////                          FROM HorarioDistribuicaoJson
        //////////                          WHERE IdEscola = @IdEscola 
        //////////                          AND AnoLetivo = @AnoLetivo
        //////////                          AND Periodo = @Periodo
        //////////                          AND MatrizHorarios = @MatrizHorarios;";

        //////////        var count = await connection.ExecuteScalarAsync<int>(queryVerificar, new
        //////////        {
        //////////            IdEscola = idEscola,
        //////////            AnoLetivo = anoLetivo,
        //////////            Periodo = periodo,
        //////////            MatrizHorarios = matrizJson
        //////////        });


        //////////        // Se já existir, retornar false para indicar que não foi salvo
        //////////        if (count > 0)
        //////////        {
        //////////            return false; // A matriz já está gravada
        //////////        }



        //////////        // Query para salvar no banco, incluindo a nova coluna Turmas
        //////////        string query = @"INSERT INTO HorarioDistribuicaoJson 
        //////////                 (IdEscola, AnoLetivo, Periodo, NomePlanilha, MatrizHorarios, Turmas, DataCadastro, HoraCadastro) 
        //////////                 VALUES (@IdEscola, @AnoLetivo, @Periodo, @NomePlanilha, @MatrizHorarios, @Turmas, @DataCadastro, @HoraCadastro);";

        //////////        await connection.ExecuteAsync(query, new
        //////////        {
        //////////            IdEscola = idEscola,
        //////////            AnoLetivo = anoLetivo,
        //////////            Periodo = periodo,
        //////////            NomePlanilha = nomePlanilhaAutomatico,
        //////////            MatrizHorarios = matrizJson,
        //////////            Turmas = turmasJson, // Insere o JSON das turmas
        //////////            DataCadastro = dataCadastro,
        //////////            HoraCadastro = horaCadastro
        //////////        });

        //////////        return true;
        //////////    }
        //////////}


        public async Task<bool> SalvarMatrizComoJson(string[][,] matrizes, List<string> turmas, int idEscola, string periodo, int anoLetivo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();

                // Converter cada matriz string[,] para string[][]
                var matrizesJagged = matrizes.Select(ConverterParaJaggedArray).ToArray();

                // Serializar as matrizes como JSON para gravar (mantém como antes)
                var matrizJson = JsonSerializer.Serialize(matrizesJagged);

                // Serializar as turmas como JSON para gravar (mantém como antes)
                var turmasJson = JsonSerializer.Serialize(turmas);

                // Remover espaços em branco, quebras de linha, tabs, etc. do JSON para COMPARAÇÃO
                var matrizJsonComparacao = RemoverEspacosEmBranco(matrizJson);
                var turmasJsonComparacao = RemoverEspacosEmBranco(turmasJson);

                // Query para buscar todos os registros existentes no banco de dados
                string queryBuscar = @"SELECT MatrizHorarios, Turmas 
                               FROM HorarioDistribuicaoJson
                               WHERE IdEscola = @IdEscola 
                               AND AnoLetivo = @AnoLetivo
                               AND Periodo = @Periodo";

                var registros = await connection.QueryAsync(queryBuscar, new
                {
                    IdEscola = idEscola,
                    AnoLetivo = anoLetivo,
                    Periodo = periodo
                });

                // Iterar sobre todos os registros e comparar as matrizes e turmas (apenas para comparação)
                foreach (var registro in registros)
                {
                    // Remover espaços em branco dos JSONs armazenados no banco para COMPARAÇÃO
                    var matrizBancoComparacao = RemoverEspacosEmBranco(registro.MatrizHorarios);
                    var turmasBancoComparacao = RemoverEspacosEmBranco(registro.Turmas);

                    // Comparar os JSONs sem os espaços em branco
                    if (matrizJsonComparacao == matrizBancoComparacao && turmasJsonComparacao == turmasBancoComparacao)
                    {
                        return false; // Já existe uma matriz igual, não salvar
                    }
                }

                TimeZoneInfo brtZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brtZone);


                // Caso contrário, salva a nova matriz (mantém como antes)
                var nomePlanilhaAutomatico = $"Planilha_{now:dd_MM_yyyy_HH_mm_ss}";
                var dataCadastro = now;
                var horaCadastro = now.TimeOfDay;

                // Query para salvar no banco (mantém como antes)
                string querySalvar = @"INSERT INTO HorarioDistribuicaoJson 
                               (IdEscola, AnoLetivo, Periodo, NomePlanilha, MatrizHorarios, Turmas, DataCadastro, HoraCadastro) 
                               VALUES (@IdEscola, @AnoLetivo, @Periodo, @NomePlanilha, @MatrizHorarios, @Turmas, @DataCadastro, @HoraCadastro);";

                await connection.ExecuteAsync(querySalvar, new
                {
                    IdEscola = idEscola,
                    AnoLetivo = anoLetivo,
                    Periodo = periodo,
                    NomePlanilha = nomePlanilhaAutomatico,
                    MatrizHorarios = matrizJson,
                    Turmas = turmasJson, // Salva as turmas como JSON (mantém como antes)
                    DataCadastro = dataCadastro,
                    HoraCadastro = horaCadastro
                });

                return true; // A matriz foi salva com sucesso
            }
        }

        // Método para remover espaços em branco, quebras de linha, tabs, etc. do JSON para COMPARAÇÃO
        private string RemoverEspacosEmBranco(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? input : new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }






        public async Task<(string[][,], List<string>)> RecuperarMatrizComoJson(int idEscola, string periodo, int anoLetivo)
        {
            using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();

                // Query para buscar a matriz JSON e as turmas no banco de dados
                string query = @"SELECT MatrizHorarios, Turmas FROM HorarioDistribuicaoJson 
                         WHERE IdEscola = @IdEscola AND AnoLetivo = @AnoLetivo AND Periodo = @Periodo
                         LIMIT 1;";

                var result = await connection.QuerySingleOrDefaultAsync<(string MatrizHorarios, string Turmas)>(query, new
                {
                    IdEscola = idEscola,
                    AnoLetivo = anoLetivo,
                    Periodo = periodo
                });

                if (result.MatrizHorarios != null && result.Turmas != null)
                {
                    // Desserializar o JSON de MatrizHorarios de volta para string[][][]
                    var matrizesJagged = JsonSerializer.Deserialize<string[][][]>(result.MatrizHorarios);

                    // Converter de string[][][] para string[,]
                    var matrizes = matrizesJagged.Select(ConverterParaMatriz).ToArray();

                    // Desserializar o JSON de Turmas de volta para List<string>
                    var turmas = JsonSerializer.Deserialize<List<string>>(result.Turmas);

                    // Retornar as matrizes e as turmas
                    return (matrizes, turmas);
                }

                // Retornar matrizes vazias e uma lista de turmas vazia se não encontrar nada
                return (new string[5][,], new List<string>());
            }
        }





        public static string[,] ConverterParaMatriz(string[][] jaggedArray)
        {
            int linhas = jaggedArray.Length;
            int colunas = jaggedArray[0].Length;
            var resultado = new string[linhas, colunas];

            for (int i = 0; i < linhas; i++)
            {
                for (int j = 0; j < colunas; j++)
                {
                    resultado[i, j] = jaggedArray[i][j];
                }
            }

            return resultado;
        }

        // Método auxiliar para converter de string[,] para string[][]
        public string[][] ConverterParaJaggedArray(string[,] matriz)
        {
            int linhas = matriz.GetLength(0);
            int colunas = matriz.GetLength(1);
            var resultado = new string[linhas][];

            for (int i = 0; i < linhas; i++)
            {
                resultado[i] = new string[colunas];
                for (int j = 0; j < colunas; j++)
                {
                    resultado[i][j] = matriz[i, j];
                }
            }

            return resultado;
        }
    }
}
