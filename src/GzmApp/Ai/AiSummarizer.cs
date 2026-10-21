using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using GzmApp.Data;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace GzmApp.Ai;

// Sol6:Endpoint, Sol6:Model, Sol6:Kind ("openai" | "azure") in appsettings.json.
// Sol6:ApiKey only from dotnet user-secrets or the SOL6__APIKEY environment variable – never in the repository.
public sealed class Sol6Options
{
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Kind { get; set; } = "openai";
    public string ApiKey { get; set; } = string.Empty;
}

public interface IAiSummarizer
{
    Task<string> DescribeAsync(string task, IReadOnlyList<RecordField> fields, CancellationToken ct = default);
}

// Creates the chat client from Sol6:*; tests register their own (a fake IChatClient).
public delegate IChatClient ChatClientFactory(Sol6Options options);

// One IChatClient call per click. Missing key or a model error give a message, never an exception.
public sealed class AiSummarizer(IOptions<Sol6Options> options, ILogger<AiSummarizer> logger, ChatClientFactory? clientFactory = null) : IAiSummarizer
{
    public const string MissingKey = "Brak klucza SOL-6 – poproś prowadzącego";
    public const string MissingConfig = "Brak konfiguracji SOL-6 (Sol6:Endpoint, Sol6:Model) – poproś prowadzącego";
    public const string ModelError = "SOL-6 nie odpowiedział – spróbujcie za chwilę";
    public const string SystemPrompt = "Odpowiadaj tylko na podstawie przekazanych rekordów, nie wymyślaj faktów.";

    public async Task<string> DescribeAsync(string task, IReadOnlyList<RecordField> fields, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);
        ArgumentNullException.ThrowIfNull(fields);

        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            return MissingKey;
        if (string.IsNullOrWhiteSpace(config.Endpoint) || string.IsNullOrWhiteSpace(config.Model))
            return MissingConfig;
        return await AskAsync(config, task, fields, ct);
    }

    private async Task<string> AskAsync(Sol6Options config, string task, IReadOnlyList<RecordField> fields, CancellationToken ct)
    {
        try
        {
            var messages = new List<ChatMessage> { new(ChatRole.System, SystemPrompt), new(ChatRole.User, UserPrompt(task, fields)) };
            var response = await (clientFactory ?? CreateClient)(config).GetResponseAsync(messages, cancellationToken: ct);
            return string.IsNullOrWhiteSpace(response.Text) ? ModelError : response.Text.Trim();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Only the type of the error: the message could contain request details.
            logger.LogWarning("SOL-6: błąd wywołania ({ErrorType})", ex.GetType().Name);
            return ModelError;
        }
    }

    public static IChatClient CreateClient(Sol6Options config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return config.Kind == "azure"
            ? new AzureOpenAIClient(new Uri(config.Endpoint), new AzureKeyCredential(config.ApiKey)).GetChatClient(config.Model).AsIChatClient()
            : new OpenAIClient(new ApiKeyCredential(config.ApiKey), new OpenAIClientOptions { Endpoint = new Uri(config.Endpoint) })
                .GetChatClient(config.Model).AsIChatClient();
    }

    private static string UserPrompt(string task, IReadOnlyList<RecordField> fields) =>
        $"{task}\n\nRekord:\n{string.Join("\n", fields.Select(f => $"- {f.Name}: {f.Value}"))}";
}
