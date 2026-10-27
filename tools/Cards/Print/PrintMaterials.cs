using System.Text.RegularExpressions;
using Expect;

namespace Cards.Print;

// print/opis-scenariusza-sNN.html (students), print/sciaga-prowadzacego.html (teacher)
// and print/instrukcja-stanowiska.html (station setup, static), from scenarios/sNN/*.
public static partial class PrintMaterials
{
    public const string CheatSheetFile = "sciaga-prowadzacego.html";
    public const string StationFile = "instrukcja-stanowiska.html";
    public const string SheetPrefix = "opis-scenariusza-";

    public static void Write(string root, IReadOnlyDictionary<string, ScenarioText> texts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(texts);

        var templates = Path.Combine(root, "tools", "Cards", "templates", "print");
        var sheetTemplate = File.ReadAllText(Path.Combine(templates, "opis-scenariusza.html"));
        var scenarios = Load(root, texts);
        var stories = ScenarioSheet.LoadStories(Path.Combine(root, "tools", "Cards", "opisy.json"));
        Directory.CreateDirectory(Path.Combine(root, "print"));
        foreach (var s in scenarios)
            Save(Path.Combine(root, "print", $"{SheetPrefix}{s.Mapping.Scenario}.html"), ScenarioSheet.Render(sheetTemplate, s.Mapping, s.Text, stories[s.Mapping.Scenario]));
        var rows = scenarios.Select(s => new CheatSheetRow(s.Mapping, s.Text, s.Checkpoints)).ToList();
        Save(Path.Combine(root, "print", CheatSheetFile), CheatSheet.Render(File.ReadAllText(Path.Combine(templates, "sciaga.html")), rows));
        // The station instruction has no scenario variables – the template goes to print/ as is.
        Save(Path.Combine(root, "print", StationFile), File.ReadAllText(Path.Combine(templates, StationFile)));
    }

    public static IReadOnlyList<string> Check(string root, IReadOnlyDictionary<string, ScenarioText> texts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(texts);

        var scenarios = Load(root, texts);
        var sheets = scenarios.SelectMany(s => CheckSheet(root, s.Mapping.Scenario));
        var sheet = Path.Combine(root, "print", CheatSheetFile);
        var sheetErrors = File.Exists(sheet)
            ? PrintChecker.CheckCheatSheet(File.ReadAllText(sheet), scenarios.ToDictionary(s => s.Mapping.Scenario, s => s.Checkpoints))
            : [$"brak print/{CheatSheetFile}"];
        return sheets.Concat(sheetErrors).Concat(CheckStation(root)).ToList();
    }

    private static IEnumerable<string> CheckStation(string root)
    {
        var path = Path.Combine(root, "print", StationFile);
        if (!File.Exists(path))
            return [$"brak print/{StationFile}"];
        var html = File.ReadAllText(path);
        // The page prints screenshots from docs/obrazki – every referenced image must exist.
        var missing = Img().Matches(html).Select(m => m.Groups["src"].Value)
            .Where(src => !File.Exists(Path.GetFullPath(Path.Combine(root, "print", src))))
            .Select(src => $"{StationFile}: brak obrazka {src}");
        return PrintChecker.CheckStation(html).Concat(missing);
    }

    private static IEnumerable<string> CheckSheet(string root, string scenario)
    {
        var path = Path.Combine(root, "print", $"{SheetPrefix}{scenario}.html");
        return File.Exists(path) ? PrintChecker.CheckSheet(scenario, File.ReadAllText(path)) : [$"brak print/{SheetPrefix}{scenario}.html"];
    }

    private sealed record Loaded(ScenarioMapping Mapping, ScenarioText Text, IReadOnlyList<Checkpoint> Checkpoints, PromptOutline Outline);

    private static List<Loaded> Load(string root, IReadOnlyDictionary<string, ScenarioText> texts) =>
        Directory.GetDirectories(Path.Combine(root, "scenarios"), "s*").Order(StringComparer.Ordinal)
            .Select(d => (Directory: d, Mapping: MappingLoader.Load(Path.Combine(d, "mapping.json"))))
            .Select(x => new Loaded(x.Mapping, texts[x.Mapping.Scenario],
                CheckpointFile.Read(Path.Combine(x.Directory, "checkpoints.md")),
                PromptOutline.Parse(File.ReadAllText(Path.Combine(x.Directory, "karta.md")))))
            .ToList();

    private static void Save(string path, string html) =>
        File.WriteAllText(path, html.ReplaceLineEndings("\n"), new System.Text.UTF8Encoding(false));

    [GeneratedRegex(@"<img src=""(?<src>[^""]+)""")]
    private static partial Regex Img();
}
