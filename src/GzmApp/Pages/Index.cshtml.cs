using System.Globalization;
using System.Text.Encodings.Web;
using GzmApp.Ai;
using GzmApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GzmApp.Pages;

public class IndexModel(IRecordRepository repository, IAiAnalyzer analyzer, HtmlEncoder encoder, IConfiguration configuration) : PageModel
{
    private const int Take = 20;
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");

    public string Scenario { get; private set; } = configuration["Scenario"] ?? "(brak scenariusza)";

    public RecordList Lista { get; private set; } = new(0, []);

    public long? Statystyka { get; private set; }

    public IReadOnlyList<MapRecord> Mapa { get; private set; } = [];

    public MapPodglad? Podglad { get; private set; }

    public int Deficyt { get; private set; }

    public string StatystykaTekst => Statystyka?.ToString("N0", Polish) ?? string.Empty;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Lista = await repository.SearchAsync(null, Take, ct);
        Statystyka = await repository.SumAvailableAsync(ct);
        await LoadMapAsync(ct);
    }

    public async Task<IActionResult> OnGetSzukajAsync(string? q, CancellationToken ct)
    {
        if (TooLong(q))
            return LongPhrase();

        var lista = await repository.SearchAsync(q, Take, ct);
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
        return Partial("_Mapa", preview);
    }

    private async Task<IActionResult> WholePageAsync(RecordList lista, CancellationToken ct)
    {
        Lista = lista;
        Statystyka = await repository.SumAvailableAsync(ct);
        await LoadMapAsync(ct);
        return Page();
    }

    private async Task LoadMapAsync(CancellationToken ct)
    {
        Mapa = await repository.MapAsync(ct);
        Deficyt = await repository.DeficitCountAsync(ct);
        Podglad = FirstDeficit(ct);
    }

    // The map preview starts on the first commune with a deficit, so a marker is visible at once.
    private MapPodglad? FirstDeficit(CancellationToken ct)
    {
        var deficit = Mapa.FirstOrDefault(r => r.AvailableBikes < r.RequiredBikes);
        var geo = deficit is null ? null : GzmMap.Of(deficit.Commune);
        return geo is null
            ? null
            : new MapPodglad(deficit!.Commune, deficit.AvailableBikes, deficit.RequiredBikes, geo.Lat, geo.Lon);
    }

    private bool IsHtmx() => Request.Headers["HX-Request"] == "true";

    private static bool TooLong(string? q) => q is not null && q.Length > 100;

    private ContentResult LongPhrase() => new()
    {
        StatusCode = 400,
        ContentType = "text/html; charset=utf-8",
        Content = "<p class=\"alert alert-warning\">Fraza jest za długa – wpisz najwyżej 100 znaków.</p>",
    };
}
