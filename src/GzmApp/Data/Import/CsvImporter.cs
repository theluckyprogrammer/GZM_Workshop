using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace GzmApp.Data.Import;

// Generic import driven by scenarios/<Scenario>/mapping.json: CSV → #Staging → MERGE, in one transaction.
public sealed class CsvImporter : ICsvImporter
{
    private readonly SqlRunner _sql;
    private readonly ScenarioMapping _mapping;
    private readonly ImportPaths _paths;
    private readonly ILogger<CsvImporter> _logger;

    public CsvImporter(SqlRunner sql, ScenarioMapping mapping, ImportPaths paths, ILogger<CsvImporter> logger)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(logger);

        _sql = sql;
        _mapping = mapping;
        _paths = paths;
        _logger = logger;
    }

    public async Task<ImportResult?> ImportIfEmptyAsync(CancellationToken ct = default)
    {
        await EnsureTableAsync(ct);
        var rows = await CountRowsAsync(ct);
        if (rows == 0)
            return await ImportAsync(ct);

        _logger.LogInformation("Tabela {Table} ma już {Rows} wierszy – import pominięty (wymuś: dotnet run -- import).", _mapping.Table, rows);
        return null;
    }

    public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        await EnsureTableAsync(ct);
        var file = _paths.DataFile(_mapping);
        var table = CsvSource.Read(file, _mapping.Source);
        var records = RecordReader.Read(_mapping, table);
        _logger.LogInformation("Plik {File}: kodowanie {Encoding}, {Rows} wierszy danych, {Records} do importu.",
            Path.GetFileName(file), table.EncodingName, table.Rows.Count, records.Count);

        var weird = await SqlRetry.RunAsync(() => _sql.InTransactionAsync(tx => MergeAsync(tx, records, ct), ct), _logger, ct);
        var result = new ImportResult(_mapping.Table, await CountRowsAsync(ct), weird, stopwatch.Elapsed);
        _logger.LogInformation("Import {Scenario}: {Rows} wierszy w tabeli {Table}, „dziwnych” rekordów: {Weird}, czas: {Seconds:F1} s.",
            _mapping.Scenario, result.TableRows, result.Table, result.WeirdRecords, result.Elapsed.TotalSeconds);
        return result;
    }

    private async Task<int> MergeAsync(SqlTransaction transaction, IReadOnlyList<ImportRecord> records, CancellationToken ct)
    {
        await _sql.ExecuteAsync(transaction, RecordTableBuilder.CreateStaging(_mapping), ct: ct);
        await StagingLoader.LoadAsync(_sql, transaction, _mapping, records, ct);
        var weird = await _sql.ExecuteScalarAsync<int>(transaction, MergeSql.CountWeird(_mapping), ct: ct);
        await _sql.ExecuteAsync(transaction, MergeSql.Merge(_mapping), ct: ct);
        return weird;
    }

    private async Task EnsureTableAsync(CancellationToken ct)
    {
        var script = await RecordTableBuilder.WriteScriptAsync(_mapping, _paths.DbDirectory, ct);
        await SqlRetry.RunAsync(() => _sql.ExecuteAsync(script, ct: ct), _logger, ct);
    }

    private Task<int> CountRowsAsync(CancellationToken ct) =>
        SqlRetry.RunAsync(() => _sql.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM dbo.[{_mapping.Table}]", ct: ct), _logger, ct);
}
