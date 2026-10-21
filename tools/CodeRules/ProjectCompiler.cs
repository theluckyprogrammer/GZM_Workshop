using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeRules;

// A C# project as a Roslyn compilation: its .cs files (no bin/obj) + references from its build output and the shared
// frameworks (Microsoft.NETCore.App, Microsoft.AspNetCore.App). Build the project first, so bin/ has the package dlls.
public static class ProjectCompiler
{
    public static CSharpCompilation Compile(string projectDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

        var trees = SourceFiles(projectDirectory)
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), new CSharpParseOptions(LanguageVersion.Preview), f))
            .ToList();
        return CSharpCompilation.Create(Path.GetFileName(projectDirectory), trees, References(projectDirectory),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    public static IEnumerable<string> SourceFiles(string projectDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

        return Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !Segments(projectDirectory, f).Any(s => s is "bin" or "obj"))
            .Order(StringComparer.Ordinal);
    }

    private static IEnumerable<string> Segments(string root, string file) =>
        Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static List<MetadataReference> References(string projectDirectory)
    {
        var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var aspNet = runtime.Replace("Microsoft.NETCore.App", "Microsoft.AspNetCore.App", StringComparison.Ordinal);
        var bin = Directory.Exists(Path.Combine(projectDirectory, "bin"))
            ? Directory.GetFiles(Path.Combine(projectDirectory, "bin"), "*.dll", SearchOption.AllDirectories)
            : [];
        return new[] { runtime, aspNet }.Where(Directory.Exists).SelectMany(d => Directory.GetFiles(d, "*.dll")).Concat(bin)
            .GroupBy(Path.GetFileName).Select(g => g.First())
            .Where(IsAssembly).Select(f => (MetadataReference)MetadataReference.CreateFromFile(f)).ToList();
    }

    private static bool IsAssembly(string path)
    {
        try
        {
            System.Reflection.AssemblyName.GetAssemblyName(path);
            return true;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }
}
