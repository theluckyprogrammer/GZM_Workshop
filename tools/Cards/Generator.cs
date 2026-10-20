using Expect;

namespace Cards;

// Writes karta.md, prompty/*.md and pulapki.md of one scenario from tools/Cards/templates.
public static class Generator
{
    public static readonly string[] Outputs =
    [
        "karta.md", "prompty/etap1.md", "prompty/etap2.md", "prompty/etap3.md", "prompty/etap4.md",
        "prompty/detektyw.md", "prompty/gwiazdka.md", "pulapki.md",
    ];

    public static void Write(string templateDirectory, string scenarioDirectory, ScenarioMapping mapping, ScenarioText text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioDirectory);
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(text);

        var checkpoints = CheckpointFile.Read(Path.Combine(scenarioDirectory, "checkpoints.md"));
        var renderer = new TemplateRenderer(Variables.For(mapping, checkpoints, text), checkpoints, Traps.For(mapping, checkpoints, text));
        Directory.CreateDirectory(Path.Combine(scenarioDirectory, "prompty"));
        foreach (var output in Outputs)
            WriteOne(renderer, Path.Combine(templateDirectory, Path.GetFileName(output)), Path.Combine(scenarioDirectory, output));
    }

    private static void WriteOne(TemplateRenderer renderer, string template, string target)
    {
        var text = renderer.Render(File.ReadAllText(template)).ReplaceLineEndings("\n");
        File.WriteAllText(target, text, new System.Text.UTF8Encoding(false));
    }
}
