using Expect;

namespace Cards.Print;

public sealed record CheatSheetRow(ScenarioMapping Mapping, ScenarioText Text, IReadOnlyList<Checkpoint> Checkpoints);

// print/sciaga-prowadzacego.html: all scenarios in one table, values for variants A from checkpoints.md.
// Every value sits in <span data-cp="ID">, so the check can compare it with checkpoints.md.
public static class CheatSheet
{
    public static string Render(string template, IReadOnlyList<CheatSheetRow> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(rows);

        var variables = new Dictionary<string, string> { ["rows"] = string.Join("\n", rows.Select(Row)) };
        return new TemplateRenderer(variables, [], []).Render(template);
    }

    private static string Row(CheatSheetRow row)
    {
        var cp = row.Checkpoints;
        var cells = new[]
        {
            InlineHtml.Encode(row.Mapping.Scenario),
            InlineHtml.Encode(row.Text.App),
            Value(cp, "E1.rows"),
            $"{Value(cp, "E2.phrase")} → {Value(cp, "E2.count")}",
            Statistic(row.Mapping, cp),
            Value(cp, "E4.title"),
            InlineHtml.FromMarkdown(row.Text.Traps.FirstOrDefault()?.Trap ?? "–"),
        };
        return $"  <tr data-scenario=\"{row.Mapping.Scenario}\">{string.Concat(cells.Select(c => $"<td>{c}</td>"))}</tr>";
    }

    private static string Statistic(ScenarioMapping mapping, IReadOnlyList<Checkpoint> checkpoints)
    {
        var kind = InlineHtml.Encode($"{mapping.Statistic.Kind}({mapping.Statistic.Field})");
        var groups = checkpoints.Where(c => c.Id.StartsWith("E3.group.", StringComparison.Ordinal)).ToList();
        return groups.Count == 0
            ? $"{kind}: {Value(checkpoints, "E3.value")}"
            : $"{kind}<ol>{string.Concat(groups.Select(g => $"<li>{InlineHtml.Encode(g.Label.Split(": ", 2)[1])}: {Span(g)}</li>"))}</ol>";
    }

    private static string Value(IReadOnlyList<Checkpoint> checkpoints, string id) =>
        Span(checkpoints.First(c => c.Id == id));

    private static string Span(Checkpoint checkpoint) =>
        $"<span data-cp=\"{checkpoint.Id}\">{InlineHtml.Encode(checkpoint.Value)}</span>";
}
