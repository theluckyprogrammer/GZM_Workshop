using System.Text.Json;
using System.Text.RegularExpressions;
using GzmApp.Data.Import;
using GzmApp.Tests.Data;
using GzmApp.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GzmApp.Tests;

// Level 2 (branch ratunek/etap-1): the import at start-up, checked in the database against checkpoints.md
// and against the second parser from level 1.
[Collection(AppsCollection.Name)]
public partial class ImportTests(AppsFixture apps)
{
    public static TheoryData<string> Codes => ScenarioData.Codes;

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-01")]
    [Trait("Poziom", "2")]
    public async Task Row_count_after_import_equals_E1(string code)
    {
        var app = await StartedAsync(code);

        Assert.Equal(ScenarioData.Get(code).Value("E1.rows"), (await CountAsync(app, code)).ToString());
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-02")]
    [Trait("Poziom", "2")]
    public async Task Check_field_in_row_7_equals_checkpoint(string code)
    {
        var scenario = ScenarioData.Get(code);
        var values = await ColumnAsync(await StartedAsync(code), scenario, Column(scenario, scenario.Text("checkField")));

        Assert.Equal(scenario.Value("E1.row7.value"), values.Single(v => v.SourceRow == 7).Value);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-03")]
    [Trait("Poziom", "2")]
    public async Task Polish_letters_are_kept(string code)
    {
        var scenario = ScenarioData.Get(code);
        var data = new IndependentScenario(scenario);
        var sample = TextColumns(data).SelectMany(f => data.Rows.Where(r => PolishLetter().IsMatch(r.Cells[f].Text ?? "")).Select(r => (Field: f, Row: r))).FirstOrDefault();
        if (sample.Row is null)
            return;
        var values = await ColumnAsync(await StartedAsync(code), scenario, Column(scenario, sample.Field));

        Assert.Equal(sample.Row.Cells[sample.Field].Text, values.Single(v => v.SourceRow == sample.Row.SourceRow).Value);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-04")]
    [Trait("Poziom", "2")]
    public Task Decimal_values_equal_the_file(string code) => AssertColumnsAsync(code, t => t.StartsWith("decimal", StringComparison.Ordinal));

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-05")]
    [Trait("Poziom", "2")]
    public Task Dates_follow_the_format_from_mapping(string code) =>
        AssertColumnsAsync(code, t => t is "date" || t.StartsWith("datetime2", StringComparison.Ordinal) || t.StartsWith("time", StringComparison.Ordinal));

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-06")]
    [Trait("Poziom", "2")]
    public async Task Empty_or_unreadable_fields_are_NULL(string code)
    {
        var scenario = ScenarioData.Get(code);
        var app = await StartedAsync(code);
        foreach (var column in Imported(scenario))
        {
            var field = column.GetProperty("field").GetString()!;
            var expected = scenario.Value($"E1.weird.{field}").Split(" / ").Sum(int.Parse);
            Assert.Equal(expected, (await ColumnAsync(app, scenario, column)).Count(v => v.Value is null));
        }
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "E1-07")]
    [Trait("Poziom", "2")]
    public async Task Second_import_keeps_the_row_count(string code)
    {
        var app = await StartedAsync(code);
        var result = await app.Services.GetRequiredService<ICsvImporter>().ImportAsync();

        Assert.Equal(ScenarioData.Get(code).Value("E1.rows"), result.TableRows.ToString());
        Assert.Equal(result.TableRows, await CountAsync(app, code));
    }

    // A test trigger throws in the middle of the MERGE: the transaction is rolled back, the table stays empty.
    [Fact]
    [Trait("Spec", "E1-08")]
    [Trait("Poziom", "2")]
    public async Task Import_interrupted_halfway_leaves_no_partial_data()
    {
        const string code = "s06";
        var app = await StartedAsync(code);
        var table = ScenarioData.Get(code).Text("table");
        var importer = app.Services.GetRequiredService<ICsvImporter>();
        await app.Database.ExecuteAsync($"DELETE FROM dbo.[{table}]");
        await app.Database.ExecuteAsync($"CREATE OR ALTER TRIGGER dbo.[TR_Test_Break] ON dbo.[{table}] AFTER INSERT AS THROW 50000, 'Przerwany import (test)', 1;");
        try
        {
            await Assert.ThrowsAnyAsync<Exception>(() => importer.ImportAsync());
            Assert.Equal(0, await CountAsync(app, code));
        }
        finally
        {
            await app.Database.ExecuteAsync("DROP TRIGGER IF EXISTS dbo.[TR_Test_Break]");
            await importer.ImportAsync();
        }
    }

    private async Task<AppFactory> StartedAsync(string code)
    {
        var app = apps.For(code);
        await app.CreateBrowser().GetAsync("/");
        return app;
    }

    private async Task AssertColumnsAsync(string code, Func<string, bool> type)
    {
        var scenario = ScenarioData.Get(code);
        var data = new IndependentScenario(scenario);
        var columns = Imported(scenario).Where(c => type(c.GetProperty("sqlType").GetString()!)).ToList();
        var app = await StartedAsync(code);
        foreach (var column in columns)
        {
            var field = column.GetProperty("field").GetString()!;
            var expected = data.Rows.Select(r => (r.SourceRow, r.Cells[field].Text)).OrderBy(r => r.SourceRow).ToList();
            Assert.Equal(expected, (await ColumnAsync(app, scenario, column)).OrderBy(r => r.SourceRow).ToList());
        }
    }

    // The value as text, in the same form as checkpoints.md (style 121: yyyy-mm-dd hh:mi:ss).
    private static Task<IReadOnlyList<(int SourceRow, string? Value)>> ColumnAsync(AppFactory app, Scenario scenario, JsonElement column)
    {
        var field = $"[{column.GetProperty("field").GetString()}]";
        var type = column.GetProperty("sqlType").GetString()!;
        var text = type.StartsWith("time", StringComparison.Ordinal) ? $"LEFT(CONVERT(nvarchar(4000), {field}, 121), 5)" : $"CONVERT(nvarchar(4000), {field}, 121)";
        return app.Database.PairsAsync($"SELECT [SourceRow], {text} FROM dbo.[{scenario.Text("table")}]");
    }

    private static Task<int> CountAsync(AppFactory app, string code) =>
        app.Database.ScalarAsync<int>($"SELECT COUNT(*) FROM dbo.[{ScenarioData.Get(code).Text("table")}]");

    private static IEnumerable<JsonElement> Imported(Scenario scenario) =>
        scenario.Mapping.GetProperty("columns").EnumerateArray().Where(c => !c.TryGetProperty("import", out var i) || i.GetBoolean());

    private static JsonElement Column(Scenario scenario, string field) =>
        Imported(scenario).First(c => c.GetProperty("field").GetString() == field);

    private static IEnumerable<string> TextColumns(IndependentScenario data) =>
        data.Imported.Where(c => c.GetProperty("sqlType").GetString()!.StartsWith("nvarchar", StringComparison.Ordinal)).Select(c => c.GetProperty("field").GetString()!);

    [GeneratedRegex("[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]")]
    private static partial Regex PolishLetter();
}
