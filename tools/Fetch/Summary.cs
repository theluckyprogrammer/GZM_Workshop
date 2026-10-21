namespace Fetch;

public static class Summary
{
    public static void Print(IReadOnlyList<DatasetMeta> metas)
    {
        ArgumentNullException.ThrowIfNull(metas);

        Console.WriteLine("scenariusz | zbiór | wiersze | kodowanie | separator | uwaga");
        foreach (var meta in metas)
            Console.WriteLine(Line(meta));
    }

    private static string Line(DatasetMeta m)
    {
        var separator = m.Format?.Separator == "\t" ? "TAB" : m.Format?.Separator ?? "-";
        return $"{m.Scenario ?? "-"} | {m.Code} | {m.Content?.Rows.ToString() ?? "-"} | {m.Format?.Encoding ?? "-"} | {separator} | {Note(m)}";
    }

    private static string Note(DatasetMeta m)
    {
        var notes = new List<string?>
        {
            m.Status == "rejected" ? "ODRZUCONY: " + string.Join("; ", m.RejectionReasons) : null,
            m.ReplacesCode is null ? null : $"zastępuje {m.ReplacesCode}",
            m.Format?.HeaderLine > 1 ? $"nagłówek w linii {m.Format.HeaderLine}" : null,
            m.File?.WorkCopyPath is null ? null : "kopia robocza 5000",
            m.Content?.InconsistentRows > 0 ? $"{m.Content.InconsistentRows} wierszy z inną liczbą pól" : null,
            m.PiiForManualReview.Count > 0 ? "PII do przeglądu: " + string.Join(", ", m.PiiForManualReview.Select(p => $"{p.Kind}×{p.Count}")) : null,
        };
        return string.Join("; ", notes.Where(n => n is not null));
    }
}
