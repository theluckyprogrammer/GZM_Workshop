using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GzmApp.Tests.Infrastructure;

// The app for one scenario on its own test database (TestServer, no port).
// UseSetting works before Program.cs reads the configuration, so "Scenario" and GZM_CONNSTR apply from the start.
public sealed class AppFactory : WebApplicationFactory<Program>
{
    public AppFactory(string scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        Scenario = scenario;
        Database = new TestDatabase(scenario);
    }

    public string Scenario { get; }

    public TestDatabase Database { get; }

    public HttpClient CreateBrowser() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");
        builder.UseSetting("Scenario", Scenario);
        builder.UseSetting("GZM_CONNSTR", Database.ConnectionString);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Database.DropAsync();
    }
}
