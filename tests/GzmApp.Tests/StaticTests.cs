using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

// Level 8: static checks of the repository (no app, no database).
public partial class StaticTests
{
    private static readonly string[] DeniedEdits =
        ["**/*.csproj", "**/launch.json", "**/tasks.json", "**/launchSettings.json", "**/Program.cs", "src/GzmApp/Identity/**", "global.json"];

    // Transitive dependencies of Microsoft.Data.SqlClient under the Microsoft redistributable license (findings.md, 23).
    private static readonly string[] LicenseExceptions = ["Microsoft.Data.SqlClient.SNI.runtime", "Microsoft.Identity.Client.NativeInterop"];

    [Fact]
    [Trait("Spec", "Q-01")]
    [Trait("Poziom", "8")]
    public void Kilo_config_is_valid_JSONC_and_denies_protected_files_and_shell()
    {
        var permission = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepoPaths.Root, "kilo.jsonc")),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }).RootElement.GetProperty("permission");

        Assert.All(DeniedEdits, pattern => Assert.Equal("deny", permission.GetProperty("edit").GetProperty(pattern).GetString()));
        Assert.Equal("deny", permission.GetProperty("bash").GetProperty("*").GetString());
    }

    [Fact]
    [Trait("Spec", "Q-04")]
    [Trait("Poziom", "8")]
    public void Every_direct_package_is_in_LICENSES_md()
    {
        var licenses = File.ReadAllText(Path.Combine(RepoPaths.Root, "docs", "LICENSES.md"));
        var packages = Directory.GetFiles(RepoPaths.Root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(p => XDocument.Load(p).Descendants("PackageReference"))
            .Select(r => $"| {r.Attribute("Include")!.Value}")
            .Distinct();

        Assert.All(packages, p => Assert.Contains(p, licenses, StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Spec", "Q-04")]
    [Trait("Poziom", "8")]
    public void Every_package_including_transitive_ones_has_an_allowed_license()
    {
        var wrong = Directory.GetFiles(RepoPaths.Root, "project.assets.json", SearchOption.AllDirectories)
            .SelectMany(Packages).Distinct()
            .Where(p => !LicenseExceptions.Contains(p.Id) && !Allowed().IsMatch(p.License))
            .Select(p => $"{p.Id} {p.Version}: {p.License}");

        Assert.Empty(wrong);
    }

    private static IEnumerable<(string Id, string Version, string License)> Packages(string assets)
    {
        var root = JsonDocument.Parse(File.ReadAllText(assets)).RootElement;
        var folder = root.GetProperty("packageFolders").EnumerateObject().First().Name;
        return root.GetProperty("libraries").EnumerateObject()
            .Where(l => l.Value.GetProperty("type").GetString() == "package")
            .Select(l => (Name: l.Name, Path: Path.Combine(folder, l.Value.GetProperty("path").GetString()!)))
            .Select(l => (l.Name.Split('/')[0], l.Name.Split('/')[1], License(l.Path)));
    }

    // <license type="expression">, or a known license URL (xunit: Apache-2.0).
    private static string License(string packagePath)
    {
        var nuspec = Directory.GetFiles(packagePath, "*.nuspec").FirstOrDefault();
        var text = nuspec is null ? string.Empty : File.ReadAllText(nuspec);
        var expression = Expression().Match(text);
        return expression.Success ? expression.Groups[1].Value
            : text.Contains("github.com/xunit/xunit", StringComparison.Ordinal) || text.Contains("xunit/xunit/master/license.txt", StringComparison.Ordinal) ? "Apache-2.0"
            : "brak licencji w nuspec";
    }

    [GeneratedRegex(@"^(MIT|Apache-2\.0|MS-PL|BSD-2-Clause|BSD-3-Clause|0BSD)( OR (MIT|Apache-2\.0|MS-PL|BSD-2-Clause|BSD-3-Clause|0BSD))*$")]
    private static partial Regex Allowed();

    [GeneratedRegex(@"<license type=""expression"">([^<]+)</license>")]
    private static partial Regex Expression();
}
