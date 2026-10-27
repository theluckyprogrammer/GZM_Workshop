using Expect;

namespace Cards.Print;

// print/karta-pary-sNN.html: one A4 card per pair, built from karta data and prompty.md (no expected values).
public static class PairCard
{
    public static string Render(string template, ScenarioMapping mapping, ScenarioText text, PromptOutline outline)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(outline);

        var variables = new Dictionary<string, string>
        {
            ["app"] = InlineHtml.Encode(text.App), ["code"] = InlineHtml.Encode(mapping.Scenario),
            ["idea"] = InlineHtml.Encode(text.Idea), ["aiTask"] = InlineHtml.Encode(text.AiTask),
            ["dataset"] = InlineHtml.Encode(mapping.Dataset), ["attribution"] = InlineHtml.Encode(mapping.Attribution),
            ["steps"] = string.Join("\n", outline.Steps.Select(StepHtml)),
            ["next"] = string.Join("\n", outline.Next.Select(n => $"    <li>{InlineHtml.FromMarkdown(Shorten(n))}</li>")),
        };
        return new TemplateRenderer(variables, [], []).Render(template);
    }

    private static string StepHtml(Step step) =>
        $"""
        <section class="krok">
          <h3>Krok {step.Number} – {InlineHtml.Encode(step.Title)}</h3>
          <p><strong>{(step.Number == 0 ? "Zanim zaczniecie" : "Zobaczcie, co powstało")}:</strong></p>
          <ul>
        {string.Join("\n", step.Look.Select(l => $"    <li>{InlineHtml.FromMarkdown(l)}</li>"))}
          </ul>
        {string.Join("\n", step.Decisions.Select(DecisionHtml))}
        </section>
        """;

    private static string DecisionHtml(Decision decision) =>
        $"""
          <div class="decyzja">
            <p><strong>Decyzja {decision.Number}:</strong> {InlineHtml.Encode(decision.Question)}</p>
            <span class="opcja">☐ pytacie Kilo o 3 propozycje i wybieracie (prompty.md)</span>
          </div>
          """;

    // An AI idea may end with "Prompt na start:" – on paper only the idea itself.
    private static string Shorten(string idea) => idea.Replace(" Prompt na start:", string.Empty, StringComparison.Ordinal);
}
