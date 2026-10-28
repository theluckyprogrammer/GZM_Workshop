namespace GzmApp.Specs.Infrastructure;

public static class RepoPaths
{
    // The repository root is the first parent folder with global.json.
    public static string Root { get; } = Find(AppContext.BaseDirectory);

    public static string Scenarios => Path.Combine(Root, "scenarios");

    private static string Find(string start)
    {
        var directory = new DirectoryInfo(start);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Nie znaleziono global.json nad katalogiem testów.");
    }
}
