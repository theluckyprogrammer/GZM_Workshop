using System.Security.Cryptography;
using System.Text;

namespace GzmApp.Data.Import;

// Values: in the order of ScenarioMapping.Imported. IsWeird: at least one field empty or unparsable.
public sealed record ImportRecord(int SourceRow, object?[] Values, byte[] RowHash, bool IsWeird);

// CSV rows → typed records. Rows without a value in skipRowsWhenEmpty (legend, sum under the table) are skipped.
public static class RecordReader
{
    private const char HashSeparator = '\u001F';

    public static IReadOnlyList<ImportRecord> Read(ScenarioMapping mapping, CsvTable table)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(table);

        EnsureHeaders(mapping, table);
        var columns = mapping.Imported
            .Select(c => new ImportColumn(c, SqlType.Parse(c.SqlType!), IndexOf(table, c.Csv)))
            .ToList();
        var skipIndex = mapping.Source.SkipRowsWhenEmpty is { } skip ? IndexOf(table, skip) : -1;
        return table.Rows
            .Where(r => skipIndex < 0 || !string.IsNullOrWhiteSpace(FieldAt(r, skipIndex)))
            .Select(r => ToRecord(r, columns, table.Headers.Count))
            .ToList();
    }

    private static void EnsureHeaders(ScenarioMapping mapping, CsvTable table)
    {
        var expected = mapping.Columns.Select(c => c.Csv).ToList();
        if (!expected.SequenceEqual(table.Headers))
            throw new InvalidDataException(
                $"{mapping.Scenario}: nagłówki pliku [{string.Join(" | ", table.Headers)}] ≠ columns w mapping.json [{string.Join(" | ", expected)}].");
    }

    private static int IndexOf(CsvTable table, string csv) => table.Headers.ToList().IndexOf(csv);

    private static string FieldAt(CsvRow row, int index) => index < row.Fields.Length ? row.Fields[index] : "";

    private static ImportRecord ToRecord(CsvRow row, List<ImportColumn> columns, int headerCount)
    {
        var raw = columns.Select(c => FieldAt(row, c.Index)).ToList();
        var parsed = columns.Select((c, i) => ValueParser.Parse(c.Column, c.Type, raw[i])).ToList();
        var isWeird = row.Fields.Length != headerCount || parsed.Any(p => p.State != ValueState.Ok);
        return new ImportRecord(row.SourceRow, parsed.Select(p => p.Value).ToArray(), Hash(raw), isWeird);
    }

    // RowHash = SHA-256 of all imported values (trimmed) joined with \u001F.
    private static byte[] Hash(IEnumerable<string> raw) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(HashSeparator, raw.Select(r => r.Trim()))));

    private sealed record ImportColumn(ColumnMapping Column, SqlType Type, int Index);
}
