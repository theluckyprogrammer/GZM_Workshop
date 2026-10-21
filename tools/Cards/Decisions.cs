using Expect;

namespace Cards;

// The four decisions of prompty.md. Each variant is one line the pair pastes into the next prompt instead of [DECYZJA n].
public sealed record Variant(string Letter, string Label, string Line);

public static class Decisions
{
    private static readonly string[] NumericTypes = ["int", "bigint", "decimal"];

    public static IReadOnlyList<IReadOnlyList<Variant>> For(ScenarioMapping mapping, ScenarioText text)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(text);

        Validate(mapping, text);
        return
        [
            Letters(text.Search.Select(o => (o.Label, SearchLine(mapping, o)))),
            Letters(text.ListFields.Select(o => (o.Label, $"Na liście przy każdym rekordzie pokaż tytuł `{mapping.TitleField}` oraz: {Fields(o.Fields)}."))),
            Letters(text.Stats.Select(o => (o.Label, StatLine(mapping, o)))),
            Letters(FavoriteViews(mapping)),
        ];
    }

    private static string SearchLine(ScenarioMapping mapping, SearchOption option) =>
        $"Szukaj w polu `{option.Field}` (kolumna w pliku: `{mapping.Imported.First(c => c.Field == option.Field).Csv}`), "
        + (option.Match == "prefix" ? "wartość ma się zaczynać od frazy: `LIKE @q + '%'`." : "fraza w dowolnym miejscu: `LIKE '%' + @q + '%'`.");

    private static string StatLine(ScenarioMapping mapping, StatOption option) =>
        $"Kafelek: {SqlText.Description(option.Kind, option.Field)} Zapytanie: `{SqlText.Statistic(option.Kind, option.Field, mapping.Table)}`";

    private static IEnumerable<(string, string)> FavoriteViews(ScenarioMapping mapping) =>
    [
        ("lista tytułów", $"Na stronie „Moje ulubione” pokaż listę tytułów (`{mapping.TitleField}`) ulubionych rekordów."),
        ("tytuły z datą dodania", $"Na stronie „Moje ulubione” pokaż tytuł (`{mapping.TitleField}`) i datę dodania (`CreatedAt`), od najnowszych."),
        ("tytuły z przyciskiem „Usuń”", $"Na stronie „Moje ulubione” pokaż tytuł (`{mapping.TitleField}`) i przycisk „Usuń” (`hx-delete`), który usuwa wiersz z listy bez przeładowania strony."),
    ];

    private static List<Variant> Letters(IEnumerable<(string Label, string Line)> options) =>
        options.Select((o, i) => new Variant(((char)('A' + i)).ToString(), o.Label, o.Line)).ToList();

    private static string Fields(IEnumerable<string> fields) => string.Join(", ", fields.Select(f => $"`{f}`"));

    // Variant A must match mapping.json (the rescue branches and checkpoints.md use it).
    private static void Validate(ScenarioMapping mapping, ScenarioText text)
    {
        var columns = mapping.Imported.ToDictionary(c => c.Field!, c => c.SqlType!);
        var fields = text.Search.Select(o => o.Field).Concat(text.ListFields.SelectMany(o => o.Fields)).Concat(text.Stats.Select(o => o.Field));
        Ensure(mapping, fields.All(columns.ContainsKey), "pole spoza mapping.json");
        Ensure(mapping, text.Search.Count >= 2 && text.ListFields.Count >= 2 && text.Stats.Count >= 2, "każda decyzja potrzebuje co najmniej 2 wariantów");
        Ensure(mapping, text.Search[0] is { Match: "contains" } first && first.Field == mapping.Filter.Field, "wariant A wyszukiwania ≠ filter z mapping.json");
        Ensure(mapping, text.Stats[0].Kind == mapping.Statistic.Kind && text.Stats[0].Field == mapping.Statistic.Field, "wariant A statystyki ≠ statistic z mapping.json");
        Ensure(mapping, text.Stats.Where(o => o.Kind is "sum" or "avg").All(o => NumericTypes.Any(columns[o.Field].StartsWith)), "sum/avg na polu nieliczbowym");
    }

    private static void Ensure(ScenarioMapping mapping, bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException($"{mapping.Scenario} (scenarios.json): {message}.");
    }
}
