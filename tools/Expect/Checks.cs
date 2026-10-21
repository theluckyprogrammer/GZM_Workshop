using System.Globalization;
using System.Text.RegularExpressions;

namespace Expect;

// One control value: Id (e.g. E1.rows), what to check, the value.
public sealed record Checkpoint(string Id, string Label, string Value);

public static partial class Checks
{
    public const int SampleRow = 7;
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");

    public static IEnumerable<Checkpoint> Import(ScenarioMapping mapping, RecordSet set)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(set);

        var sample = set.Row(SampleRow)[mapping.CheckField];
        if (sample.State != ValueState.Ok)
            throw new InvalidDataException($"Wiersz 7: pole kontrolne {mapping.CheckField} jest puste albo nieparsowalne. Wybierz inne checkField.");
        var weird = set.Records.Where(r => r.IsWeird).ToList();
        yield return new("E1.rows", "Liczba wierszy w tabeli po imporcie", Number(set.Records.Count));
        yield return new("E1.fileRows", "Wiersze danych w pliku (bez nagłówka)", Number(set.FileRows));
        yield return new("E1.skipped", "Pominięte wiersze (notatki pod tabelą)", Rows(set.SkippedRows));
        yield return new("E1.duplicates", "Duplikaty klucza (scalone przez MERGE)", Rows(set.DuplicateRows));
        yield return new("E1.row7.raw", $"Wiersz 7, kolumna CSV `{CsvName(mapping, mapping.CheckField)}` – tekst w pliku", sample.Raw);
        yield return new("E1.row7.value", $"Wiersz 7, pole `{mapping.CheckField}` – wartość w bazie", sample.Display);
        yield return new("E1.weird", "„Dziwne” rekordy (co najmniej jedno pole puste albo nieparsowalne)", Number(weird.Count));
        yield return new("E1.weird.rows", "Pierwsze „dziwne” rekordy (SourceRow)", Rows(weird.Select(r => r.SourceRow).Take(5).ToList()));
        foreach (var column in mapping.Imported)
            yield return WeirdColumn(column, set);
        yield return new("E1.wrongFieldCount", "Wiersze z inną liczbą pól niż nagłówek", Number(set.Records.Count(r => r.WrongFieldCount)));
    }

    public static IEnumerable<Checkpoint> Filter(ScenarioMapping mapping, RecordSet set)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(set);

        var (phrase, matches) = FindPhrase(mapping.Filter.Field, set.Records);
        yield return new("E2.phrase", $"Fraza wpisana w wyszukiwarkę (pole `{mapping.Filter.Field}`)", phrase);
        yield return new("E2.count", "Liczba wyników", Number(matches.Count));
        foreach (var (match, index) in matches.Select((m, i) => (m, i + 1)))
            yield return new($"E2.result.{index}", $"Wynik {index}: SourceRow {match.SourceRow}", match[mapping.TitleField].Display);
    }

    public static IEnumerable<Checkpoint> Favorite(ScenarioMapping mapping, RecordSet set)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(set);

        var title = set.Row(SampleRow)[mapping.TitleField];
        if (title.State != ValueState.Ok)
            throw new InvalidDataException($"{mapping.Scenario}: wiersz 7 nie ma tytułu w polu {mapping.TitleField}.");
        yield return new("E4.title", $"Tytuł rekordu z wiersza 7 (pole `{mapping.TitleField}`)", title.Display);
    }

    private static Checkpoint WeirdColumn(ColumnMapping column, RecordSet set)
    {
        var values = set.Records.Select(r => r[column.Field!]).ToList();
        var empty = values.Count(v => v.State == ValueState.Empty);
        var bad = values.Count(v => v.State == ValueState.Unparsable);
        if (!column.Nullable && empty + bad > 0)
            throw new InvalidDataException($"Pole {column.Field}: {empty} pustych i {bad} nieparsowalnych, a nullable = false.");
        return new($"E1.weird.{column.Field}", $"Pole `{column.Field}`: puste / nieparsowalne", $"{empty} / {bad}");
    }

    // Phrase: a word (≥ 4 letters or digits) from the filter field that gives 2–10 results.
    // Order: Polish letter first, then a result count closest to 5, then alphabetical.
    private static (string Phrase, List<DataRecord> Matches) FindPhrase(string field, IReadOnlyList<DataRecord> records)
    {
        var words = records.Select(r => r[field]).Where(v => v.State == ValueState.Ok)
            .SelectMany(v => Word().Matches(v.Display).Select(m => m.Value.ToLower(Polish)))
            .Distinct().Order(StringComparer.Ordinal);
        var candidates = words.Select(w => (Phrase: w, Matches: records.Where(r => Contains(r[field], w)).ToList()))
            .Where(c => c.Matches.Count is >= 2 and <= 10);
        var best = candidates
            .OrderByDescending(c => c.Phrase.Any(PolishLetters.Contains))
            .ThenBy(c => Math.Abs(c.Matches.Count - 5))
            .FirstOrDefault();
        return best.Phrase is null ? throw new InvalidDataException($"Pole {field}: brak frazy dającej 2–10 wyników.") : best;
    }

    // Like SQL LIKE '%phrase%' with a collation that ignores case and accents (CI_AI).
    public static bool Contains(ParsedValue value, string phrase) =>
        value.State == ValueState.Ok && Polish.CompareInfo.IndexOf(value.Display, phrase, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private const string PolishLetters = "ąćęłńóśźż";

    public static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Rows(IReadOnlyList<int> rows) =>
        rows.Count == 0 ? "brak" : string.Join(", ", rows.Select(r => r.ToString(CultureInfo.InvariantCulture)));

    private static string CsvName(ScenarioMapping mapping, string field) => mapping.Imported.First(c => c.Field == field).Csv;

    [GeneratedRegex(@"[\p{L}\p{Nd}]{4,}")]
    private static partial Regex Word();
}
