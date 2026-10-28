using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace GzmApp.Specs.Infrastructure;

public static partial class Html
{
    private static readonly HtmlParser Parser = new();

    public static IDocument Parse(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        return Parser.ParseDocument(html);
    }

    // Token for HTMX requests: <body hx-headers:inherited='{"RequestVerificationToken":"..."}'>.
    public static string BodyToken(string html)
    {
        var attribute = Parse(html).Body?.GetAttribute("hx-headers:inherited") ?? string.Empty;
        var match = TokenInJson().Match(attribute);
        return match.Success ? match.Groups[1].Value : throw new InvalidOperationException("Brak tokenu w <body hx-headers:inherited>.");
    }

    // Token of a normal form: <input name="__RequestVerificationToken" value="...">.
    public static string FormToken(string html) =>
        Parse(html).QuerySelector("input[name=__RequestVerificationToken]")?.GetAttribute("value")
        ?? throw new InvalidOperationException("Brak __RequestVerificationToken w formularzu.");

    [GeneratedRegex(@"""RequestVerificationToken""\s*:\s*""([^""]+)""")]
    private static partial Regex TokenInJson();
}
