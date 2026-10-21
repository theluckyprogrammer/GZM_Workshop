using Microsoft.Data.SqlClient;

namespace GzmApp.Tests.Infrastructure;

// Database GzmTest_{scenario}: from GZM_TEST_CONNSTR (Database replaced) or LocalDB. The app creates it at start-up;
// DropAsync removes it after the tests.
public sealed class TestDatabase
{
    public const string EnvironmentVariable = "GZM_TEST_CONNSTR";
    private const string LocalDb = @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";

    public TestDatabase(string scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        var builder = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable(EnvironmentVariable) ?? LocalDb)
        {
            InitialCatalog = $"GzmTest_{scenario}",
        };
        ConnectionString = builder.ConnectionString;
        Name = builder.InitialCatalog;
    }

    public string ConnectionString { get; }

    public string Name { get; }

    public async Task<T?> ScalarAsync<T>(string sql, object? param = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, param);
        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task DropAsync()
    {
        var master = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(master.ConnectionString);
        await connection.OpenAsync();
        var sql = $"IF DB_ID(@name) IS NOT NULL BEGIN ALTER DATABASE [{Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{Name}]; END";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", Name);
        await command.ExecuteNonQueryAsync();
    }

    private static void AddParameters(SqlCommand command, object? param) =>
        (param?.GetType().GetProperties() ?? []).ToList()
            .ForEach(p => command.Parameters.AddWithValue("@" + p.Name, p.GetValue(param) ?? DBNull.Value));
}
