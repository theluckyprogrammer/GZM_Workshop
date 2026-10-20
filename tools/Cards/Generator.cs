using Expect;

namespace Cards;

// Writes karta.md, prompty.md (students) and prowadzacy.md (teacher) of one scenario from tools/Cards/templates.
public static class Generator
{
    public static readonly string[] StudentOutputs = ["karta.md", "prompty.md"];
    public static readonly string[] Outputs = [.. StudentOutputs, "prowadzacy.md"];

    public static void Write(string templateDirectory, string scenarioDirectory, ScenarioMapping mapping, ScenarioText text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioDirectory);
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(text);

        var checkpoints = CheckpointFile.Read(Path.Combine(scenarioDirectory, "checkpoints.md"));
        var renderer = new TemplateRenderer(Variables.For(mapping, checkpoints, text), checkpoints, Traps.For(mapping, checkpoints, text));
        foreach (var output in Outputs)
            WriteOne(renderer, Path.Combine(templateDirectory, output), Path.Combine(scenarioDirectory, output));
    }

    private static void WriteOne(TemplateRenderer renderer, string template, string target)
    {
        var text = renderer.Render(File.ReadAllText(template)).ReplaceLineEndings("\n");
        File.WriteAllText(target, text, new System.Text.UTF8Encoding(false));
    }
}
