using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;

namespace GzmApp.Data;

// Thin layer over Dapper. Every query is written to the log (category "SQL") with its parameters.
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

    public string DatabaseName => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

    public SqlRunner ForDatabase(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        var builder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = databaseName };
        return new SqlRunner(builder.ConnectionString, _loggerFactory);
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        await using var connection = new SqlConnection(ConnectionString);
        var rows = await connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        await using var connection = new SqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        await using var connection = new SqlConnection(ConnectionString);
        return await connection.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    private void Log(string sql, object? param) =>
        _logger.LogInformation("{Sql} | parametry: {Parameters}", sql.Trim(), FormatParameters(param));

    private static string FormatParameters(object? param) =>
        param is null ? "(brak)" : JsonSerializer.Serialize(param);
}
