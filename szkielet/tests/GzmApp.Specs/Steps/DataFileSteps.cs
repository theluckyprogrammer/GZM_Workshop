using GzmApp.Specs.Data;
using GzmApp.Specs.Infrastructure;
using Spectree;

namespace GzmApp.Specs.Steps;

// Krok 0 (sNN-000-dane): the data file that the first prompt downloads – it exists, has all rows
// (E1.fileRows from checkpoints.md) and its header has every imported column. No app, no database:
// these are the first green tests, before the pair builds anything.
public sealed class DataFileSteps
{
    [Step("Plik z danymi `{plik}` istnieje w repozytorium.")]
    public void FileExists(string file)
    {
        Assert.True(File.Exists(FullPath(file)),
            $"Brak pliku {file}. Uruchomcie pierwszy prompt z karta.md (krok 0) – skrypt tools/nowy-portal.ps1 pobiera dane z portalu GZM.");
    }

    [Step("Plik z danymi ma pełną liczbę wierszy.")]
    public void FileHasAllRows()
    {
        var expected = int.Parse(ActiveScenario.Data.Value("E1.fileRows"));
        var actual = DataRows().Count;
        Assert.True(actual == expected,
            $"Plik ma {actual} wierszy danych, a zbiór z portalu ma {expected}. Coś zgubiło się przy pobieraniu – uruchomcie skrypt z kroku 0 jeszcze raz.");
    }

    [Step("Nagłówek pliku ma wszystkie kolumny, które w kroku 1 zaimportujecie do bazy.")]
    public void HeaderHasAllColumns()
    {
        var expected = ActiveScenario.Imported.Select(c => c.GetProperty("csv").GetString() ?? string.Empty).ToList();
        var header = ReadRecords().FirstOrDefault() ?? [];
        var missing = expected.Where(c => !header.Contains(c, StringComparer.Ordinal)).ToList();
        Assert.True(missing.Count == 0,
            $"W nagłówku pliku brakuje kolumn: {string.Join(", ", missing)}. To nie ten plik – uruchomcie skrypt z kroku 0 jeszcze raz.");
    }

    private static List<List<string>> DataRows() => ReadRecords().Skip(1).ToList();

    private static List<List<string>> ReadRecords()
    {
        var file = SourceText("file");
        Assert.True(File.Exists(FullPath(file)), $"Brak pliku {file} – uruchomcie pierwszy prompt (krok 0).");
        var text = RawCsv.Decode(File.ReadAllBytes(FullPath(file)));
        return RawCsv.Records(text, SourceText("separator").Single(), SourceInt("headerLine"));
    }

    private static string FullPath(string file) => Path.Combine(RepoPaths.Root, file);

    private static string SourceText(string property) =>
        ActiveScenario.Data.Mapping.GetProperty("source").GetProperty(property).GetString() ?? string.Empty;

    private static int SourceInt(string property) =>
        ActiveScenario.Data.Mapping.GetProperty("source").GetProperty(property).GetInt32();
}
