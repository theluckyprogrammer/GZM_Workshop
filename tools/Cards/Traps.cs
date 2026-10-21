using Expect;

namespace Cards;

// Traps that follow from mapping.json (built from data/metadata.json) and checkpoints.md, plus hand-written ones.
public static class Traps
{
    private const string Mssql = "rozszerzenie mssql";

    public static IReadOnlyList<Trap> For(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints, ScenarioText text)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(checkpoints);
        ArgumentNullException.ThrowIfNull(text);

        return ImportTraps.For(mapping, checkpoints)
            .Concat(Search(mapping, checkpoints))
            .Concat(Statistic(mapping, checkpoints))
            .Concat(Favorites(mapping, checkpoints))
            .Concat(text.Traps.Select(t => new Trap(t.Stage, t.Trap, t.Effect, t.Tool)))
            .ToList();
    }

    private static IEnumerable<Trap> Search(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var phrase = checkpoints.Value("E2.phrase");
        if (phrase.Any("ąćęłńóśźż".Contains))
            yield return new(2, $"Fraza z ogonkami: „{phrase}”", "Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`).", "Network: adres żądania; log SQL: wartość parametru `q`");
        yield return new(2, "Wielkość liter i ogonki", $"Porównanie `=` albo LIKE z collation `CS`/`AS` nie znajdzie „{phrase.ToUpperInvariant()}”.", "log SQL: czy zapytanie ma `COLLATE ..._CI_AI`");
        yield return new(2, "Fraza `' OR 1=1 --`", "Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0.", "Network + log SQL");
        foreach (var empty in EmptyCount(checkpoints, mapping.Filter.Field).Where(n => n > 0))
            yield return new(2, $"Puste pole `{mapping.Filter.Field}` w {empty} rekordach", "Te rekordy nie pojawią się w wynikach wyszukiwania – to poprawne.", $"{Mssql}: `SELECT COUNT(*) FROM {mapping.Table} WHERE {mapping.Filter.Field} IS NULL`");
    }

    private static IEnumerable<Trap> Statistic(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var nulls = checkpoints.Value("E3.nulls");
        yield return new(3, "Statystyka liczona z listy wyników", "Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL.", "log SQL: zapytanie statystyki bez `TOP 20`");
        if (mapping.Statistic.Kind == "avg")
            yield return new(3, "AVG na kolumnie liczb całkowitych", "SQL liczy wtedy średnią jako liczbę całkowitą i gubi część po przecinku. Rzutuj na `decimal`.", "log SQL + wartość E3");
        if (mapping.Statistic.Kind is "top5" or "count-by")
            yield return new(3, "Wielkość liter i spacje w grupach", "SQL (collation bez wielkości liter) łączy „Abc” i „abc” w jedną grupę, a `GroupBy` w C# nie. Licz w SQL.", "Watch na liście grup + log SQL");
        if (nulls != "0")
            yield return new(3, $"Puste wartości w polu `{mapping.Statistic.Field}`: {nulls}", "NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie.", $"{Mssql}: `SELECT COUNT(*) FROM {mapping.Table} WHERE {mapping.Statistic.Field} IS NULL`");
    }

    private static IEnumerable<Trap> Favorites(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        foreach (var empty in EmptyCount(checkpoints, mapping.TitleField).Where(n => n > 0))
            yield return new(4, $"Rekordy bez tytułu (`{mapping.TitleField}` puste: {empty})", "Na liście ulubionych pojawi się pusta pozycja. Pokaż np. „(bez nazwy)”.", $"{Mssql}: `SELECT COUNT(*) FROM {mapping.Table} WHERE {mapping.TitleField} IS NULL`");
        yield return new(4, "Dwa kliknięcia ★ w dwóch kartach", "Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze.", $"{Mssql}: `SELECT COUNT(*) FROM Favorites`");
        yield return new(4, "Brak tokenu antiforgery", "Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia.", "Network: nagłówek `RequestVerificationToken`");
    }

    // E1.weird.<Field> = "empty / unparsable".
    public static IEnumerable<int> EmptyCount(IReadOnlyList<Checkpoint> checkpoints, string field)
    {
        ArgumentNullException.ThrowIfNull(checkpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        return checkpoints.Where(c => c.Id == $"E1.weird.{field}").Select(c => int.Parse(c.Value.Split(" / ")[0]));
    }
}
