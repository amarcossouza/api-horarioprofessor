using MySqlConnector;

namespace API_HorarioProfessor.Universidade;

/// <summary>
/// Abre conexões MySQL para o módulo universidade (Dapper com SQL explícito).
/// Registrada como singleton com a mesma string de conexão da API.
/// </summary>
public sealed class UniversidadeConexao
{
    public UniversidadeConexao(string connectionString)
    {
        if (connectionString is null) throw new ArgumentNullException(nameof(connectionString));
        // a gravação de um curso inteiro faz centenas de comandos num banco remoto e compartilhado:
        // tempo-limite maior só nas conexões do módulo (a string da API não muda)
        var cs = new MySqlConnectionStringBuilder(connectionString);
        if (cs.DefaultCommandTimeout < 300) cs.DefaultCommandTimeout = 300;
        ConnectionString = cs.ConnectionString;
    }

    public string ConnectionString { get; }

    public async Task<MySqlConnection> AbrirAsync()
    {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
