namespace Fetch;

// Rejection rules from prompt 02 that can be checked automatically.
public static class DatasetRules
{
    public const int MinRows = 30;
    public const int MaxRows = 20_000;
    public const int WorkCopyRows = 5_000;

    public static IReadOnlyList<string> Rejections(int rows, IReadOnlyList<ColumnProfile> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        var reasons = new List<string>();
        AddIf(reasons, rows < MinRows, $"mniej niż {MinRows} wierszy ({rows})");
        AddIf(reasons, rows > MaxRows, $"więcej niż {MaxRows} wierszy ({rows})");
        AddIf(reasons, !columns.Any(IsFilterField), "brak pola tekstowego do filtrowania");
        AddIf(reasons, !HasStatisticField(columns), "brak pola liczbowego, datowego lub kategorii do statystyki");
        return reasons;
    }

    public static bool NeedsWorkCopy(int rows) => rows > WorkCopyRows && rows <= MaxRows;

    private static bool IsFilterField(ColumnProfile column) => column.Type is "text" or "category";

    private static bool IsStatisticField(ColumnProfile column) => column.IsNumericOrDate || column.Type == "category";

    // Statistic and filter must be two different columns.
    private static bool HasStatisticField(IReadOnlyList<ColumnProfile> columns) =>
        columns.Where(IsStatisticField).Any(stat => columns.Any(filter => IsFilterField(filter) && filter != stat));

    private static void AddIf(List<string> reasons, bool condition, string reason)
    {
        if (condition)
            reasons.Add(reason);
    }
}
