using System.Text.RegularExpressions;
using Expect;

namespace Cards;

// "check": the generated files of a scenario are complete and every **value** (ID) equals checkpoints.md.
public static partial class Checker
{
    private const string Ending = "Nie dodawaj pakietów.";
    private static readonly string[] Sections = ["## CEL", "## DANE", "## ZASADY", "## GOTOWE GDY"];
    private static readonly string[] StagePrompts = ["prompty/etap1.md", "prompty/etap2.md", "prompty/etap3.md", "prompty/etap4.md", "prompty/gwiazdka.md"];

    public static IReadOnlyList<string> Check(string scenarioDirectory, IReadOnlyList<Checkpoint> checkpoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioDirectory);
        ArgumentNullException.ThrowIfNull(checkpoints);

        var missing = Generator.Outputs.Where(f => !File.Exists(Path.Combine(scenarioDirectory, f))).Select(f => $"brak pliku {f}");
        var present = Generator.Outputs.Where(f => File.Exists(Path.Combine(scenarioDirectory, f)));
        return missing.Concat(present.SelectMany(f => CheckFile(f, File.ReadAllText(Path.Combine(scenarioDirectory, f)), checkpoints))).ToList();
    }

    private static IEnumerable<string> CheckFile(string name, string text, IReadOnlyList<Checkpoint> checkpoints)
    {
        var references = Reference().Matches(text).ToList();
        var wrong = references.Where(m => !Matches(m, checkpoints)).Select(m => $"{name}: {m.Value} ≠ checkpoints.md");
        var leftovers = text.Contains("{{") ? [$"{name}: niewypełnione pole {{{{…}}}}"] : Array.Empty<string>();
        var empty = EmptyField().IsMatch(text) ? [$"{name}: puste pole"] : Array.Empty<string>();
        return wrong.Concat(leftovers).Concat(empty).Concat(StageRules(name, text, references.Count));
    }

    private static IEnumerable<string> StageRules(string name, string text, int references)
    {
        if (!StagePrompts.Contains(name) && name != "prompty/detektyw.md")
            yield break;
        if (!text.TrimEnd().EndsWith(Ending, StringComparison.Ordinal) || !text.Contains("Nie zmieniaj plików poza: ", StringComparison.Ordinal))
            yield return $"{name}: brak zdania końcowego „Nie zmieniaj plików poza: … {Ending}”";
        if (StagePrompts.Contains(name) && Sections.Any(s => !text.Contains(s, StringComparison.Ordinal)))
            yield return $"{name}: brak sekcji CEL / DANE / ZASADY / GOTOWE GDY";
        if (StagePrompts.Contains(name) && references == 0)
            yield return $"{name}: GOTOWE GDY bez wartości z checkpoints.md";
    }

    private static bool Matches(Match match, IReadOnlyList<Checkpoint> checkpoints) =>
        checkpoints.Any(c => c.Id == match.Groups["id"].Value && c.Value == match.Groups["value"].Value);

    // **value** (E1.rows)
    [GeneratedRegex(@"\*\*(?<value>(?:(?!\*\*).)+)\*\* \((?<id>E\d\.[\w.]+)\)")]
    private static partial Regex Reference();

    // Empty bold, empty code span or a list item / table cell with nothing in it.
    [GeneratedRegex(@"\*\*\*\*|(?<!`)``(?!`)|^[ \t]*-[ \t]*$|\|[ \t]+\|", RegexOptions.Multiline)]
    private static partial Regex EmptyField();
}
