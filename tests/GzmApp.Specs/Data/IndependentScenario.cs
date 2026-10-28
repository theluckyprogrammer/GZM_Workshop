using System.Globalization;
using System.Text.Json;
using GzmApp.Specs.Infrastructure;

namespace GzmApp.Specs.Data;

public sealed record Row(int SourceRow, IReadOnlyDictionary<string, Cell> Cells, bool WrongFieldCount)
{
    public bool Weird => WrongFieldCount || Cells.Values.Any(c => c.Text is null);
}

// The values of checkpoints.md computed a second time from data/raw, without tools/Expect and without CsvHelper.
public sealed class IndependentScenario
{
    private static readonly CompareInfo Polish = CultureInfo.GetCultureInfo("pl-PL").CompareInfo;

    public IndependentScenario(Scenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        Scenario = scenario;
        var source = scenario.Mapping.GetProperty("source");
        var text = RawCsv.Decode(File.ReadAllBytes(Path.Combine(RepoPaths.Root, source.GetProperty("file").GetString()!)));
        var records = RawCsv.Records(text, source.GetProperty("separator").GetString()![0], source.GetProperty("headerLine").GetInt32());
        Headers = records[0];
        FileRows = records.Count - 1;
        Rows = Deduplicate(records.Skip(1).Select((r, i) => ToRow(r, i + 1)).Where(r => r is not null).Select(r => r!).ToList());
    }

    public Scenario Scenario { get; }

    public IReadOnlyList<string> Headers { get; }

    public int FileRows { get; }

    public IReadOnlyList<Row> Rows { get; }

    public IEnumerable<JsonElement> Imported =>
        Scenario.Mapping.GetProperty("columns").EnumerateArray().Where(c => !c.TryGetProperty("import", out var i) || i.GetBoolean());

    public Row Row7 => Rows.Single(r => r.SourceRow == 7);

    public IReadOnlyList<Row> Matching(string phrase, string field) =>
        Rows.Where(r => r.Cells[field].Text is { } t && Polish.IndexOf(t, phrase, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0).ToList();

    private Row? ToRow(List<string> fields, int number)
    {
        var skip = Scenario.Mapping.GetProperty("source").GetProperty("skipRowsWhenEmpty").GetString();
        if (skip is not null && string.IsNullOrWhiteSpace(Field(fields, skip)))
            return null;
        var cells = Imported.ToDictionary(c => c.GetProperty("field").GetString()!, c => IndependentValues.Read(c, Field(fields, c.GetProperty("csv").GetString()!)));
        return new Row(number, cells, fields.Count != Headers.Count);
    }

    private string Field(List<string> fields, string csv)
    {
        var index = Headers.ToList().IndexOf(csv);
        return index < fields.Count ? fields[index] : string.Empty;
    }

    // Upsert on the key: the first row of each key stays.
    private List<Row> Deduplicate(List<Row> rows)
    {
        var key = Scenario.Mapping.GetProperty("key");
        var fields = key.GetProperty("fields").EnumerateArray().Select(f => f.GetString()!).ToList();
        Func<Row, string> keyOf = key.GetProperty("kind").GetString() == "RowHash"
            ? r => string.Join('\u001F', r.Cells.Values.Select(c => c.Raw.Trim()))
            : r => string.Join('\u001F', fields.Select(f => r.Cells[f].Text));
        return rows.GroupBy(keyOf).Select(g => g.First()).ToList();
    }
}
