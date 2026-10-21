using System.Net;
using System.Text.RegularExpressions;
using Expect;

namespace Cards.Print;

// Checks of print/: the cheat sheet equals checkpoints.md, pair cards have no expected values and no placeholders.
public static partial class PrintChecker
{
    public static IEnumerable<string> CheckCard(string scenario, string html)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);
        ArgumentNullException.ThrowIfNull(html);

        var name = $"print/karta-pary-{scenario}.html";
        if (html.Contains("data-cp", StringComparison.Ordinal) || CheckpointId().IsMatch(html))
            yield return $"{name}: wartość oczekiwana na karcie ucznia";
        if (html.Contains("{{", StringComparison.Ordinal))
            yield return $"{name}: niewypełnione pole";
        if (EmptyElement().IsMatch(html))
            yield return $"{name}: puste pole (pusty element listy lub komórki)";
    }

    public static IEnumerable<string> CheckCheatSheet(string html, IReadOnlyDictionary<string, IReadOnlyList<Checkpoint>> checkpoints)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(checkpoints);

        var rows = Row().Matches(html).ToDictionary(m => m.Groups["s"].Value, m => m.Groups["cells"].Value);
        var missing = checkpoints.Keys.Where(s => !rows.ContainsKey(s)).Select(s => $"ściąga: brak wiersza {s}");
        var wrong = rows.SelectMany(r => Cell().Matches(r.Value)
            .Where(m => !Matches(checkpoints, r.Key, m.Groups["id"].Value, WebUtility.HtmlDecode(m.Groups["v"].Value)))
            .Select(m => $"ściąga {r.Key}: {m.Groups["id"].Value} = {m.Groups["v"].Value} ≠ checkpoints.md"));
        var count = rows.Values.Sum(v => Cell().Matches(v).Count);
        return missing.Concat(wrong).Concat(count == 0 ? ["ściąga: brak wartości"] : []);
    }

    private static bool Matches(IReadOnlyDictionary<string, IReadOnlyList<Checkpoint>> checkpoints, string scenario, string id, string value) =>
        checkpoints.TryGetValue(scenario, out var list) && list.Any(c => c.Id == id && c.Value == value);

    [GeneratedRegex(@"<tr data-scenario=""(?<s>s\d\d)"">(?<cells>.*?)</tr>", RegexOptions.Singleline)]
    private static partial Regex Row();

    [GeneratedRegex(@"<span data-cp=""(?<id>[^""]+)"">(?<v>.*?)</span>", RegexOptions.Singleline)]
    private static partial Regex Cell();

    [GeneratedRegex(@"\(E\d\.[\w.]+\)")]
    private static partial Regex CheckpointId();

    [GeneratedRegex(@"<(li|td)>\s*</\1>")]
    private static partial Regex EmptyElement();
}
