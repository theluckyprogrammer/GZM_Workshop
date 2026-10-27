namespace Expect;

public sealed record DataRecord(int SourceRow, IReadOnlyDictionary<string, ParsedValue> Values, bool WrongFieldCount)
{
    public ParsedValue this[string field] => Values[field];

    public bool IsWeird => WrongFieldCount || Values.Values.Any(v => v.State != ValueState.Ok);
}

// Records: rows that end up in the table (after skipping and after the upsert on the key).
public sealed record RecordSet(IReadOnlyList<DataRecord> Records, int FileRows, IReadOnlyList<int> SkippedRows, IReadOnlyList<int> DuplicateRows)
{
    public DataRecord Row(int sourceRow) =>
        Records.FirstOrDefault(r => r.SourceRow == sourceRow)
        ?? throw new InvalidDataException($"Wiersz {sourceRow} nie trafia do tabeli (pominięty albo duplikat).");
}

public static class RecordBuilder
{
    private const char KeySeparator = '\u001F';

    public static RecordSet Build(ScenarioMapping mapping, SourceTable table)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(table);

        EnsureHeaders(mapping, table);
        var columns = mapping.Imported.Select(c => (Column: c, Type: SqlType.Parse(c.SqlType!), Index: IndexOf(table, c.Csv))).ToList();
        var kept = table.Rows.Where(r => !IsSkipped(mapping, table, r)).ToList();
        var records = kept.Select(r => ToRecord(r, columns, table.Headers.Count)).ToList();
        var groups = records.GroupBy(r => Key(mapping, r)).ToList();
        var unique = groups.Select(g => g.First()).ToList();
        var duplicates = groups.SelectMany(g => g.Skip(1)).Select(r => r.SourceRow).ToList();
        var skipped = table.Rows.Except(kept).Select(r => r.Number).ToList();
        return new RecordSet(unique, table.Rows.Count, skipped, duplicates);
    }

    private static void EnsureHeaders(ScenarioMapping mapping, SourceTable table)
    {
        var expected = mapping.Columns.Select(c => c.Csv).ToList();
        if (!expected.SequenceEqual(table.Headers))
            throw new InvalidDataException(
                $"{mapping.Scenario}: nagłówki pliku [{string.Join(" | ", table.Headers)}] ≠ columns w mapping.json [{string.Join(" | ", expected)}].");
    }

    private static int IndexOf(SourceTable table, string csv) => table.Headers.ToList().IndexOf(csv);

    // skipRowsWhenEmpty: rows without a value in this column are notes under the table (legend, sum).
    private static bool IsSkipped(ScenarioMapping mapping, SourceTable table, SourceRow row)
    {
        var column = mapping.Source.SkipRowsWhenEmpty;
        if (column is null)
            return false;
        var index = IndexOf(table, column);
        return index >= row.Fields.Length || string.IsNullOrWhiteSpace(row.Fields[index]);
    }

    private static DataRecord ToRecord(SourceRow row, List<(ColumnMapping Column, SqlType Type, int Index)> columns, int headerCount)
    {
        var values = columns.ToDictionary(
            c => c.Column.Field!,
            c => ValueParser.Parse(c.Column, c.Type, c.Index < row.Fields.Length ? row.Fields[c.Index] : ""));
        return new DataRecord(row.Number, values, row.Fields.Length != headerCount);
    }

    // Natural key: parsed key values. RowHash: all imported values, trimmed.
    private static string Key(ScenarioMapping mapping, DataRecord record)
    {
        if (mapping.Key.IsRowHash)
            return string.Join(KeySeparator, record.Values.Values.Select(v => v.Raw.Trim()));

        var parts = mapping.Key.Fields.Select(f => record[f]).ToList();
        if (parts.Any(p => p.State != ValueState.Ok))
            throw new InvalidDataException($"{mapping.Scenario}: wiersz {record.SourceRow} ma pusty albo błędny klucz.");
        return string.Join(KeySeparator, parts.Select(p => p.Display));
    }
}
