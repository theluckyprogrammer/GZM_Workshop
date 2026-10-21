using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

// Level 9, M-05 (branch ratunek/etap-3): the footer shows the CC BY 4.0 attribution of the scenario's data.
[Collection(AppsCollection.Name)]
public class AttributionTests(AppsFixture apps)
{
    public static TheoryData<string> Codes => ScenarioData.Codes;

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "M-05")]
    [Trait("Poziom", "9")]
    public async Task Footer_shows_the_attribution_from_mapping(string code)
    {
        var page = Html.Parse(await apps.For(code).CreateBrowser().GetStringAsync("/"));
        var footer = page.QuerySelector("#data-attribution")!.TextContent.Trim();

        Assert.Equal(ScenarioData.Get(code).Text("attribution"), footer);
        Assert.Contains("otwartedane.metropoliagzm.pl", footer, StringComparison.Ordinal);
    }
}
