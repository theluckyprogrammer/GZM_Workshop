namespace Expect;

public static class RepoRoot
{
    // The repository root is the first parent folder with global.json.
    public static string Find(string start)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(start);

        var directory = new DirectoryInfo(start);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Nie znaleziono global.json. Podaj katalog repo jako argument.");
    }
}
