using System.Text.Json;
using Expect;

namespace Cards.Print;

public sealed record SheetIdea(string Title, string Text);

// Hand-written story per scenario (tools/Cards/opisy.json): who uses the app, what the screen looks like, where it can grow.
public sealed record SheetStory(string Story, string Screen, IReadOnlyList<SheetIdea> Future);

// print/opis-scenariusza-sNN.html: one printed page per scenario – an imaginative description of the app (no expected values).
public static class ScenarioSheet
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyDictionary<string, SheetStory> LoadStories(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return JsonSerializer.Deserialize<Dictionary<string, SheetStory>>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException($"{path}: pusty plik.");
    }

    public static string Render(string template, ScenarioMapping mapping, ScenarioText text, SheetStory story)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(story);

        var variables = new Dictionary<string, string>
        {
            ["app"] = InlineHtml.Encode(text.App), ["code"] = InlineHtml.Encode(mapping.Scenario),
            ["idea"] = InlineHtml.Encode(text.Idea), ["dataset"] = InlineHtml.Encode(mapping.Dataset),
            ["attribution"] = InlineHtml.Encode(mapping.Attribution), ["favorites"] = InlineHtml.Encode(text.Favorites),
            ["story"] = InlineHtml.Encode(story.Story), ["screen"] = InlineHtml.Encode(story.Screen),
            ["future"] = string.Join("\n", story.Future.Select(i => $"  <li><strong>{InlineHtml.Encode(i.Title)}{(i.Title.EndsWith('?') ? "" : ".")}</strong> {InlineHtml.Encode(i.Text)}</li>")),
        };
        return new TemplateRenderer(variables, [], []).Render(template);
    }
}
