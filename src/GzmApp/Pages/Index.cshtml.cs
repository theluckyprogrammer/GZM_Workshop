using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using GzmApp.Ai;
using GzmApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

public class IndexModel(IRecordRepository repository, IAiAnalyzer analyzer, HtmlEncoder encoder, IConfiguration configuration) : PageModel
{
    private const int Take = 20;
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string Scenario { get; private set; } = configuration["Scenario"] ?? "(brak scenariusza)";

    public RecordList Lista { get; private set; } = new(0, []);

    public long? Statystyka { get; private set; }

    public long? Wymagane { get; private set; }

    public IReadOnlyList<MapRecord> Mapa { get; private set; } = [];

    public IReadOnlyList<MapPointDto> PunktyMapy { get; private set; } = [];

    public IReadOnlyList<MapRecord> RankingDeficytow { get; private set; } = [];

    public MapPodglad? Podglad { get; private set; }

    public int Deficyt { get; private set; }

    public ListFilter Filtr { get; private set; } = ListFilter.All;

    public string? Fraza { get; private set; }

    public string StatystykaTekst => Statystyka?.ToString("N0", Polish) ?? string.Empty;

    public string WymaganeTekst => Wymagane?.ToString("N0", Polish) ?? string.Empty;

    public int PokrycieProcent
    {
        get
        {
            if (Statystyka is null or 0 || Wymagane is null or 0)
                return 0;
            return (int)Math.Clamp(Math.Round(Statystyka.Value * 100.0 / Wymagane.Value), 0, 999);
        }
    }

    public string PunktyMapyJson => JsonSerializer.Serialize(PunktyMapy, JsonOptions);

    public async Task OnGetAsync(string? q, string? filtr, CancellationToken ct)
    {
        Fraza = q;
        Filtr = ParseFilter(filtr);
        Lista = await repository.SearchAsync(q, Take, Filtr, ct);
        await LoadStatsAsync(ct);
    }

    public async Task<IActionResult> OnGetSzukajAsync(string? q, string? filtr, CancellationToken ct)
    {
        if (TooLong(q))
            return LongPhrase();

        Fraza = q;
        Filtr = ParseFilter(filtr);
        var lista = await repository.SearchAsync(q, Take, Filtr, ct);
        if (!IsHtmx())
            return await WholePageAsync(lista, ct);
        return Partial("_Lista", lista);
    }

    public async Task<IActionResult> OnGetAnalizujAsync(string? q, CancellationToken ct)
    {
        try
        {
            return Partial("_Analiza", await analyzer.AnalyzeAsync(q, ct));
        }
        catch (Sol6Exception ex)
        {
            return Content($"<div class=\"alert alert-warning\">{encoder.Encode(ex.Message)}</div>", "text/html; charset=utf-8");
        }
    }

    public async Task<IActionResult> OnGetMapaAsync(string? gmina, CancellationToken ct)
    {
        var record = string.IsNullOrWhiteSpace(gmina) ? null : await repository.FindByCommuneAsync(gmina, ct);
        var geo = record is null ? null : GzmMap.Of(record.Commune);
        var preview = geo is null ? null : new MapPodglad(record!.Commune, record.AvailableBikes, record.RequiredBikes, geo.Lat, geo.Lon);
        return Partial("_MapDetail", preview);
    }

    private async Task<IActionResult> WholePageAsync(RecordList lista, CancellationToken ct)
    {
        Lista = lista;
        await LoadStatsAsync(ct);
        return Page();
    }

    private async Task LoadStatsAsync(CancellationToken ct)
    {
        Statystyka = await repository.SumAvailableAsync(ct);
        Wymagane = await repository.SumRequiredAsync(ct);
        Mapa = await repository.MapAsync(ct);
        Deficyt = await repository.DeficitCountAsync(ct);
        PunktyMapy = BuildPoints(Mapa);
        RankingDeficytow = Mapa
            .Where(r => r.IsDeficit)
            .OrderBy(r => r.AvailableBikes - r.RequiredBikes)
            .Take(5)
            .ToList();
        Podglad = FirstDeficit();
    }

    private static IReadOnlyList<MapPointDto> BuildPoints(IReadOnlyList<MapRecord> mapa)
    {
        var list = new List<MapPointDto>(mapa.Count);
        foreach (var r in mapa)
        {
            var geo = GzmMap.Of(r.Commune);
            if (geo is null)
                continue;
            list.Add(new MapPointDto(r.Commune, geo.Lat, geo.Lon, r.AvailableBikes, r.RequiredBikes));
        }
        return list;
    }

    private MapPodglad? FirstDeficit()
    {
        var deficit = RankingDeficytow.FirstOrDefault() ?? Mapa.FirstOrDefault(r => r.IsDeficit);
        var geo = deficit is null ? null : GzmMap.Of(deficit.Commune);
        return geo is null
            ? null
            : new MapPodglad(deficit!.Commune, deficit.AvailableBikes, deficit.RequiredBikes, geo.Lat, geo.Lon);
    }

    private bool IsHtmx() => Request.Headers["HX-Request"] == "true";

    private static bool TooLong(string? q) => q is not null && q.Length > 100;

    private static ListFilter ParseFilter(string? filtr) => filtr?.Trim().ToLowerInvariant() switch
    {
        "deficyt" or "deficit" => ListFilter.Deficit,
        "nadwyzka" or "nadwyżka" or "surplus" => ListFilter.Surplus,
        _ => ListFilter.All,
    };

    private ContentResult LongPhrase() => new()
    {
        StatusCode = 400,
        ContentType = "text/html; charset=utf-8",
        Content = "<p class=\"alert alert-warning\">Fraza jest za długa – wpisz najwyżej 100 znaków.</p>",
    };
}

public sealed record MapPointDto(string Commune, double Lat, double Lon, int Available, int Required);
