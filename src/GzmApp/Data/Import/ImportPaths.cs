namespace GzmApp.Data.Import;

// Paths inside the repository: scenarios/<Scenario>/mapping.json, data/..., db/.
public sealed record ImportPaths(string RepoRoot, string Scenario)
{
    public string MappingFile => Path.Combine(RepoRoot, "scenarios", Scenario, "mapping.json");

    public string DbDirectory => Path.Combine(RepoRoot, "db");

    // data/work/<file> (cleaned copy) wins over the file from mapping.json (data/raw/...).
    public string DataFile(ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var work = Path.Combine(RepoRoot, "data", "work", Path.GetFileName(mapping.Source.File));
        return File.Exists(work) ? work : Path.Combine(RepoRoot, mapping.Source.File);
    }

    // The repository root is the first parent folder with global.json (from the project folder or from bin/).
    public static ImportPaths Resolve(string contentRoot, string scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        var root = FindRoot(contentRoot) ?? FindRoot(AppContext.BaseDirectory)
            ?? throw new InvalidOperationException("Nie znaleziono katalogu repozytorium (global.json).");
        return new ImportPaths(root, scenario);
    }

    private static string? FindRoot(string start)
    {
        var directory = new DirectoryInfo(start);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
            directory = directory.Parent;
        return directory?.FullName;
    }
}
