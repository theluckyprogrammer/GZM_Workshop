using Expect;

namespace Cards;

// The five decisions of prompty.md (decision 5 belongs to the extra step "favorites" in "Co dalej?"). Each variant is one rule sentence of the specification (scenarios/sNN/spec/*.md):
// variant A is already in the spec file, another variant replaces that rule. Every sentence has a [Step] in tests/GzmApp.Specs.
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
            Letters(text.ListFields.Select(o => (o.Label, $"Przy każdym rekordzie lista pokazuje tytuł `{mapping.TitleField}` i pola: {Fields(o.Fields)}."))),
            Letters(text.Stats.Select(o => (o.Label, StatLine(o)))),
            Letters(AiTasks(text)),
            Letters(FavoriteViews()),
        ];
    }

    private static string SearchLine(ScenarioMapping mapping, SearchOption option)
    {
        var csv = mapping.Imported.First(c => c.Field == option.Field).Csv;
        return option.Match == "prefix"
            ? $"Wyszukiwarka pokazuje rekordy, w których pole `{option.Field}` (kolumna w pliku: `{csv}`) zaczyna się od frazy."
            : $"Wyszukiwarka szuka frazy w dowolnym miejscu pola `{option.Field}` (kolumna w pliku: `{csv}`).";
    }

    private static string StatLine(StatOption option) => option.Kind switch
    {
        "sum" => $"Kafelek ze statystyką pokazuje sumę pola `{option.Field}`.",
        "avg" => $"Kafelek ze statystyką pokazuje średnią pola `{option.Field}` z 2 miejscami po przecinku.",
        "top5" => $"Kafelek ze statystyką pokazuje 5 najczęstszych wartości pola `{option.Field}` z liczbą rekordów.",
        _ => $"Kafelek ze statystyką pokazuje liczbę rekordów dla każdej wartości pola `{option.Field}`.",
    };

    // Variant A fits the data set; B and C work for every data set.
    private static IEnumerable<(string, string)> AiTasks(ScenarioText text) =>
    [
        (text.AiLabel, AiLine(text.AiTask)),
        ("3 ciekawostki", AiLine("Znajdź w tych rekordach 3 ciekawostki i krótko je opisz.")),
        ("kontrola jakości danych", AiLine("Sprawdź jakość tych danych: wskaż puste pola, literówki i różne zapisy tej samej wartości.")),
    ];

    public static string AiLine(string task)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);

        return $"Model dostaje zadanie: {task}";
    }

    private static IEnumerable<(string, string)> FavoriteViews() =>
    [
        ("lista tytułów", "Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów."),
        ("tytuły z datą dodania", "Strona „Moje ulubione” pokazuje tytuł i datę dodania każdego rekordu, od najnowszych."),
        ("tytuły z przyciskiem „Usuń”", "Strona „Moje ulubione” pokazuje tytuł i przycisk „Usuń”, który usuwa rekord z listy bez przeładowania strony."),
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
        Ensure(mapping, mapping.ListFields is { } list && text.ListFields[0].Fields.SequenceEqual(list), "wariant A listy ≠ listFields z mapping.json");
        Ensure(mapping, text.Stats.Where(o => o.Kind is "sum" or "avg").All(o => NumericTypes.Any(columns[o.Field].StartsWith)), "sum/avg na polu nieliczbowym");
    }

    private static void Ensure(ScenarioMapping mapping, bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException($"{mapping.Scenario} (scenarios.json): {message}.");
    }
}
