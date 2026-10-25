using Microsoft.Extensions.AI;

namespace GzmApp.Specs.Steps;

// The AI model of the tests: SpecApp puts it in DI in place of the real model (no key, no tokens, always the same answer).
// It remembers what the app sent and how many times it was asked. The steps run one after another (AssemblyInfo).
// UseRealModel passes the calls on to the real model (rule 9 of step 4) until the next Reset.
public sealed class FakeModel : IChatClient
{
    public const string DefaultAnswer = "- Nr 7 jest najciekawszy.\n- To odpowiedź udawanego modelu z testów.";

    private Func<IReadOnlyList<ChatMessage>, CancellationToken, Task<ChatResponse>> _answer = Fixed(null);

    public static FakeModel Instance { get; } = new();

    public int Calls { get; private set; }

    public string SentText { get; private set; } = string.Empty;

    // The text of the last answer of the real model (UseRealModel), null when it did not answer.
    public string? RealAnswer { get; private set; }

    // Before every step: no calls yet and the default answer.
    public void Reset(Func<ChatResponse>? answer = null)
    {
        Clear();
        _answer = Fixed(answer);
    }

    public void UseRealModel(IChatClient model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Clear();
        _answer = async (messages, ct) =>
        {
            var response = await model.GetResponseAsync(messages, cancellationToken: ct);
            RealAnswer = response.Text;
            return response;
        };
    }

    public static ChatResponse Reply(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new(new ChatMessage(ChatRole.Assistant, text));
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var sent = messages.ToList();
        Calls++;
        SentText = string.Join("\n", sent.Select(m => m.Text));
        return await _answer(sent, cancellationToken);
    }

    private void Clear()
    {
        Calls = 0;
        SentText = string.Empty;
        RealAnswer = null;
    }

    private static Func<IReadOnlyList<ChatMessage>, CancellationToken, Task<ChatResponse>> Fixed(Func<ChatResponse>? answer) =>
        (_, _) => Task.FromResult((answer ?? (() => Reply(DefaultAnswer)))());

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Krok 4 woła model jednym GetResponseAsync, bez strumieniowania.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
