using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace GzmApp.Ai;

// Registers IChatClient for the AI model (GPT-6 Luna on Azure AI Foundry, OpenAI-compatible endpoint .../openai/v1/).
// Pages get IChatClient and IOptions<Sol6Options> from DI; tests replace IChatClient with a fake one.
public static class Sol6Setup
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(45);

    public static IServiceCollection AddSol6(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<Sol6Options>(configuration.GetSection("Sol6"));
        services.AddSingleton<IChatClient>(sp => CreateClient(
            sp.GetRequiredService<IOptions<Sol6Options>>().Value,
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("AI")));
        return services;
    }

    // The real model with the log of every call; without Endpoint, Model or ApiKey every call gives Sol6Exception.MissingKey.
    // Also used by rule 9 of step 4 (tests/GzmApp.Specs), the only test that asks the real model.
    public static IChatClient CreateClient(Sol6Options options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        return new Sol6LoggingClient(options.IsConfigured ? Foundry(options) : new MissingKeyClient(), options.Model, logger);
    }

    private static IChatClient Foundry(Sol6Options options) =>
        new OpenAIClient(new ApiKeyCredential(options.ApiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(options.Endpoint),
            NetworkTimeout = Timeout,
            RetryPolicy = new ClientRetryPolicy(maxRetries: 1),
        }).GetChatClient(options.Model).AsIChatClient();
}
