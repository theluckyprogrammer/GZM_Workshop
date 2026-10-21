namespace GzmApp.Tests.Infrastructure;

// One AppFactory per scenario, created on first use and shared by all tests of the "Apps" collection.
// At the end every factory is disposed and its GzmTest_{scenario} database dropped.
public sealed class AppsFixture : IAsyncLifetime
{
    private readonly Dictionary<string, AppFactory> _apps = [];
    private readonly Lock _lock = new();

    public AppFactory For(string scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario);

        lock (_lock)
        {
            if (!_apps.TryGetValue(scenario, out var app))
                _apps[scenario] = app = new AppFactory(scenario);
            return app;
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var app in _apps.Values)
            await app.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class AppsCollection : ICollectionFixture<AppsFixture>
{
    public const string Name = "Apps";
}
