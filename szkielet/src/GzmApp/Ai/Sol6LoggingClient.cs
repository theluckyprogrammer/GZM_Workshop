using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace GzmApp.Ai;

// Writes every call of the model to the log (category "AI"), so the pair sees in the terminal that the AI worked:
// how much text went to the model, how long it took and how many tokens it used. Never the key or the text itself.
// Any error of the model becomes Sol6Exception with a short Polish message.
public sealed class Sol6LoggingClient(IChatClient inner, string model, ILogger logger) : DelegatingChatClient(inner)
{
    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var sent = messages.ToList();
        logger.LogInformation("AI ({Model}): wysyłam do modelu {Messages} wiadomości, {Chars} znaków", model, sent.Count, sent.Sum(m => m.Text.Length));
        var clock = Stopwatch.StartNew();
        var response = await AnswerOrSol6ErrorAsync(sent, options, cancellationToken);
        logger.LogInformation("AI ({Model}): odpowiedź po {Seconds:0.0} s, tokeny: wejście {Input}, wyjście {Output}",
            model, clock.Elapsed.TotalSeconds, response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount);
        return response;
    }

    private async Task<ChatResponse> AnswerOrSol6ErrorAsync(List<ChatMessage> messages, ChatOptions? options, CancellationToken ct)
    {
        try
        {
            return await base.GetResponseAsync(messages, options, ct);
        }
        catch (Exception ex) when (ex is not Sol6Exception and not OperationCanceledException)
        {
            // Only the type of the error: its message could contain details of the request.
            logger.LogWarning("AI ({Model}): błąd wywołania ({ErrorType})", model, ex.GetType().Name);
            throw new Sol6Exception(Sol6Exception.NoAnswer);
        }
    }
}

// Used when Sol6:Endpoint, Sol6:Model or Sol6:ApiKey is missing: every call ends with the message for the pair.
public sealed class MissingKeyClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new Sol6Exception(Sol6Exception.MissingKey);

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new Sol6Exception(Sol6Exception.MissingKey);

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
