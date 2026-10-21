using CodeRules;

// Usage: dotnet run --project tools/CodeRules -- <repo root> <folder> [<folder> ...]
// Checks every C# project (folder with a .csproj) under the given folders. Exit code 1 when there are violations.
var root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
var folders = args.Skip(1).DefaultIfEmpty("src").Select(f => Path.Combine(root, f));
var projects = folders.SelectMany(f => Directory.GetFiles(f, "*.csproj", SearchOption.AllDirectories)).Select(Path.GetDirectoryName).OfType<string>().Order().ToList();
var violations = projects.SelectMany(p => Check(p, root)).ToList();

violations.ForEach(v => Console.WriteLine(v));
Console.WriteLine($"CodeRules: {projects.Count} projektów, naruszeń: {violations.Count}");
return violations.Count == 0 ? 0 : 1;

static IEnumerable<Violation> Check(string project, string root)
{
    var compilation = ProjectCompiler.Compile(project);
    return compilation.SyntaxTrees.SelectMany(t => Rules.Check(t, compilation.GetSemanticModel(t), root));
}
