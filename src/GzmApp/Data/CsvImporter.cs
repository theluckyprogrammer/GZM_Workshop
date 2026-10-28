using System.Globalization;
using System.Diagnostics;
using CsvHelper;
using CsvHelper.Configuration;

namespace GzmApp.Data;

public interface ICsvImporter
{
    Task ImportAsync(CancellationToken ct = default);
}

// Imports the CSV file "Dzienna dostępność rowerów w gminach" into the BikeAvailability table.
// Rows are upserted by the natural key (Day, Commune); invalid rows are skipped and logged.
public sealed class CsvImporter : ICsvImporter
{
    private const string UpsertSql = """
        INSERT INTO [BikeAvailability] ([SourceRow], [Day], [Commune], [AvailableBikes], [RequiredBikes], [Exceeded])
        VALUES (@SourceRow, @Day, @Commune, @AvailableBikes, @RequiredBikes, @Exceeded)
        ON CONFLICT ([Day], [Commune]) DO UPDATE SET
            [SourceRow] = @SourceRow,
            [AvailableBikes] = @AvailableBikes,
            [RequiredBikes] = @RequiredBikes,
            [Exceeded] = @Exceeded
        """;

    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pl-PL");
    private static readonly string[] Formats = ["yyyy-MM-dd"];

    private readonly SqlRunner _sql;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CsvImporter> _logger;

    public CsvImporter(SqlRunner sql, IWebHostEnvironment environment, ILogger<CsvImporter> logger)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _sql = sql;
        _environment = environment;
        _logger = logger;
    }

    public async Task ImportAsync(CancellationToken ct = default)
    {
        var existing = await CountRowsAsync(ct);
        if (existing > 0)
        {
            _logger.LogInformation("Import pominięty – w tabeli jest {Count} wierszy", existing);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var rows = ReadRows();
        foreach (var row in rows)
            await UpsertAsync(row, ct);

        var count = await CountRowsAsync(ct);
        _logger.LogInformation("Zaimportowano {Count} wierszy w {Seconds:F1} s", count, stopwatch.Elapsed.TotalSeconds);
    }

    private List<BikeRow> ReadRows()
    {
        var path = Path.Combine(_environment.ContentRootPath, "..", "..", "data/raw/s03_rowery-dostepnosc-gminy.csv");
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(Culture) { Delimiter = "," });
        csv.Read();
        csv.ReadHeader();

        var raw = new List<RawRow>();
        var sourceRow = 0;
        while (csv.Read())
        {
            sourceRow++;
            raw.Add(ReadFields(sourceRow, csv));
        }

        return raw.Select(ParseRow).OfType<BikeRow>().ToList();
    }

    private RawRow ReadFields(int sourceRow, CsvReader csv) => new(
        sourceRow,
        csv.GetField("Dzień")?.Trim(),
        csv.GetField("Gmina")?.Trim(),
        csv.GetField("Liczba dostępnych rowerów")?.Trim(),
        csv.GetField("Wymagana liczba dostępnych rowerów")?.Trim(),
        csv.GetField("Przekroczono")?.Trim());

    private BikeRow? ParseRow(RawRow raw)
    {
        var day = ParseDay(raw.Day);
        var available = ParseInt(raw.Available);
        var required = ParseInt(raw.Required);
        var exceeded = ParseExceeded(raw.Exceeded);
        if (day is null || string.IsNullOrWhiteSpace(raw.Commune) || available is null || required is null || exceeded is null)
        {
            _logger.LogWarning("Pominięto wiersz {SourceRow} – puste albo nieczytelne pole", raw.SourceRow);
            return null;
        }

        return new BikeRow(raw.SourceRow, day, raw.Commune, available.Value, required.Value, exceeded.Value);
    }

    private static string? ParseDay(string? value) =>
        DateTime.TryParseExact(value, Formats, Culture, DateTimeStyles.None, out var day)
            ? day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, Culture, out var number) ? number : null;

    private static int? ParseExceeded(string? value) => value?.ToUpperInvariant() switch
    {
        "TAK" => 1,
        "NIE" => 0,
        _ => null,
    };

    private async Task UpsertAsync(BikeRow row, CancellationToken ct) =>
        await _sql.ExecuteAsync(UpsertSql, row, ct);

    private async Task<int> CountRowsAsync(CancellationToken ct) =>
        await _sql.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [BikeAvailability]", ct: ct);

    private sealed record RawRow(int SourceRow, string? Day, string? Commune, string? Available, string? Required, string? Exceeded);

    private sealed record BikeRow(int SourceRow, string Day, string Commune, int AvailableBikes, int RequiredBikes, int Exceeded);
}
