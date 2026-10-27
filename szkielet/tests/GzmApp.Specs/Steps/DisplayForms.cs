using System.Globalization;

namespace GzmApp.Specs.Steps;

// The ways a database value may be written on the page. The prompt asks for pl-PL numbers, yyyy-MM-dd dates and
// "tak"/"nie", but a step accepts every common form: the specification says what is shown, not how.
public static class DisplayForms
{
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static IReadOnlyList<string> Of(object? value) => value switch
    {
        null => [],
        string text => [Normalize(text)],
        bool flag => flag ? ["tak", "true", "✓", "✔"] : ["nie", "false", "✗", "✘"],
        DateTime date => Dates(date),
        TimeSpan time => [time.ToString(@"hh\:mm", Invariant), time.ToString(@"h\:mm", Invariant)],
        decimal number => Numbers(number),
        IConvertible number => Numbers(Convert.ToDecimal(number, Invariant)),
        _ => [Normalize(value.ToString() ?? string.Empty)],
    };

    public static bool Shows(string text, object? value) =>
        value is null || Of(value).Any(form => Normalize(text).Contains(form, StringComparison.OrdinalIgnoreCase));

    // Spaces of every kind (also the non-breaking space of pl-PL thousands) become one plain space.
    public static string Normalize(string text) =>
        string.Join(' ', text.Replace(' ', ' ').Replace(' ', ' ').Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    // "4 750,5" or "4750.5" → 4750.5; null when the text is not a number.
    public static decimal? Number(string text)
    {
        var plain = Normalize(text).Replace(" ", string.Empty).Replace(',', '.');
        return decimal.TryParse(plain, NumberStyles.Number, Invariant, out var number) ? number : null;
    }

    private static List<string> Numbers(decimal number) =>
    [
        number.ToString("0.##########", Polish), number.ToString("0.##########", Invariant),
        Normalize(number.ToString("N0", Polish)), Normalize(number.ToString("#,0.##########", Polish)),
        number.ToString("F2", Polish), number.ToString("F2", Invariant),
    ];

    private static List<string> Dates(DateTime date) => date.TimeOfDay == TimeSpan.Zero
        ? [date.ToString("yyyy-MM-dd", Invariant), date.ToString("dd.MM.yyyy", Invariant), date.ToString("d.MM.yyyy", Invariant)]
        : [date.ToString("yyyy-MM-dd HH:mm", Invariant), date.ToString("dd.MM.yyyy HH:mm", Invariant), date.ToString("yyyy-MM-ddTHH:mm", Invariant)];
}
