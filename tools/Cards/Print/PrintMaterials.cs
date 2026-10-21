using Expect;

namespace Cards.Print;

// print/karta-pary-sNN.html (students) and print/sciaga-prowadzacego.html (teacher), from scenarios/sNN/*.
public static class PrintMaterials
{
    public const string CheatSheetFile = "sciaga-prowadzacego.html";

    public static void Write(string root, IReadOnlyDictionary<string, ScenarioText> texts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(texts);

        var templates = Path.Combine(root, "tools", "Cards", "templates", "print");
        var cardTemplate = File.ReadAllText(Path.Combine(templates, "karta-pary.html"));
        var scenarios = Load(root, texts);
        Directory.CreateDirectory(Path.Combine(root, "print"));
        foreach (var s in scenarios)
            Save(Path.Combine(root, "print", $"karta-pary-{s.Mapping.Scenario}.html"), PairCard.Render(cardTemplate, s.Mapping, s.Text, s.Outline));
        var rows = scenarios.Select(s => new CheatSheetRow(s.Mapping, s.Text, s.Checkpoints)).ToList();
        Save(Path.Combine(root, "print", CheatSheetFile), CheatSheet.Render(File.ReadAllText(Path.Combine(templates, "sciaga.html")), rows));
    }

    public static IReadOnlyList<string> Check(string root, IReadOnlyDictionary<string, ScenarioText> texts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(texts);

        var scenarios = Load(root, texts);
        var cards = scenarios.SelectMany(s => CheckCard(root, s.Mapping.Scenario));
        var sheet = Path.Combine(root, "print", CheatSheetFile);
        var sheetErrors = File.Exists(sheet)
            ? PrintChecker.CheckCheatSheet(File.ReadAllText(sheet), scenarios.ToDictionary(s => s.Mapping.Scenario, s => s.Checkpoints))
            : [$"brak print/{CheatSheetFile}"];
        return cards.Concat(sheetErrors).ToList();
    }

    private static IEnumerable<string> CheckCard(string root, string scenario)
    {
        var path = Path.Combine(root, "print", $"karta-pary-{scenario}.html");
        return File.Exists(path) ? PrintChecker.CheckCard(scenario, File.ReadAllText(path)) : [$"brak print/karta-pary-{scenario}.html"];
    }

    private sealed record Loaded(ScenarioMapping Mapping, ScenarioText Text, IReadOnlyList<Checkpoint> Checkpoints, PromptOutline Outline);

    private static List<Loaded> Load(string root, IReadOnlyDictionary<string, ScenarioText> texts) =>
        Directory.GetDirectories(Path.Combine(root, "scenarios"), "s*").Order(StringComparer.Ordinal)
            .Select(d => (Directory: d, Mapping: MappingLoader.Load(Path.Combine(d, "mapping.json"))))
            .Select(x => new Loaded(x.Mapping, texts[x.Mapping.Scenario],
                CheckpointFile.Read(Path.Combine(x.Directory, "checkpoints.md")),
                PromptOutline.Parse(File.ReadAllText(Path.Combine(x.Directory, "prompty.md")))))
            .ToList();

    private static void Save(string path, string html) =>
        File.WriteAllText(path, html.ReplaceLineEndings("\n"), new System.Text.UTF8Encoding(false));
}
