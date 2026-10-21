using System.Globalization;

namespace GzmApp.Data;

// How a database value is shown on the page: dates yyyy-MM-dd, numbers with a Polish decimal comma.
public static class DisplayValue
{
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");

    public static string Format(object? value) => value switch
    {
        null or DBNull => string.Empty,
        DateTime date when date.TimeOfDay == TimeSpan.Zero => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        TimeSpan time => time.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
        bool flag => flag ? "tak" : "nie",
        decimal number => number.ToString("0.##########", Polish),
        IFormattable formattable => formattable.ToString(null, Polish),
        _ => value.ToString() ?? string.Empty,
    };

    public static string Average(object? value) =>
        value is null or DBNull ? "brak danych" : Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString("F2", Polish);
}
