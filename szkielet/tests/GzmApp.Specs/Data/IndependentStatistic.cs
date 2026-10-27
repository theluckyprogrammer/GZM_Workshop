using System.Globalization;

namespace GzmApp.Specs.Data;

// E3 computed independently: NULLs out, groups without case (like a CI collation), avg rounded to 2 places.
public static class IndependentStatistic
{
    private static readonly StringComparer Groups = StringComparer.Create(CultureInfo.GetCultureInfo("pl-PL"), ignoreCase: true);

    public static IReadOnlyList<(string Label, string Value)> Compute(IndependentScenario data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var statistic = data.Scenario.Mapping.GetProperty("statistic");
        var field = statistic.GetProperty("field").GetString()!;
        var values = data.Rows.Select(r => r.Cells[field].Text).OfType<string>().ToList();
        return statistic.GetProperty("kind").GetString() switch
        {
            "sum" => [("Suma", values.Sum(Number).ToString(CultureInfo.InvariantCulture))],
            "avg" => [("Średnia", Math.Round(values.Average(Number), 2, MidpointRounding.AwayFromZero).ToString("F2", CultureInfo.InvariantCulture))],
            "top5" => Grouped(values).Take(5).ToList(),
            _ => Grouped(values).ToList(),
        };
    }

    private static IEnumerable<(string Label, string Value)> Grouped(List<string> values) =>
        values.GroupBy(v => v, Groups)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key, Groups)
            .Select(g => (g.Key, g.Count().ToString(CultureInfo.InvariantCulture)));

    private static decimal Number(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);
}
