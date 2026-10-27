using Expect;

namespace Cards;

// Stage 1 traps: file format, cultures, date formats, empty values, rows that do not go to the table.
public static class ImportTraps
{
    private const string Watch = "breakpoint w imporcie + Watch";

    public static IEnumerable<Trap> For(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(checkpoints);

        return FileTraps(mapping, checkpoints)
            .Concat(mapping.Imported.SelectMany(Column))
            .Concat(Empty(mapping, checkpoints));
    }

    private static IEnumerable<Trap> FileTraps(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var source = mapping.Source;
        yield return source.Encoding == "Windows-1250"
            ? new(1, "Kodowanie Windows-1250", "Czytany jako UTF-8 plik zamienia ogonki w „�”.", $"{Watch} na tekście z ogonkiem; SQLite Viewer: tabela {mapping.Table}, pierwsze wiersze")
            : new(1, $"Kodowanie {source.Encoding}", "Plik jest dobry, ale zapisany ponownie w Excelu dostanie Windows-1250 i ogonki się zepsują. Nie zapisuj go w Excelu.", $"SQLite Viewer: tabela {mapping.Table}, pierwsze wiersze");
        if (source.Separator != ",")
            yield return new(1, $"Separator to `{source.Separator}`, nie przecinek", "Parser ustawiony na przecinek widzi jedną kolumnę w każdym wierszu.", $"{Watch} na liczbie pól w wierszu");
        if (source.HeaderLine > 1)
            yield return new(1, $"Nagłówek dopiero w linii {source.HeaderLine}", "Linie tytułu zostaną wzięte za nagłówek i nazwy kolumn się nie zgodzą.", $"{Watch} na nazwach kolumn");
        if (source.SkipRowsWhenEmpty is not null)
            yield return new(1, $"Notatki pod tabelą (wiersze {checkpoints.Value("E1.skipped")})", $"Bez pomijania w tabeli będzie {checkpoints.Value("E1.fileRows")} zamiast {checkpoints.Value("E1.rows")} wierszy.", "SQLite Viewer: liczba wierszy tabeli i wartość E1.rows");
        if (checkpoints.Value("E1.duplicates") != "brak")
            yield return new(1, $"Identyczne wiersze w pliku (drugie z par: {checkpoints.Value("E1.duplicates")})", "Drugi taki sam wiersz łamie unikalny `RowHash` i import przerywa się błędem.", "komunikat błędu w terminalu + log SQL");
        if (mapping.Columns.Any(c => !c.Import))
            yield return new(1, $"Kolumny, których nie importujemy: {mapping.Columns.Count(c => !c.Import)}", "Nie dodawaj dla nich pól – tabela jest gotowa w prompcie.", "SQLite Viewer: lista kolumn tabeli");
    }

    private static IEnumerable<Trap> Column(ColumnMapping column)
    {
        var name = $"`{column.Csv}`";
        if (column.Culture == "invariant" && column.SqlType!.StartsWith("decimal", StringComparison.Ordinal))
            yield return new(1, $"Kropka dziesiętna w {name}", "Z kulturą pl-PL (przecinek) liczba się nie odczyta albo straci kropkę.", $"{Watch} na wartości po odczycie");
        if (column.Format == "unix-ms")
            yield return new(1, $"Daty w {name} jako milisekundy od 1970 r.", "Bez przeliczenia w bazie będzie liczba albo błąd typu. Przeliczaj w UTC, inaczej data przesunie się o dzień.", "SQLite Viewer: wiersz z SourceRow = 7 i wartość E1.row7.value");
        if (column.Format?.Contains("'Z'", StringComparison.Ordinal) == true)
            yield return new(1, $"Czas w {name} jest w UTC (litera Z)", "Godziny są o 1–2 h wcześniejsze niż czas polski. Nie przeliczaj – tak jest w danych.", $"{Watch} na wartości `DateTime`");
        if (column.Format?.Contains('|') == true && column.SqlType != "bit")
            yield return new(1, $"Dwa zapisy w {name}: `{column.Format.Replace("|", "` i `")}`", "Jeden wzorzec odrzuci część wartości – wzrośnie liczba „dziwnych” rekordów.", "log importu: liczba „dziwnych” vs wartość E1.weird");
        if (column.SqlType == "bit" && column.Format != "true|false")
            yield return new(1, $"{name} zawiera słowa „{column.Format!.Replace("|", "”/„")}”", "`bool.Parse` ich nie przyjmie – zna tylko true/false.", $"{Watch} na wartości kolumny");
    }

    private static IEnumerable<Trap> Empty(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints) =>
        mapping.Imported
            .Select(c => (Column: c, Empty: Traps.EmptyCount(checkpoints, c.Field!).Single()))
            .Where(c => c.Empty > 0)
            .Select(c => new Trap(1, $"Puste pola w `{c.Column.Csv}`: {c.Empty}", "Muszą trafić do bazy jako NULL, nie jako pusty tekst ani 0.", $"SQLite Viewer: kolumna {c.Column.Field} – puste komórki mają być NULL"));
}
