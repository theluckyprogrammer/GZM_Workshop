using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using GzmApp.Tests.Data;
using GzmApp.Tests.Infrastructure;

namespace GzmApp.Tests;

// Level 1: files in data/raw, data/metadata.json, mapping.json and checkpoints.md agree with each other.
public partial class DataTests
{
    private static readonly JsonElement Metadata =
        JsonDocument.Parse(File.ReadAllText(Path.Combine(RepoPaths.Root, "data", "metadata.json"))).RootElement.Clone();

    public static TheoryData<string> Codes => ScenarioData.Codes;

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "D-01")]
    [Trait("Poziom", "1")]
    public void File_has_SHA256_and_row_count_from_metadata(string code)
    {
        var meta = MetaFor(code);
        var path = Path.Combine(RepoPaths.Root, meta.GetProperty("file").GetProperty("path").GetString()!);

        Assert.Equal(meta.GetProperty("file").GetProperty("sha256").GetString(), Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))));
        Assert.Equal(meta.GetProperty("content").GetProperty("rows").GetInt32(), new IndependentScenario(ScenarioData.Get(code)).FileRows);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "D-02")]
    [Trait("Poziom", "1")]
    public void Headers_in_file_metadata_and_mapping_are_the_same(string code)
    {
        var scenario = ScenarioData.Get(code);
        var inMetadata = MetaFor(code).GetProperty("format").GetProperty("headers").EnumerateArray().Select(h => h.GetString()!).ToList();
        var inMapping = scenario.Mapping.GetProperty("columns").EnumerateArray().Select(c => c.GetProperty("csv").GetString()!).ToList();

        Assert.Equal(inMetadata, new IndependentScenario(scenario).Headers);
        Assert.Equal(inMetadata, inMapping);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "D-03")]
    [Trait("Poziom", "1")]
    public void License_and_attribution_are_filled_in(string code)
    {
        var license = MetaFor(code).GetProperty("license");

        Assert.False(string.IsNullOrWhiteSpace(license.GetProperty("title").GetString()));
        Assert.StartsWith("Źródło: ", license.GetProperty("attribution").GetString());
        Assert.Equal(license.GetProperty("attribution").GetString(), ScenarioData.Get(code).Text("attribution"));
    }

    [Theory]
    [MemberData(nameof(Codes))]
    [Trait("Spec", "D-04")]
    [Trait("Poziom", "1")]
    public void Second_parser_gives_the_values_of_checkpoints(string code)
    {
        var scenario = ScenarioData.Get(code);
        var data = new IndependentScenario(scenario);

        Assert.Equal(scenario.Value("E1.rows"), data.Rows.Count.ToString());
        Assert.Equal(Plain(scenario.Value("E1.row7.raw")), Normalize(data.Row7.Cells[scenario.Text("checkField")].Raw));
        Assert.Equal(scenario.Value("E1.row7.value"), data.Row7.Cells[scenario.Text("checkField")].Text);
        Assert.Equal(scenario.Value("E1.weird"), data.Rows.Count(r => r.Weird).ToString());
        AssertWeirdColumns(scenario, data);
        AssertFilter(scenario, data);
        Assert.Equal(StatisticOf(scenario), IndependentStatistic.Compute(data).Select(s => s.Value).ToList());
        Assert.Equal(Plain(scenario.Value("E4.title")), Normalize(data.Row7.Cells[scenario.Text("titleField")].Text!));
    }

    // Personal data rules of prompt 02. Mobile-phone hits were reviewed by hand (findings.md, step 02):
    // s10 FID_1 is part of a GUID, s13 additionalInfo is a number published by the institution.
    [Fact]
    [Trait("Spec", "D-05")]
    [Trait("Poziom", "1")]
    public void No_PESEL_or_email_and_phone_numbers_only_where_reviewed()
    {
        var reviewed = new[] { ("s10", "FID_1"), ("s13", "additionalInfo") };
        var hits = ScenarioData.All.SelectMany(s => PhoneHits(new IndependentScenario(s), s.Code)).Distinct().ToList();

        Assert.Empty(ScenarioData.All.Where(s => Pesel().IsMatch(RawText(s)) || Email().IsMatch(RawText(s))).Select(s => s.Code));
        Assert.All(hits, h => Assert.Contains(h, reviewed));
    }

    private static void AssertWeirdColumns(Scenario scenario, IndependentScenario data) =>
        Assert.All(data.Imported.Select(c => c.GetProperty("field").GetString()!), f =>
            Assert.Equal(scenario.Value($"E1.weird.{f}"), $"{data.Rows.Count(r => r.Cells[f].Empty)} / {data.Rows.Count(r => r.Cells[f].Bad)}"));

    private static void AssertFilter(Scenario scenario, IndependentScenario data)
    {
        var matches = data.Matching(scenario.Value("E2.phrase"), scenario.Mapping.GetProperty("filter").GetProperty("field").GetString()!);
        Assert.Equal(scenario.Value("E2.count"), matches.Count.ToString());
        Assert.Equal(scenario.Numbered("E2.result").Select(c => Plain(c.Value)), matches.Select(r => Normalize(r.Cells[scenario.Text("titleField")].Text ?? "NULL")));
    }

    private static List<string> StatisticOf(Scenario scenario) =>
        scenario.Checkpoints.ContainsKey("E3.value") ? [scenario.Value("E3.value")] : scenario.Numbered("E3.group").Select(c => c.Value).ToList();

    // Every column of the raw file (also the ones that are not imported) with a value that looks like a mobile number.
    private static IEnumerable<(string, string)> PhoneHits(IndependentScenario data, string code)
    {
        var source = data.Scenario.Mapping.GetProperty("source");
        var records = RawCsv.Records(RawText(data.Scenario), source.GetProperty("separator").GetString()![0], source.GetProperty("headerLine").GetInt32());
        return data.Headers.Where((_, i) => records.Skip(1).Any(r => i < r.Count && Phone().IsMatch(r[i]))).Select(h => (code, h));
    }

    private static string RawText(Scenario s) =>
        RawCsv.Decode(File.ReadAllBytes(Path.Combine(RepoPaths.Root, s.Mapping.GetProperty("source").GetProperty("file").GetString()!)));

    private static JsonElement MetaFor(string code) =>
        Metadata.GetProperty("datasets").EnumerateArray().Single(d =>
            d.GetProperty("status").GetString() == "used"
            && (d.TryGetProperty("scenario", out var s) && s.GetString() == code || d.GetProperty("code").GetString() == code));

    private static string Plain(string checkpoint) => ScenarioData.Unescape(checkpoint);

    private static string Normalize(string value) => value.Trim().Replace("\r\n", "\n");

    [GeneratedRegex(@"\b\d{11}\b")]
    private static partial Regex Pesel();

    [GeneratedRegex(@"[\w.+-]+@[\w-]+(?:\.[\w-]+)+")]
    private static partial Regex Email();

    [GeneratedRegex(@"(?<![\d,.])(?:\+48[ -]?)?(?:45|5\d|6\d|7[2-9]|8[18])\d[ -]?\d{3}[ -]?\d{3}(?![\d,.])")]
    private static partial Regex Phone();
}
