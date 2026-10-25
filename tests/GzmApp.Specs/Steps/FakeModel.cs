using Microsoft.Extensions.AI;

namespace GzmApp.Specs.Steps;

// The AI model of the tests: SpecApp puts it in DI in place of SOL-6 (no key, no tokens, always the same answer).
// It remembers what the app sent and how many times it was asked. The steps run one after another (AssemblyInfo).
public sealed class FakeModel : IChatClient
{
    public const string DefaultAnswer = "- Nr 7 jest najciekawszy.\n- To odpowiedź udawanego modelu z testów.";

    private Func<ChatResponse> _answer = () => Reply(DefaultAnswer);

    public static FakeModel Instance { get; } = new();

    public int Calls { get; private set; }

    public string SentText { get; private set; } = string.Empty;

    // Before every step: no calls yet and the default answer.
    public void Reset(Func<ChatResponse>? answer = null)
    {
        Calls = 0;
        SentText = string.Empty;
        _answer = answer ?? (() => Reply(DefaultAnswer));
    }

    public static ChatResponse Reply(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new(new ChatMessage(ChatRole.Assistant, text));
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        Calls++;
        SentText = string.Join("\n", messages.Select(m => m.Text));
        return Task.FromResult(_answer());
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Krok 4 woła model jednym GetResponseAsync, bez strumieniowania.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
