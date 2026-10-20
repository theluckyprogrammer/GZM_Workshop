using System.Text;

namespace GzmApp.Data.Import;

public sealed record DetectedEncoding(string Name, Encoding Encoding, int PreambleLength)
{
    public string Decode(byte[] bytes) => Encoding.GetString(bytes, PreambleLength, bytes.Length - PreambleLength);
}

// BOM first, then strict UTF-8, then fallback to Windows-1250.
public static class EncodingDetector
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static DetectedEncoding Detect(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (StartsWith(bytes, 0xEF, 0xBB, 0xBF))
            return new DetectedEncoding("UTF-8 z BOM", new UTF8Encoding(true), 3);
        if (StartsWith(bytes, 0xFF, 0xFE))
            return new DetectedEncoding("UTF-16 LE z BOM", new UnicodeEncoding(false, true), 2);
        if (IsStrictUtf8(bytes))
            return new DetectedEncoding("UTF-8", StrictUtf8, 0);

        return new DetectedEncoding("Windows-1250", Encoding.GetEncoding(1250), 0);
    }

    private static bool StartsWith(byte[] bytes, params byte[] prefix) =>
        bytes.Length >= prefix.Length && bytes.AsSpan(0, prefix.Length).SequenceEqual(prefix);

    private static bool IsStrictUtf8(byte[] bytes)
    {
        try
        {
            StrictUtf8.GetCharCount(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
