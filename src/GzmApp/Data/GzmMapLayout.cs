namespace GzmApp.Data;

public sealed record GeoPoint(double Lat, double Lon);

// Approximate geographic centers of the GZM communes (OpenStreetMap view) and the bounding box
// of the whole metropolis, used by the map card on the home page.
public static class GzmMap
{
    public const double CenterLat = 50.30;
    public const double CenterLon = 18.90;
    public const double MinLat = 49.98;
    public const double MaxLat = 50.60;
    public const double MinLon = 18.30;
    public const double MaxLon = 19.50;

    private static readonly Dictionary<string, GeoPoint> Centers = new(StringComparer.Ordinal)
    {
        ["Bieruń"] = new(50.10, 19.09),
        ["Bobrowniki"] = new(50.42, 19.11),
        ["Bojszowy"] = new(50.07, 19.02),
        ["Bytom"] = new(50.35, 18.91),
        ["Będzin"] = new(50.33, 19.13),
        ["Chełm Śląski"] = new(50.13, 19.19),
        ["Chorzów"] = new(50.30, 18.95),
        ["Czeladź"] = new(50.31, 19.07),
        ["Dąbrowa Górnicza"] = new(50.32, 19.23),
        ["Gierałtowice"] = new(50.18, 18.63),
        ["Gliwice"] = new(50.29, 18.67),
        ["Imielin"] = new(50.15, 19.14),
        ["Katowice"] = new(50.26, 19.02),
        ["Knurów"] = new(50.23, 18.65),
        ["Kobiór"] = new(50.07, 18.97),
        ["Lędziny"] = new(50.21, 19.11),
        ["Mierzęcice"] = new(50.46, 19.05),
        ["Mikołów"] = new(50.17, 18.72),
        ["Mysłowice"] = new(50.24, 19.13),
        ["Piekary Śląskie"] = new(50.39, 18.95),
        ["Pilchowice"] = new(50.16, 18.62),
        ["Psary"] = new(50.46, 19.16),
        ["Pyskowice"] = new(50.40, 18.63),
        ["Radzionków"] = new(50.40, 18.91),
        ["Ruda Śląska"] = new(50.29, 18.86),
        ["Rudziniec"] = new(50.19, 18.45),
        ["Siemianowice Śląskie"] = new(50.32, 19.02),
        ["Siewierz"] = new(50.47, 19.23),
        ["Sosnowiec"] = new(50.29, 19.15),
        ["Sośnicowice"] = new(50.23, 18.60),
        ["Sławków"] = new(50.31, 19.39),
        ["Tarnowskie Góry"] = new(50.44, 18.86),
        ["Tychy"] = new(50.13, 18.99),
        ["Wojkowice"] = new(50.37, 19.08),
        ["Wyry"] = new(50.14, 18.66),
        ["Zabrze"] = new(50.32, 18.78),
        ["Zbrosławice"] = new(50.20, 18.75),
        ["Łaziska Górne"] = new(50.14, 18.78),
        ["Świerklaniec"] = new(50.49, 18.90),
        ["Świętochłowice"] = new(50.30, 18.92),
    };

    public static GeoPoint? Of(string commune) =>
        Centers.TryGetValue(commune, out var point) ? point : null;
}

// One commune prepared for the map preview: numbers from the table plus its coordinates.
public sealed record MapPodglad(string Commune, int AvailableBikes, int RequiredBikes, double Lat, double Lon);
