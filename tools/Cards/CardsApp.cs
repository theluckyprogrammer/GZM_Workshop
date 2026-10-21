using Expect;

namespace Cards;

// Usage: dotnet run --project tools/Cards [-- check]
// Without arguments: generates the materials in scenarios/sNN/ and print/. "check": verifies them, exit code 1 on errors.
public static class CardsApp
{
    public static int Main(string[] args)
    {
        var root = RepoRoot.Find(Directory.GetCurrentDirectory());
        var tool = Path.Combine(root, "tools", "Cards");
        var texts = ScenarioTexts.Load(Path.Combine(tool, "scenarios.json"));
        var directories = Directory.GetDirectories(Path.Combine(root, "scenarios"), "s*").Order(StringComparer.Ordinal).ToList();
        var check = args.Contains("check");
        var errors = directories.SelectMany(d => check ? Check(d) : Generate(tool, d, texts)).ToList();
        if (!check)
            Print.PrintMaterials.Write(root, texts);
        errors.AddRange(Print.PrintMaterials.Check(root, texts));
        errors.ForEach(e => Console.WriteLine($"BŁĄD {e}"));
        Console.WriteLine($"Cards {(check ? "check" : "generate")}: {directories.Count} scenariuszy, błędów: {errors.Count}");
        return errors.Count == 0 ? 0 : 1;
    }

    private static IEnumerable<string> Generate(string tool, string directory, IReadOnlyDictionary<string, ScenarioText> texts)
    {
        var mapping = MappingLoader.Load(Path.Combine(directory, "mapping.json"));
        if (!texts.TryGetValue(mapping.Scenario, out var text))
            return [$"{mapping.Scenario}: brak wpisu w tools/Cards/scenarios.json"];
        Generator.Write(Path.Combine(tool, "templates"), directory, mapping, text);
        return Check(directory);
    }

    private static IEnumerable<string> Check(string directory)
    {
        var scenario = Path.GetFileName(directory);
        var checkpoints = CheckpointFile.Read(Path.Combine(directory, "checkpoints.md"));
        return Checker.Check(directory, checkpoints).Select(e => $"{scenario}/{e}");
    }
}
