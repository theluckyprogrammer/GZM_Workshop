using Expect;

namespace Cards;

// Writes karta.md, prompty.md (students), prowadzacy.md (teacher) and the specification spec/sNN-NNN-*.md
// (one capability file per workshop step, tests in tests/GzmApp.Specs) of one scenario from tools/Cards/templates.
public static class Generator
{
    public static readonly string[] StudentOutputs = ["karta.md", "prompty.md"];
    public static readonly string[] Outputs = [.. StudentOutputs, "prowadzacy.md"];
    public static readonly string[] SpecTemplates = ["001-importuj-dane.md", "002-szukaj-rekordow.md", "003-pokaz-strone.md", "004-zapisz-ulubione.md"];

    public static string SpecFile(string scenario, string template)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        return Path.Combine("spec", $"{scenario}-{template}");
    }

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
        Directory.CreateDirectory(Path.Combine(scenarioDirectory, "spec"));
        foreach (var template in SpecTemplates)
            WriteOne(renderer, Path.Combine(templateDirectory, "spec", template), Path.Combine(scenarioDirectory, SpecFile(mapping.Scenario, template)));
    }

    private static void WriteOne(TemplateRenderer renderer, string template, string target)
    {
        var text = renderer.Render(File.ReadAllText(template)).ReplaceLineEndings("\n");
        File.WriteAllText(target, text, new System.Text.UTF8Encoding(false));
    }
}
