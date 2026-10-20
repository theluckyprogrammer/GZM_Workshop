using System.Globalization;
using System.Text.Json;

namespace Expect;

public sealed record SourceMapping(string File, string Encoding, string Separator, int HeaderLine, string? SkipRowsWhenEmpty);

// Import = false: the column exists in the file but is not loaded into the table.
public sealed record ColumnMapping(
    string Csv, string? Field = null, string? SqlType = null, string? Culture = null,
    string? Format = null, bool Nullable = false, bool Import = true)
{
    public CultureInfo CultureInfo =>
        Culture == "invariant" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(Culture!);
}

public sealed record KeyMapping(string Kind, IReadOnlyList<string> Fields)
{
    public bool IsRowHash => Kind == "RowHash";
}

public sealed record FilterMapping(string Field);

public sealed record StatisticMapping(string Kind, string Field);

public sealed record ScenarioMapping(
    string Scenario, string Dataset, SourceMapping Source, string Table, IReadOnlyList<ColumnMapping> Columns,
    KeyMapping Key, FilterMapping Filter, StatisticMapping Statistic, string TitleField, string CheckField,
    string Attribution, string SourceUrl)
{
    public IReadOnlyList<ColumnMapping> Imported => Columns.Where(c => c.Import).ToList();
}

public static class MappingLoader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
    private static readonly string[] StatisticKinds = ["top5", "sum", "avg", "count-by"];
    private static readonly string[] Cultures = ["pl-PL", "invariant"];

    public static ScenarioMapping Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var mapping = JsonSerializer.Deserialize<ScenarioMapping>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException($"{path}: pusty plik.");
        Validate(mapping);
        return mapping;
    }

    private static void Validate(ScenarioMapping mapping)
    {
        var fields = mapping.Imported.Select(c => c.Field ?? "").ToList();
        var referenced = mapping.Key.Fields.Concat([mapping.Filter.Field, mapping.Statistic.Field, mapping.TitleField, mapping.CheckField]);
        Ensure(mapping, fields.Count == fields.Distinct().Count(), "powtórzona nazwa pola");
        Ensure(mapping, mapping.Imported.All(IsComplete), "importowana kolumna bez field/sqlType/culture");
        Ensure(mapping, referenced.All(fields.Contains), "odwołanie do pola, którego nie ma w columns");
        Ensure(mapping, StatisticKinds.Contains(mapping.Statistic.Kind), $"statystyka musi być jedną z: {string.Join(", ", StatisticKinds)}");
        Ensure(mapping, mapping.Key.IsRowHash || (mapping.Key.Kind == "natural" && mapping.Key.Fields.Count > 0), "klucz: natural z polami albo RowHash");
    }

    private static bool IsComplete(ColumnMapping column) =>
        !string.IsNullOrWhiteSpace(column.Field) && !string.IsNullOrWhiteSpace(column.SqlType) && Cultures.Contains(column.Culture);

    private static void Ensure(ScenarioMapping mapping, bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException($"{mapping.Scenario}/mapping.json: {message}.");
    }
}
