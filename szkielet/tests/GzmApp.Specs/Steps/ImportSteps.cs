using System.Text.Json;
using System.Text.RegularExpressions;
using GzmApp.Tests.Data;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 1 (sNN-001-importuj-dane): the import at start-up, checked in the database against checkpoints.md
// and against the second CSV parser of GzmApp.Tests (IndependentScenario).
public sealed partial class ImportSteps
{
    [Step("Po starcie aplikacji tabela `{tabela}` ma tyle wierszy, ile poprawnych rekordów jest w pliku.")]
    public async Task RowCountEqualsFile(string table)
    {
        Assert.True(table == ActiveScenario.Table, $"Specyfikacja mówi o tabeli `{table}`, a scenariusz {ActiveScenario.Code} ma tabelę `{ActiveScenario.Table}`.");
        var rows = await Records.CountAsync();
        var expected = ActiveScenario.Data.Value("E1.rows");

        Assert.True(rows.ToString() == expected, $"W tabeli `{table}` jest {rows} wierszy, a poprawnych rekordów w pliku jest {expected}.");
    }

    [Step("Wiersz 7 ma w polu `{pole}` tę samą wartość co w pliku.")]
    public async Task Row7EqualsFile(string field)
    {
        var values = await Records.ColumnTextAsync(ActiveScenario.Column(field));
        var row7 = values.Where(v => v.SourceRow == 7).Select(v => v.Value).ToList();
        var expected = ActiveScenario.Data.Value("E1.row7.value");

        Assert.True(row7.Count == 1, $"W tabeli nie ma dokładnie jednego wiersza z `SourceRow` = 7 (jest {row7.Count}).");
        Assert.True(row7[0] == expected, $"Wiersz 7, pole `{field}`: w bazie „{row7[0] ?? "NULL"}”, a z pliku wychodzi „{expected}”.");
    }

    [Step("Polskie litery z pliku są w bazie bez zmian.")]
    public async Task PolishLettersAreKept()
    {
        Assert.True(await Records.CountAsync() > 0, $"Tabela `{ActiveScenario.Table}` jest pusta – import jeszcze nie działa.");
        var data = new IndependentScenario(ActiveScenario.Data);
        var sample = TextFields(data)
            .SelectMany(f => data.Rows.Where(r => PolishLetter().IsMatch(r.Cells[f].Text ?? string.Empty)).Select(r => (Field: f, Row: r)))
            .FirstOrDefault();
        if (sample.Row is null)
            return;

        var stored = (await Records.ColumnTextAsync(ActiveScenario.Column(sample.Field))).FirstOrDefault(v => v.SourceRow == sample.Row.SourceRow).Value;
        Assert.True(stored == sample.Row.Cells[sample.Field].Text,
            $"Wiersz {sample.Row.SourceRow}, pole `{sample.Field}`: w pliku „{sample.Row.Cells[sample.Field].Text}”, w bazie „{stored}”. Sprawdźcie kodowanie pliku.");
    }

    [Step("Liczby i daty z pliku są w bazie z tą samą wartością.")]
    public async Task NumbersAndDatesEqualFile()
    {
        Assert.True(await Records.CountAsync() > 0, $"Tabela `{ActiveScenario.Table}` jest pusta – import jeszcze nie działa.");
        var data = new IndependentScenario(ActiveScenario.Data);
        foreach (var column in data.Imported.Where(c => IsNumberOrDate(c.GetProperty("sqlType").GetString()!)))
            await AssertColumnAsync(data, column);
    }

    [Step("Puste albo nieczytelne pola są w bazie jako NULL.")]
    public async Task EmptyFieldsAreNull()
    {
        foreach (var column in ActiveScenario.Imported)
        {
            var field = column.GetProperty("field").GetString()!;
            var expected = ActiveScenario.Data.Value($"E1.weird.{field}").Split(" / ").Sum(int.Parse);
            var nulls = (await Records.ColumnTextAsync(column)).Count(v => v.Value is null);
            Assert.True(nulls == expected, $"Pole `{field}`: w bazie {nulls} wartości NULL, a w pliku {expected} pustych albo nieczytelnych.");
        }
    }

    [Step("Kolejne uruchomienie aplikacji nie zmienia liczby wierszy.")]
    public async Task SecondStartKeepsRowCount()
    {
        var before = await Records.CountAsync();
        await using var again = (await ActiveScenario.AppAsync()).Restart();
        await ActiveScenario.StartedOrExplainedAsync(again);
        var after = await Records.CountAsync();

        Assert.True(before == after, $"Po pierwszym starcie w tabeli było {before} wierszy, po drugim – {after}. Import ma się uruchamiać tylko dla pustej tabeli albo przez MERGE.");
    }

    private static async Task AssertColumnAsync(IndependentScenario data, JsonElement column)
    {
        var field = column.GetProperty("field").GetString()!;
        var expected = data.Rows.ToDictionary(r => r.SourceRow, r => r.Cells[field].Text);
        var wrong = (await Records.ColumnTextAsync(column)).FirstOrDefault(v => expected.TryGetValue(v.SourceRow, out var e) && e != v.Value);

        Assert.True(wrong.SourceRow == 0, $"Wiersz {wrong.SourceRow}, pole `{field}`: w bazie „{wrong.Value ?? "NULL"}”, a z pliku wychodzi „{expected.GetValueOrDefault(wrong.SourceRow) ?? "NULL"}”. Sprawdźcie kulturę i format z tabeli kolumn.");
    }

    private static bool IsNumberOrDate(string type) =>
        type.StartsWith("decimal", StringComparison.Ordinal) || type is "date" || type.StartsWith("datetime2", StringComparison.Ordinal) || type.StartsWith("time", StringComparison.Ordinal);

    private static IEnumerable<string> TextFields(IndependentScenario data) =>
        data.Imported.Where(c => c.GetProperty("sqlType").GetString()!.StartsWith("nvarchar", StringComparison.Ordinal)).Select(c => c.GetProperty("field").GetString()!);

    [GeneratedRegex("[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]")]
    private static partial Regex PolishLetter();
}
