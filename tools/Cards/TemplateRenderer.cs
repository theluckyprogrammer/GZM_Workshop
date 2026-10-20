using System.Text.RegularExpressions;
using Expect;

namespace Cards;

// {{name}} – variable, {{cp:ID}} – "**value** (ID)", {{cpList:PREFIX}} – bullet list of PREFIX.N values,
// {{cpStat}} – E3 result as a list, {{traps:N}} – trap table of stage N. An unknown placeholder is an error.
public sealed partial class TemplateRenderer(IReadOnlyDictionary<string, string> variables, IReadOnlyList<Checkpoint> checkpoints, IReadOnlyList<Trap> traps)
{
    public string Render(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return Placeholder().Replace(template, match => Resolve(match.Groups["kind"].Value, match.Groups["arg"].Value));
    }

    private string Resolve(string kind, string arg) => kind switch
    {
        "cp" => Reference(checkpoints.FirstOrDefault(c => c.Id == arg) ?? throw Unknown(arg)),
        "cpList" => List(checkpoints.Where(c => c.Id.StartsWith(arg + ".", StringComparison.Ordinal))),
        "cpStat" => List(checkpoints.Where(c => c.Id == "E3.value" || c.Id.StartsWith("E3.group.", StringComparison.Ordinal))),
        "traps" => TrapTable(int.Parse(arg)),
        _ => variables.TryGetValue(kind, out var value) ? value : throw Unknown(kind),
    };

    public static string Reference(Checkpoint checkpoint) => $"**{checkpoint.Value}** ({checkpoint.Id})";

    private static string List(IEnumerable<Checkpoint> items)
    {
        var lines = items.Select(c => $"   - {Label(c)}{Reference(c)}").ToList();
        return lines.Count == 0 ? throw new InvalidDataException("Pusta lista wartości w szablonie.") : string.Join("\n", lines);
    }

    // "Miejsce 1: Modernizm" → "Modernizm: ", "Wynik 1: SourceRow 13" → "Nr 13: ".
    private static string Label(Checkpoint checkpoint) => checkpoint.Label.Split(": ", 2) switch
    {
        [_, var rest] when rest.StartsWith("SourceRow ", StringComparison.Ordinal) => $"Nr {rest["SourceRow ".Length..]}: ",
        [_, var rest] => $"{rest}: ",
        _ => "",
    };

    private string TrapTable(int stage)
    {
        var rows = traps.Where(t => t.Stage == stage).Select(t => $"| {t.Text} | {t.Effect} | {t.Tool} |").ToList();
        return "| Pułapka | Co się stanie | Narzędzie |\n|---|---|---|\n" + string.Join("\n", rows);
    }

    private static InvalidDataException Unknown(string name) => new($"Nieznane pole szablonu: {name}");

    [GeneratedRegex(@"\{\{(?<kind>[A-Za-z]+)(:(?<arg>[^}]+))?\}\}")]
    private static partial Regex Placeholder();
}
