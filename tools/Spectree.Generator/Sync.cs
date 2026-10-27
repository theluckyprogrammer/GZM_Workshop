namespace Spectree.Generator;

// Writes one test file per specification file and deletes files of steps that lost their specification.
// A file is written only when its content changed, so an unchanged build does not touch the sources.
public static class Sync
{
    public static int Run(string specDir, string outDir)
    {
        if (!Directory.Exists(specDir))
            return Report($"Brak folderu specyfikacji: {specDir} – scenariusz ma podfolder spec/ z plikami *.md.");

        var specs = ReadSpecs(specDir);
        var problems = specs.Where(pair => pair.Problem is not null)
            .Select(pair => $"{pair.Path}: {pair.Problem}")
            .ToList();
        if (problems.Count > 0)
            return Report(string.Join(Environment.NewLine, problems));

        Directory.CreateDirectory(outDir);
        var (names, written) = Write(specs.Select(pair => pair.File), outDir);
        var removed = DeleteStale(outDir, names);
        Console.WriteLine($"Spectree: testy zgodne ze specyfikacją ({names.Count} plików, zapisano {written}, usunięto {removed}).");
        return 0;
    }

    private static int Report(string message)
    {
        Console.Error.WriteLine("Spectree: " + message);
        return 1;
    }

    private static List<(string Path, CapabilityFile File, string? Problem)> ReadSpecs(string specDir) =>
        Directory.EnumerateFiles(specDir, "*.md").Order()
            .Select(path =>
            {
                var file = CapabilityFile.Parse(File.ReadAllText(path));
                return (path, file, file.Problem());
            })
            .ToList();

    private static (HashSet<string> Names, int Written) Write(IEnumerable<CapabilityFile> specs, string outDir)
    {
        var names = new HashSet<string>();
        var written = 0;
        foreach (var spec in specs)
        {
            var (name, code) = TestSource.Emit(spec);
            names.Add(name);
            var target = Path.Combine(outDir, name + ".cs");
            if (File.Exists(target) && File.ReadAllText(target) == code)
                continue;
            File.WriteAllText(target, code);
            written++;
        }
        return (names, written);
    }

    private static int DeleteStale(string outDir, HashSet<string> names) =>
        Directory.EnumerateFiles(outDir, "*.cs")
            .Where(file => names.Contains(Path.GetFileNameWithoutExtension(file)) == false
                           && TestSource.IsGenerated(File.ReadLines(file).FirstOrDefault() ?? string.Empty))
            .Count(file => { File.Delete(file); return true; });
}
