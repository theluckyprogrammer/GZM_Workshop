using System.Net;
using System.Text.RegularExpressions;

namespace Cards.Print;

// Markdown inside one line → HTML: text is encoded, `code` and **bold** become tags.
public static partial class InlineHtml
{
    public static string Encode(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return WebUtility.HtmlEncode(text);
    }

    public static string FromMarkdown(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var html = Code().Replace(Encode(markdown), m => $"<code>{m.Groups[1].Value}</code>");
        return Bold().Replace(html, m => $"<strong>{m.Groups[1].Value}</strong>");
    }

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex Code();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex Bold();
}
