using System.Net;
using GzmApp.Ai;
using GzmApp.Tests.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace GzmApp.Tests;

// Level 9, AI (branch ratunek/etap-4): "✨ Opisz" with a fake IChatClient, without a key and (optionally) for real.
public sealed class AiTests : IAsyncLifetime
{
    private const string Code = "s02";
    private const string Key = "TEST-SOL6-KLUCZ-0123456789";

    private readonly FakeChatClient _chat = new();
    private AppFactory _withKey = null!;
    private AppFactory _withoutKey = null!;

    public Task InitializeAsync()
    {
        _withKey = new AppFactory(Code, "_ai", b => b
            .UseSetting("Sol6:ApiKey", Key).UseSetting("Sol6:Endpoint", "http://sol6.test/v1").UseSetting("Sol6:Model", "test")
            .ConfigureTestServices(s => s.AddSingleton<ChatClientFactory>(_ => _chat)));
        _withoutKey = new AppFactory(Code, "_ai_bez_klucza", b => b.UseSetting("Sol6:ApiKey", string.Empty));
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _withKey.DisposeAsync();
        await _withoutKey.DisposeAsync();
    }

    [Fact]
    [Trait("Spec", "AI-01")]
    [Trait("Poziom", "9")]
    public async Task Fake_model_gives_an_HTML_fragment_and_gets_only_the_record_fields()
    {
        var (status, html) = await DescribeRecord7Async(_withKey);
        var prompt = _chat.LastMessages.Single(m => m.Role == ChatRole.User).Text;

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.DoesNotContain("<html", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(FakeChatClient.Answer, html, StringComparison.Ordinal);
        Assert.Equal(AiSummarizer.SystemPrompt, _chat.LastMessages.Single(m => m.Role == ChatRole.System).Text);
        Assert.Contains($"- Name: {ScenarioData.Unescape(ScenarioData.Get(Code).Value("E4.title"))}", prompt, StringComparison.Ordinal);
        Assert.Equal(ScenarioData.Get(Code).Mapping.GetProperty("columns").EnumerateArray().Count(c => !c.TryGetProperty("import", out var i) || i.GetBoolean()),
            prompt.Split('\n').Count(l => l.StartsWith("- ", StringComparison.Ordinal)));
    }

    [Fact]
    [Trait("Spec", "AI-02")]
    [Trait("Poziom", "9")]
    public async Task Missing_key_gives_a_friendly_message_not_an_exception()
    {
        var (status, html) = await DescribeRecord7Async(_withoutKey);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("Brak klucza SOL-6 – poproś prowadzącego", WebUtility.HtmlDecode(html), StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Spec", "AI-03")]
    [Trait("Poziom", "9")]
    public async Task Key_is_not_in_the_logs_also_after_a_model_error()
    {
        _chat.FailNext = true;
        var (status, html) = await DescribeRecord7Async(_withKey);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("SOL-6 nie odpowiedział", WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        Assert.DoesNotContain(_withKey.Logs.Entries, e => e.Message.Contains(Key, StringComparison.Ordinal));
    }

    // Only with a real key (SOL6__APIKEY + SOL6__ENDPOINT + SOL6__MODEL in the environment): ONE short call for s02.
    [Sol6KeyFact]
    [Trait("Spec", "AI-04")]
    [Trait("Poziom", "9")]
    public async Task Real_SOL6_call_describes_record_7()
    {
        await using var real = new AppFactory(Code, "_ai_prawdziwy");
        var (status, html) = await DescribeRecord7Async(real);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.DoesNotContain(AiSummarizer.ModelError, WebUtility.HtmlDecode(html), StringComparison.Ordinal);
    }

    private static async Task<(HttpStatusCode, string)> DescribeRecord7Async(AppFactory app)
    {
        var client = app.CreateBrowser();
        var id = Html.Parse(await client.GetStringAsync("/")).QuerySelectorAll("#lista > li")
            .First(li => li.TextContent.Contains("Nr 7:", StringComparison.Ordinal)).GetAttribute("data-record-id");
        var request = new HttpRequestMessage(HttpMethod.Get, $"/?handler=Podsumuj&id={id}");
        request.Headers.Add("HX-Request", "true");
        var response = await client.SendAsync(request);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private sealed class FakeChatClient : IChatClient
    {
        public const string Answer = "Opis z fałszywego modelu.";

        public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];

        public bool FailNext { get; set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            LastMessages = messages.ToList();
            if (FailNext)
            {
                FailNext = false;
                throw new HttpRequestException($"401 dla klucza {Key}");
            }
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, Answer)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}

// Skipped unless a real SOL-6 key and address are set (SOL6__APIKEY, SOL6__ENDPOINT, SOL6__MODEL).
public sealed class Sol6KeyFactAttribute : FactAttribute
{
    public Sol6KeyFactAttribute()
    {
        var ready = new[] { "SOL6__APIKEY", "SOL6__ENDPOINT", "SOL6__MODEL" }.All(v => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)));
        Skip = ready ? null : "Pominięte: brak SOL6__APIKEY / SOL6__ENDPOINT / SOL6__MODEL (prawdziwe wywołanie SOL-6).";
    }
}
