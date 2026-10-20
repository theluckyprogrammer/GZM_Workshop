using System.Data;
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

    // One connection and one transaction. Without CommitAsync (exception) the transaction is rolled back.
    public async Task<T> InTransactionAsync<T>(Func<SqlTransaction, Task<T>> work, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);
        var result = await work(transaction);
        await transaction.CommitAsync(ct);
        return result;
    }

    public async Task<T?> ExecuteScalarAsync<T>(SqlTransaction transaction, string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        return await transaction.Connection.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, transaction, cancellationToken: ct));
    }

    public async Task<int> ExecuteAsync(SqlTransaction transaction, string sql, object? param = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Log(sql, param);
        return await transaction.Connection.ExecuteAsync(new CommandDefinition(sql, param, transaction, cancellationToken: ct));
    }

    // The same statement for every parameter set (Dapper runs it once per row). Only the number of rows is logged.
    public async Task<int> ExecuteManyAsync(SqlTransaction transaction, string sql, IReadOnlyCollection<object> rows, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(rows);

        _logger.LogInformation("{Sql} | parametry: {Count} wierszy", sql.Trim(), rows.Count);
        return await transaction.Connection.ExecuteAsync(new CommandDefinition(sql, rows, transaction, cancellationToken: ct));
    }

    public async Task BulkCopyAsync(SqlTransaction transaction, string destination, DataTable rows, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(rows);

        _logger.LogInformation("SqlBulkCopy → {Destination} | {Count} wierszy", destination, rows.Rows.Count);
        using var bulk = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction) { DestinationTableName = destination };
        foreach (DataColumn column in rows.Columns)
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        await bulk.WriteToServerAsync(rows, ct);
    }

    private void Log(string sql, object? param) =>
        _logger.LogInformation("{Sql} | parametry: {Parameters}", sql.Trim(), FormatParameters(param));

    private static string FormatParameters(object? param) =>
        param is null ? "(brak)" : JsonSerializer.Serialize(param);
}
