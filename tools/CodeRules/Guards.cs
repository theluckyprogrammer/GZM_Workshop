using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeRules;

// Public methods and constructors of public types: every parameter of a non-nullable reference type must be checked
// (ArgumentNullException.ThrowIfNull, ArgumentException.ThrowIfNullOrWhiteSpace/Empty, Ensure.*, "?? throw",
// "is null" check) or passed on unchanged to such a check in a base/this constructor call.
public static class Guards
{
    private static readonly string[] Checks = ["ThrowIfNull", "ThrowIfNullOrWhiteSpace", "ThrowIfNullOrEmpty"];

    public static IEnumerable<string> Missing(BaseMethodDeclarationSyntax method, SemanticModel model)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(model);

        if (!IsPublicApi(method, model) || (method.Body is null && method.ExpressionBody is null))
            return [];
        var body = (SyntaxNode?)method.Body ?? method.ExpressionBody!;
        return method.ParameterList.Parameters
            .Where(p => NeedsGuard(model.GetDeclaredSymbol(p)))
            .Select(p => p.Identifier.Text)
            .Where(name => !IsGuarded(body, name))
            .ToList();
    }

    private static bool IsPublicApi(BaseMethodDeclarationSyntax method, SemanticModel model) =>
        model.GetDeclaredSymbol(method) is { DeclaredAccessibility: Accessibility.Public, IsOverride: false } symbol
        && symbol.ContainingType.DeclaredAccessibility == Accessibility.Public
        && !symbol.ContainingType.IsRecord
        && symbol.Name != "Main";

    private static bool NeedsGuard(IParameterSymbol? parameter) =>
        parameter is { Type.IsReferenceType: true, NullableAnnotation: not NullableAnnotation.Annotated, HasExplicitDefaultValue: false }
        && parameter.Type.TypeKind != TypeKind.Error
        && parameter.Type.SpecialType != SpecialType.System_Object;

    private static bool IsGuarded(SyntaxNode body, string name) =>
        body.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(i => IsCheck(i) && Mentions(i, name))
        || body.DescendantNodes().OfType<BinaryExpressionSyntax>().Any(b => b.IsKind(SyntaxKind.CoalesceExpression) && b.Right is ThrowExpressionSyntax && Mentions(b.Left, name))
        || body.DescendantNodes().OfType<IsPatternExpressionSyntax>().Any(p => Mentions(p.Expression, name));

    private static bool IsCheck(InvocationExpressionSyntax invocation) =>
        invocation.Expression is MemberAccessExpressionSyntax member
        && (Checks.Contains(member.Name.Identifier.Text) || member.Expression.ToString() == "Ensure");

    private static bool Mentions(SyntaxNode node, string name) =>
        node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(i => i.Identifier.Text == name);
}
