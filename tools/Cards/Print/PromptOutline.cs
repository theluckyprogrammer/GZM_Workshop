using System.Text.RegularExpressions;

namespace Cards.Print;

public sealed record Decision(int Number, string Question);

public sealed record Step(int Number, string Title, IReadOnlyList<string> Look, IReadOnlyList<Decision> Decisions);

// The road through scenarios/sNN/karta.md (the "Prompty – krok po kroku" section): steps, "see what you built" points, decisions with variants, "what next" ideas.
public sealed partial record PromptOutline(IReadOnlyList<Step> Steps, IReadOnlyList<string> Next)
{
    public static PromptOutline Parse(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var headings = StepHeading().Matches(markdown).ToList();
        var steps = headings
            .Select((m, i) => ToStep(int.Parse(m.Groups["n"].Value), m.Groups["title"].Value,
                markdown[(m.Index + m.Length)..(i + 1 < headings.Count ? headings[i + 1].Index : markdown.Length)]))
            .ToList();
        var next = markdown.Contains("## Gotowe!", StringComparison.Ordinal)
            ? NumberedItems(markdown[markdown.IndexOf("## Gotowe!", StringComparison.Ordinal)..])
            : [];
        return new PromptOutline(steps, next);
    }

    private static Step ToStep(int number, string title, string section)
    {
        var body = section.Split("\n## Gotowe!")[0];
        var marker = new[] { "zobaczcie, co powstało", "zanim zaczniecie" }.FirstOrDefault(m => body.Contains(m, StringComparison.Ordinal));
        var look = marker is null ? [] : NumberedItems(body[body.IndexOf(marker, StringComparison.Ordinal)..]);
        var decisions = DecisionHeading().Matches(body)
            .Select(m => new Decision(int.Parse(m.Groups["n"].Value), m.Groups["q"].Value))
            .ToList();
        return new Step(number, title, look, decisions);
    }

    // The first numbered list after the start of the text.
    private static List<string> NumberedItems(string text) =>
        text.Split('\n').SkipWhile(l => !Numbered().IsMatch(l)).TakeWhile(l => Numbered().IsMatch(l))
            .Select(l => Numbered().Replace(l, string.Empty).Trim()).ToList();

    [GeneratedRegex(@"^## Krok (?<n>\d) – (?<title>.+)$", RegexOptions.Multiline)]
    private static partial Regex StepHeading();

    [GeneratedRegex(@"^### ⏸ Pauza – decyzja (?<n>\d): (?<q>.+)$", RegexOptions.Multiline)]
    private static partial Regex DecisionHeading();

    [GeneratedRegex(@"^\d+\. ")]
    private static partial Regex Numbered();
}
