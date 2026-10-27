using Expect;

namespace Cards;

// The default rule sentence of each of the five decisions (variant A in scenarios.json: what mapping.json,
// the rescue branches and checkpoints.md use). The variants are no longer printed in karta.md –
// in the "decyzja" pauses the pair asks Kilo, which proposes sentences dynamically, following the
// sentence patterns embedded in tools/Cards/templates/karta.md and looking at the pair's data
// and LOG-DECYZJI.md. SpecCoverageTests in szkielet/tests checks that every pattern has a step.
public static class Decisions
{
    private static readonly string[] NumericTypes = ["int", "bigint", "decimal"];

    public static string Rule(int index, ScenarioMapping mapping, ScenarioText text)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(text);

        Validate(mapping, text);
        return index switch
        {
            1 => SearchLine(mapping, text.Search[0]),
            2 => $"Przy każdym rekordzie lista pokazuje tytuł `{mapping.TitleField}` i pola: {Fields(text.ListFields[0].Fields)}.",
            3 => StatLine(text.Stats[0]),
            4 => AiLine(text.AiTask),
            5 => "Strona „Moje ulubione” pokazuje tytuły ulubionych rekordów.",
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Nieznana decyzja."),
        };
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

    public static string AiLine(string task)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);

        return $"Model dostaje zadanie: {task}";
    }

    private static string Fields(IEnumerable<string> fields) => string.Join(", ", fields.Select(f => $"`{f}`"));

    // The default sentence (index 1) must match mapping.json (the rescue branches and checkpoints.md use it).
    private static void Validate(ScenarioMapping mapping, ScenarioText text)
    {
        var columns = mapping.Imported.ToDictionary(c => c.Field!, c => c.SqlType!);
        var fields = text.Search.Select(o => o.Field).Concat(text.ListFields.SelectMany(o => o.Fields)).Concat(text.Stats.Select(o => o.Field));
        Ensure(mapping, fields.All(columns.ContainsKey), "pole spoza mapping.json");
        Ensure(mapping, text.Search.Count >= 2 && text.ListFields.Count >= 2 && text.Stats.Count >= 2, "każda decyzja potrzebuje co najmniej 2 propozycji do wyboru");
        Ensure(mapping, text.Search[0] is { Match: "contains" } first && first.Field == mapping.Filter.Field, "zdanie domyślne wyszukiwania ≠ filter z mapping.json");
        Ensure(mapping, text.Stats[0].Kind == mapping.Statistic.Kind && text.Stats[0].Field == mapping.Statistic.Field, "zdanie domyślne statystyki ≠ statistic z mapping.json");
        Ensure(mapping, mapping.ListFields is { } list && text.ListFields[0].Fields.SequenceEqual(list), "zdanie domyślne listy ≠ listFields z mapping.json");
        Ensure(mapping, text.Stats.Where(o => o.Kind is "sum" or "avg").All(o => NumericTypes.Any(columns[o.Field].StartsWith)), "sum/avg na polu nieliczbowym");
    }

    private static void Ensure(ScenarioMapping mapping, bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException($"{mapping.Scenario} (scenarios.json): {message}.");
    }
}
