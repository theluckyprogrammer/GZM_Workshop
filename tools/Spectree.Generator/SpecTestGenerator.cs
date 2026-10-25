using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Spectree.Generator;

// Every capability file (*.md in AdditionalFiles) becomes one xUnit test class; every numbered rule becomes one test
// whose name is the whole sentence ("Reguła 3: …"). The test runs the [Step] method bound to that sentence,
// so a rule without a step is a red test with the message "Brak definicji kroku…".
[Generator]
public sealed class SpecTestGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor NoRules = Error("SPEC0001", "Plik specyfikacji bez reguł",
        "Plik '{0}' nie ma sekcji '## Reguły' z ponumerowanymi zdaniami (1. …)");

    private static readonly DiagnosticDescriptor NoCapability = Error("SPEC0002", "Plik specyfikacji bez nazwy",
        "Plik '{0}' nie ma na początku nagłówka '---' z linią 'capability: <slug>'");

    private static readonly DiagnosticDescriptor DuplicateRule = Error("SPEC0003", "Powtórzony numer reguły",
        "Plik '{0}': numer reguły {1} występuje więcej niż raz – numerów nie zmienia się ani nie powtarza");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var specs = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) => (Name: Path.GetFileName(file.Path), Text: file.GetText(ct)?.ToString()));
        var rootNamespace = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) => options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var value) ? value : null);

        context.RegisterSourceOutput(specs.Combine(rootNamespace), static (output, pair) => Generate(output, pair.Left.Name, pair.Left.Text, pair.Right));
    }

    private static void Generate(SourceProductionContext output, string name, string? text, string? rootNamespace)
    {
        if (text is null)
            return;

        var file = CapabilityFile.Parse(text);
        var duplicate = file.Rules.GroupBy(r => r.Number).FirstOrDefault(g => g.Count() > 1);
        var problem = file.Capability is null ? Diagnostic.Create(NoCapability, Location.None, name)
            : file.Rules.Count == 0 ? Diagnostic.Create(NoRules, Location.None, name)
            : duplicate is not null ? Diagnostic.Create(DuplicateRule, Location.None, name, duplicate.Key)
            : null;
        if (problem is not null)
        {
            output.ReportDiagnostic(problem);
            return;
        }

        output.AddSource(TestSource.ClassName(file.Capability!) + ".g.cs", TestSource.Emit(file, rootNamespace));
    }

    private static DiagnosticDescriptor Error(string id, string title, string message) =>
        new(id, title, message, "Spectree", DiagnosticSeverity.Error, isEnabledByDefault: true);
}
