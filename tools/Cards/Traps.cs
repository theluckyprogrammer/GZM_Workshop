using Expect;

namespace Cards;

// Traps that follow from mapping.json (built from data/metadata.json) and checkpoints.md, plus hand-written ones.
public static class Traps
{
    private const string Sql = "SQLite Viewer (plik src/GzmApp/gzm.db)";

    public static IReadOnlyList<Trap> For(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints, ScenarioText text)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(checkpoints);
        ArgumentNullException.ThrowIfNull(text);

        return ImportTraps.For(mapping, checkpoints)
            .Concat(Search(mapping, checkpoints))
            .Concat(Statistic(mapping, checkpoints))
            .Concat(Ai())
            .Concat(Favorites(mapping, checkpoints))
            .Concat(text.Traps.Select(t => new Trap(t.Stage, t.Trap, t.Effect, t.Tool)))
            .ToList();
    }

    private static IEnumerable<Trap> Search(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var phrase = checkpoints.Value("E2.phrase");
        if (phrase.Any("ąćęłńóśźż".Contains))
            yield return new(2, $"Fraza z ogonkami: „{phrase}”", "Przy złym kodowaniu albo kulturze wyników jest 0. W adresie fraza jest zakodowana (`%C5%...`).", "Network: adres żądania; log SQL: wartość parametru `q`");
        yield return new(2, "Wielkość liter i ogonki", $"Zwykłe `LIKE` w SQLite pomija wielkość liter tylko w literach bez ogonków – samo „{phrase.ToUpperInvariant()}” znajdzie 0 wyników.", "log SQL: czy zapytanie używa funkcji plain()");
        yield return new(2, "Fraza `' OR 1=1 --`", "Sklejany tekst SQL zwróci wszystkie rekordy. Z parametrem `@q` wyników jest 0.", "Network + log SQL");
        foreach (var empty in EmptyCount(checkpoints, mapping.Filter.Field).Where(n => n > 0))
            yield return new(2, $"Puste pole `{mapping.Filter.Field}` w {empty} rekordach", "Te rekordy nie pojawią się w wynikach wyszukiwania – to poprawne.", $"{Sql}: kolumna {mapping.Filter.Field} – puste komórki mają być NULL");
    }

    private static IEnumerable<Trap> Statistic(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var nulls = checkpoints.Value("E3.nulls");
        yield return new(3, "Statystyka liczona z listy wyników", "Lista pokazuje najwyżej 20 rekordów. Statystyka ma objąć całą tabelę – licz ją w SQL.", "log SQL: zapytanie statystyki bez `TOP 20`");
        if (mapping.Statistic.Kind == "avg")
            yield return new(3, "Średnia z liczb zapisanych jako tekst", "Bez rzutowania SQLite policzy średnią po swojej drodze i wynik nie zgadza się z kartą. Rzutuj: `CAST(pole AS REAL)`.", "log SQL + wartość E3");
        if (mapping.Statistic.Kind is "top5" or "count-by")
            yield return new(3, "Wielkość liter i spacje w grupach", "`GROUP BY` w SQLite rozróżnia wielkość liter („Abc” i „abc” to dwie grupy), a `GroupBy` w C# z ignorowaniem wielkości liter jedną. Licz w SQL.", "Watch na liście grup + log SQL");
        if (nulls != "0")
            yield return new(3, $"Puste wartości w polu `{mapping.Statistic.Field}`: {nulls}", "NULL nie wchodzi do statystyki. Zamiana pustych na 0 albo „” da inny wynik niż na karcie.", $"{Sql}: kolumna {mapping.Statistic.Field} – puste komórki mają być NULL");
    }

    private static IEnumerable<Trap> Ai()
    {
        yield return new(4, "Brak pliku `src/GzmApp/appsettings.Local.json` (klucz do modelu AI)", "Karta pokazuje „Brak klucza do modelu AI – poproś prowadzącego.”, reguła 9 kroku 4 (prawdziwy model) jest czerwona, reguły 1–8 przechodzą (udawany model).", "terminal: wpis kategorii `AI`; skopiuj plik z pendrive'a i uruchom aplikację ponownie");
        yield return new(4, "Model odpowiada kilka sekund", "Bez wskaźnika „AI analizuje…” uczniowie klikają kilka razy – każde kliknięcie to osobne zapytanie (i tokeny).", "terminal: wpisy `AI (…): wysyłam…` i `odpowiedź po … s`");
        yield return new(4, "Model coś zmyśla albo myli numery rekordów", "To nie błąd kodu – tak działa AI. Porównajcie odpowiedź z listą wyników („Co poszło do modelu?”).", "karta analizy: rozwijane „Co poszło do modelu?”");
        yield return new(4, "Odpowiedź modelu wstawiona jako HTML (`@Html.Raw`)", "Model może odesłać znaczniki HTML i zepsuć stronę – reguła 7 jest wtedy czerwona.", "F12 → Elements: karta `#analiza`");
    }

    private static IEnumerable<Trap> Favorites(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        foreach (var empty in EmptyCount(checkpoints, mapping.TitleField).Where(n => n > 0))
            yield return new(5, $"Rekordy bez tytułu (`{mapping.TitleField}` puste: {empty})", "Na liście ulubionych pojawi się pusta pozycja. Pokaż np. „(bez nazwy)”.", $"{Sql}: kolumna {mapping.TitleField} – puste komórki mają być NULL");
        yield return new(5, "Dwa kliknięcia ★ w dwóch kartach", "Bez klucza `PK_Favorites` w tabeli są dwa takie same wiersze.", $"{Sql}: tabela Favorites");
        yield return new(5, "Brak tokenu antiforgery", "Żądanie POST/DELETE dostaje status 400, a handler się nie uruchamia.", "Network: nagłówek `RequestVerificationToken`");
    }

    // E1.weird.<Field> = "empty / unparsable".
    public static IEnumerable<int> EmptyCount(IReadOnlyList<Checkpoint> checkpoints, string field)
    {
        ArgumentNullException.ThrowIfNull(checkpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        return checkpoints.Where(c => c.Id == $"E1.weird.{field}").Select(c => int.Parse(c.Value.Split(" / ")[0]));
    }
}
