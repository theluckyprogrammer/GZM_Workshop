using GzmApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace GzmApp.Specs.Steps;

// The pair's app on its own test database GzmTest_{scenario} (TestServer, no port, the F5 app keeps its database).
// A restarted app (Restart) shares the database and leaves it in place when disposed.
// The AI model is FakeModel: the tests use no key and no tokens, and its answers are known.
public sealed class SpecApp(string scenario, TestDatabase database, bool ownsDatabase) : WebApplicationFactory<Program>
{
    public string Scenario { get; } = scenario;

    public TestDatabase Database { get; } = database;

    public LogCapture Logs { get; } = new();

    public HttpClient CreateBrowser() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    public SpecApp Restart() => new(Scenario, Database, ownsDatabase: false);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");
        builder.UseSetting("Scenario", Scenario);
        builder.UseSetting("GZM_CONNSTR", Database.ConnectionString);
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatClient>();
            services.AddSingleton<IChatClient>(FakeModel.Instance);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (ownsDatabase)
            await Database.DropAsync();
    }
}
