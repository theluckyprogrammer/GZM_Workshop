using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

public class SmokeTests
{
    private static readonly string[] Required = ["E1.rows", "E1.row7.value", "E1.weird", "E2.phrase", "E2.count", "E3.kind", "E4.title"];

    [Fact]
    [Trait("Spec", "D-00")]
    [Trait("Poziom", "0")]
    public void ScenarioData_finds_15_scenarios_with_values_E1_to_E4()
    {
        Assert.Equal(15, ScenarioData.All.Count);
        Assert.All(ScenarioData.All, s => Assert.All(Required, id => Assert.False(string.IsNullOrWhiteSpace(s.Value(id)), $"{s.Code}: puste {id}")));
        Assert.All(ScenarioData.All, s => Assert.True(s.Numbered("E2.result").Count > 0 && s.Checkpoints.Keys.Any(k => k is "E3.value" || k.StartsWith("E3.group.", StringComparison.Ordinal)), $"{s.Code}: brak E2.result albo wyniku E3"));
    }
}

[Collection(AppsCollection.Name)]
public class InfrastructureTests(AppsFixture apps)
{
    [Fact]
    [Trait("Spec", "INF-01")]
    [Trait("Poziom", "0")]
    public async Task AppFactory_starts_the_app_on_its_own_test_database()
    {
        var app = apps.For("s02");
        var home = await app.CreateBrowser().GetAsync("/");

        Assert.Equal(System.Net.HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(1, await app.Database.ScalarAsync<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'AspNetUsers'"));
    }
}
