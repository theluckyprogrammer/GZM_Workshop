using System.Globalization;

namespace Expect;

// E3. Empty values (NULL) are left out, as in SQL aggregates.
// top5 / count-by: GROUP BY with a case-insensitive collation, ordered by count descending, then by value.
// avg: AVG over a decimal (not integer division), rounded to 2 places.
public static class Statistics
{
    private static readonly StringComparer GroupComparer = StringComparer.Create(CultureInfo.GetCultureInfo("pl-PL"), ignoreCase: true);

    public static IEnumerable<Checkpoint> Compute(ScenarioMapping mapping, RecordSet set)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(set);

        var field = mapping.Statistic.Field;
        var values = set.Records.Select(r => r[field]).Where(v => v.State == ValueState.Ok).ToList();
        yield return new("E3.kind", "Statystyka", $"{mapping.Statistic.Kind}({field})");
        yield return new("E3.nulls", "Pominięte puste wartości (NULL)", Checks.Number(set.Records.Count - values.Count));
        foreach (var checkpoint in Result(mapping.Statistic.Kind, values))
            yield return checkpoint;
    }

    private static IEnumerable<Checkpoint> Result(string kind, List<ParsedValue> values) => kind switch
    {
        "sum" => [new("E3.value", "Suma", Checks.Number(values.Sum(Decimal)))],
        "avg" => [new("E3.value", "Średnia (2 miejsca po przecinku)", Average(values))],
        "top5" => Groups(values).Take(5).Concat(TieAtFifth(values)),
        _ => Groups(values),
    };

    private static string Average(List<ParsedValue> values) =>
        values.Count == 0
            ? "NULL"
            : Math.Round(values.Average(Decimal), 2, MidpointRounding.AwayFromZero).ToString("F2", CultureInfo.InvariantCulture);

    private static IEnumerable<Checkpoint> Groups(List<ParsedValue> values) =>
        values.GroupBy(v => v.Display, GroupComparer)
            .Select(g => (Value: g.Key, Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Value, GroupComparer)
            .Select((g, i) => new Checkpoint($"E3.group.{i + 1}", $"Miejsce {i + 1}: {g.Value}", Checks.Number(g.Count)));

    // A tie between places 5 and 6 makes the top 5 depend on the ORDER BY of the value.
    private static IEnumerable<Checkpoint> TieAtFifth(List<ParsedValue> values)
    {
        var counts = Groups(values).Select(g => g.Value).ToList();
        return counts.Count > 5 && counts[4] == counts[5]
            ? [new("E3.tie", "Remis 5. i 6. miejsca (o kolejności decyduje sortowanie po wartości)", "tak")]
            : [];
    }

    private static decimal Decimal(ParsedValue value) => System.Convert.ToDecimal(value.Value, CultureInfo.InvariantCulture);
}
