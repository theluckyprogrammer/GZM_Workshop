using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace GzmApp.Tests.Infrastructure;

// The app for one scenario on its own test database (TestServer, no port).
// UseSetting works before Program.cs reads the configuration, so "Scenario" and GZM_CONNSTR apply from the start.
public class AppFactory : WebApplicationFactory<Program>
{
    private readonly Action<IWebHostBuilder>? _configure;

    // configure: extra settings or test services (e.g. a fake IChatClient) for one test class.
    public AppFactory(string scenario, string databaseSuffix = "", Action<IWebHostBuilder>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        Scenario = scenario;
        Database = new TestDatabase(scenario, databaseSuffix);
        _configure = configure;
    }

    public string Scenario { get; }

    public TestDatabase Database { get; }

    public LogCapture Logs { get; } = new();

    public HttpClient CreateBrowser() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");
        builder.UseSetting("Scenario", Scenario);
        builder.UseSetting("GZM_CONNSTR", Database.ConnectionString);
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        _configure?.Invoke(builder);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Database.DropAsync();
    }
}
