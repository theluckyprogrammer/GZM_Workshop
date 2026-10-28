using GzmApp.Data;
using Microsoft.Data.Sqlite;

namespace GzmApp.Specs.Infrastructure;

// Database GzmTest_{scenario}: a SQLite file in %TEMP% (or the file from GZM_TEST_CONNSTR). The app creates it
// at start-up; DropAsync deletes it after the tests. Every connection gets the SQL function plain().
public sealed class TestDatabase
{
    public const string EnvironmentVariable = "GZM_TEST_CONNSTR";

    private readonly string _file;

    public TestDatabase(string scenario, string suffix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
        _file = string.IsNullOrWhiteSpace(fromEnvironment)
            ? Path.Combine(Path.GetTempPath(), $"GzmTest_{scenario}{suffix}.db")
            : fromEnvironment;
        ConnectionString = $"Data Source={_file}";
        Name = Path.GetFileNameWithoutExtension(_file);
    }

    public string ConnectionString { get; }

    public string Name { get; }

    public async Task<T?> ScalarAsync<T>(string sql, object? param = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = Open();
        await using var command = Command(connection, sql, param);
        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    // Rows of (SourceRow, value as text); value is null for NULL.
    public async Task<IReadOnlyList<(int SourceRow, string? Value)>> PairsAsync(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = Open();
        await using var command = Command(connection, sql, param: null);
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<(int, string?)>();
        while (await reader.ReadAsync())
            rows.Add((reader.GetInt32(0), reader.IsDBNull(1) ? null : reader.GetString(1)));
        return rows;
    }

    public async Task ExecuteAsync(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = Open();
        await using var command = Command(connection, sql, param: null);
        await command.ExecuteNonQueryAsync();
    }

    // The database is the file itself (plus the -wal/-shm side files of an open connection).
    public Task DropAsync()
    {
        foreach (var side in new[] { "", "-wal", "-shm" })
            File.Delete(_file + side);
        return Task.CompletedTask;
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        connection.CreateFunction("plain", (string? value) => PlainText.Of(value));
        return connection;
    }

    private static SqliteCommand Command(SqliteConnection connection, string sql, object? param)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        (param?.GetType().GetProperties() ?? []).ToList()
            .ForEach(p => command.Parameters.AddWithValue("@" + p.Name, p.GetValue(param) ?? DBNull.Value));
        return command;
    }
}
