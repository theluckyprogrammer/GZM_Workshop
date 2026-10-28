using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;

namespace GzmApp.Data;

// Thin layer over Dapper. Every query is written to the log (category "SQL") with its parameters.
// Every connection gets the SQL function plain(): search without case or Polish diacritics.
public sealed class SqlRunner
{
    public const string LogCategory = "SQL";

    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public SqlRunner(string connectionString, ILoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        ConnectionString = connectionString;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger(LogCategory);
    }

    public string ConnectionString { get; }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        await using var connection = Open();
        var rows = await connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        await using var connection = Open();
        return await connection.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        await using var connection = Open();
        return await connection.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    // Opening the connection creates the database file when it does not exist yet.
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        connection.CreateFunction("plain", (string? value) => PlainText.Of(value));
        return connection;
    }

    private void Log(string sql, object? param) =>
        _logger.LogInformation("{Sql} | parametry: {Parameters}", sql.Trim(), FormatParameters(param));

    private static string FormatParameters(object? param) =>
        param is null ? "(brak)" : JsonSerializer.Serialize(param);
}
