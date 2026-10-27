using Expect;

namespace Cards;

// Values for {{name}} in the templates. Checkpoint values are rendered by TemplateRenderer ({{cp:ID}}).
public static class Variables
{
    public static Dictionary<string, string> For(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints, ScenarioText text)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(checkpoints);
        ArgumentNullException.ThrowIfNull(text);

        var variables = Basic(mapping, checkpoints, text);
        foreach (var index in Enumerable.Range(1, 5))
            variables[$"rule{index}"] = Decisions.Rule(index, mapping, text);
        return variables;
    }

    private static Dictionary<string, string> Basic(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints, ScenarioText text) =>
        new()
        {
            ["code"] = mapping.Scenario, ["app"] = text.App, ["idea"] = text.Idea, ["favorites"] = text.Favorites,
            ["aiTask"] = text.AiTask, ["dataset"] = mapping.Dataset, ["attribution"] = mapping.Attribution,
            ["sourceUrl"] = mapping.SourceUrl, ["file"] = mapping.Source.File, ["encoding"] = mapping.Source.Encoding,
            ["separatorText"] = SeparatorText(mapping.Source.Separator),
            ["separatorChar"] = mapping.Source.Separator,
            ["headerText"] = HeaderText(mapping.Source.HeaderLine),
            ["table"] = mapping.Table, ["columnsTable"] = ColumnsTable(mapping),
            ["dataNotes"] = DataNotes(mapping, checkpoints), ["createTable"] = SqlText.CreateTable(mapping),
            ["checkField"] = mapping.CheckField, ["checkCsv"] = CsvName(mapping, mapping.CheckField),
            ["filterField"] = mapping.Filter.Field, ["filterCsv"] = CsvName(mapping, mapping.Filter.Field),
            ["titleField"] = mapping.TitleField, ["phrase"] = checkpoints.Value("E2.phrase"),
            ["statDescription"] = SqlText.Description(mapping.Statistic.Kind, mapping.Statistic.Field),
            ["aiFields"] = string.Join(", ", mapping.Imported.Select(c => $"`{c.Field}`")),
            ["statQueries"] = StatQueries(mapping.Table),
            ["fieldLabels"] = string.Join(", ", mapping.Imported.Select(c => $"`{c.Field}` – {c.Label ?? c.Field}")),
        };

    // All four statistic queries with [Pole] in place of the field: the spec rule says which one and on which field.
    private static string StatQueries(string table) =>
        string.Join("\n", new[] { ("suma", "sum"), ("średnia", "avg"), ("5 najczęstszych wartości", "top5"), ("liczba rekordów dla każdej wartości", "count-by") }
            .Select(k => $"  - {k.Item1}: `{SqlText.Statistic(k.Item2, "Pole", table)}`"));

    private static string SeparatorText(string separator) => separator switch
    {
        "," => "przecinek `,`",
        ";" => "średnik `;`",
        _ => $"`{separator}`",
    };

    private static string HeaderText(int line) => line == 1
        ? "Nagłówek w pierwszej linii."
        : $"Nagłówek dopiero w linii {line}: linie 1–{line - 1} to tytuł, pomiń je.";

    private static string ColumnsTable(ScenarioMapping mapping) =>
        "| Kolumna w pliku | Pole w tabeli | Typ SQL | Kultura | Format | Może być puste |\n|---|---|---|---|---|---|\n"
        + string.Join("\n", mapping.Imported.Select(c =>
            $"| `{c.Csv}` | `{c.Field}` | `{c.SqlType}` | {c.Culture} | {FormatText(c)} | {(c.Nullable ? "tak" : "nie")} |"));

    private static string FormatText(ColumnMapping column) => column switch
    {
        { Format: null } => "–",
        { Format: "unix-ms" } => "milisekundy od 1970-01-01 UTC",
        { SqlType: "bit" } => $"„{column.Format.Split('|')[0]}” = 1, „{column.Format.Split('|')[1]}” = 0",
        _ => string.Join(" albo ", column.Format.Split('|').Select(f => $"`{f}`")),
    };

    private static string DataNotes(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var notes = new List<string> { KeyNote(mapping) };
        if (mapping.Source.Encoding == "Windows-1250")
            notes.Add("- Windows-1250 wymaga na początku `AddAppServices` wywołania `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)`.");
        if (mapping.Source.SkipRowsWhenEmpty is { } column)
            notes.Add($"- Pomiń wiersze, w których kolumna `{column}` jest pusta: to legenda i suma pod tabelą (wiersze {checkpoints.Value("E1.skipped")}).");
        if (checkpoints.Value("E1.duplicates") != "brak")
            notes.Add($"- W pliku są identyczne wiersze (drugie z par: {checkpoints.Value("E1.duplicates")}). Wstaw tylko pierwszy z nich – pomiń wiersz, którego `RowHash` już jest w tabeli.");
        return string.Join("\n", notes);
    }

    private static string KeyNote(ScenarioMapping mapping) => mapping.Key.IsRowHash
        ? "- Klucz: `RowHash` = SHA-256 z wartości wszystkich importowanych pól (po `Trim`) połączonych znakiem `\\u001F`, zapisany jako `binary(32)`."
        : $"- Klucz naturalny (unikalny): {string.Join(" + ", mapping.Key.Fields.Select(f => $"`{f}`"))}. Wiersze zapisuj przez `MERGE` po kluczu: powtórzony klucz nadpisuje wcześniejszy wiersz, bez wyjątku.";

    private static string CsvName(ScenarioMapping mapping, string field) => mapping.Imported.First(c => c.Field == field).Csv;
}
