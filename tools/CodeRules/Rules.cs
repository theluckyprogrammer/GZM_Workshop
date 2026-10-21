using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeRules;

public sealed record Violation(string File, int Line, string Rule, string Message)
{
    public override string ToString() => $"{File}:{Line}: [{Rule}] {Message}";
}

// The code rules of the workshop (00 – Zasady kodu): no nested if/loops, methods ≤ 20 lines, classes ≤ 150 lines,
// a guard clause in public methods for every non-nullable reference parameter.
public static class Rules
{
    public const int MaxMethodLines = 20;
    public const int MaxClassLines = 150;

    public static IEnumerable<Violation> Check(SyntaxTree tree, SemanticModel model, string root)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var file = Path.GetRelativePath(root, tree.FilePath).Replace('\\', '/');
        var nodes = tree.GetRoot().DescendantNodes().ToList();
        return nodes.OfType<StatementSyntax>().Where(IsNested).Select(n => At(file, n, "zagnieżdżenie", "if/pętla wewnątrz if/pętli"))
            .Concat(nodes.OfType<BaseMethodDeclarationSyntax>().Where(m => Lines(Body(m)) > MaxMethodLines)
                .Select(m => At(file, m, "metoda", $"{Name(m)}: {Lines(Body(m))} linii (max {MaxMethodLines})")))
            .Concat(nodes.OfType<TypeDeclarationSyntax>().Where(t => Lines(t) > MaxClassLines)
                .Select(t => At(file, t, "klasa", $"{t.Identifier.Text}: {Lines(t)} linii (max {MaxClassLines})")))
            .Concat(nodes.OfType<BaseMethodDeclarationSyntax>().SelectMany(m => Guards.Missing(m, model).Select(p =>
                At(file, m, "guard", $"{Name(m)}: brak guard clause dla parametru '{p}'"))));
    }

    // if or loop with an if/loop above it in the same member; "else if" is a chain, not nesting.
    private static bool IsNested(StatementSyntax statement) =>
        IsBranchOrLoop(statement)
        && !(statement is IfStatementSyntax && statement.Parent is ElseClauseSyntax)
        && statement.Ancestors().TakeWhile(a => a is not MemberDeclarationSyntax and not LocalFunctionStatementSyntax and not AnonymousFunctionExpressionSyntax)
            .OfType<StatementSyntax>().Any(IsBranchOrLoop);

    private static bool IsBranchOrLoop(SyntaxNode node) =>
        node is IfStatementSyntax or ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax;

    private static SyntaxNode? Body(BaseMethodDeclarationSyntax method) => (SyntaxNode?)method.Body ?? method.ExpressionBody;

    private static int Lines(SyntaxNode? node)
    {
        if (node is null)
            return 0;
        var span = node.GetLocation().GetLineSpan();
        return span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
    }

    private static string Name(BaseMethodDeclarationSyntax method) => method switch
    {
        MethodDeclarationSyntax m => m.Identifier.Text,
        ConstructorDeclarationSyntax c => c.Identifier.Text + " (konstruktor)",
        _ => method.Kind().ToString(),
    };

    private static Violation At(string file, SyntaxNode node, string rule, string message) =>
        new(file, node.GetLocation().GetLineSpan().StartLinePosition.Line + 1, rule, message);
}
