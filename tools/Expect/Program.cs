using System.Text;
using Expect;

// Usage: dotnet run --project tools/Expect [-- <repo root>]
// For every scenarios/sNN/mapping.json writes scenarios/sNN/checkpoints.md and docs/expect-log.txt.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : RepoRoot.Find(Directory.GetCurrentDirectory());
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
var log = new StringBuilder();
var failures = 0;

foreach (var path in Directory.GetFiles(Path.Combine(root, "scenarios"), "mapping.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
    failures += Run(path) ? 0 : 1;

File.WriteAllText(Path.Combine(root, "docs", "expect-log.txt"), log.ToString().ReplaceLineEndings("\n"), utf8);
Console.WriteLine(failures == 0 ? "Expect: OK" : $"Expect: błędy w {failures} scenariuszach");
return failures == 0 ? 0 : 1;

bool Run(string mappingPath)
{
    var scenario = Path.GetFileName(Path.GetDirectoryName(mappingPath))!;
    try
    {
        var result = ScenarioResult.Compute(root, MappingLoader.Load(mappingPath));
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(mappingPath)!, "checkpoints.md"), CheckpointWriter.Render(result), utf8);
        AppendLog(scenario, result);
        Console.WriteLine($"{scenario}: OK ({result.Checkpoints.Count} wartości)");
        return true;
    }
    catch (Exception ex) when (ex is InvalidDataException or IOException or KeyNotFoundException)
    {
        log.AppendLine($"[{scenario}] BŁĄD: {ex.Message}");
        Console.WriteLine($"{scenario}: BŁĄD: {ex.Message}");
        return false;
    }
}

void AppendLog(string scenario, ScenarioResult result)
{
    log.AppendLine($"[{scenario}] {result.Mapping.Dataset} | {result.Source.Path} | {result.Source.EncodingName}");
    foreach (var checkpoint in result.Checkpoints)
        log.AppendLine($"{scenario} {checkpoint.Id} = {CheckpointWriter.Escape(checkpoint.Value)}");
}
