using System.Globalization;
using System.Text;

namespace GzmApp.Data;

// Wyszukiwanie bez rozróżniania wielkości liter i ogonków (odpowiednik COLLATE Latin1_General_CI_AI
// z SQL Servera). SqlRunner rejestruje tę logikę w SQLite jako funkcję SQL plain().
public static class PlainText
{
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");

    public static string? Of(string? text) =>
        string.IsNullOrEmpty(text) ? text
        : string.Concat(text.ToLower(Polish).Replace('ł', 'l').Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
}
