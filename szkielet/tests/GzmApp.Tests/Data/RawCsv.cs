using System.Text;

namespace GzmApp.Tests.Data;

// A second, minimal CSV reader for checking tools/Expect – deliberately WITHOUT CsvHelper.
// Encoding: BOM → strict UTF-8 → Windows-1250. Quotes: "" inside quotes, separators and new lines inside quotes.
public static class RawCsv
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    static RawCsv() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static string Decode(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1250).GetString(bytes);
        }
    }

    // Records after skipping the lines above the header. Empty lines are not records.
    public static List<List<string>> Records(string text, char separator, int headerLine)
    {
        ArgumentNullException.ThrowIfNull(text);

        var body = string.Join('\n', text.Replace("\r\n", "\n").Split('\n').Skip(headerLine - 1));
        var reader = new RecordReader(separator);
        body.ToList().ForEach(reader.Push);
        return reader.Finish().Where(r => !(r.Count == 1 && r[0].Length == 0)).ToList();
    }

    private sealed class RecordReader(char separator)
    {
        private readonly List<List<string>> _records = [];
        private List<string> _fields = [];
        private readonly StringBuilder _field = new();
        private bool _quoted;
        private bool _quoteSeen;

        public void Push(char c)
        {
            if (_quoted) { PushQuoted(c); return; }
            if (c == '"' && _field.Length == 0) { _quoted = true; return; }
            if (c == separator) { EndField(); return; }
            if (c == '\n') { EndRecord(); return; }
            _field.Append(c);
        }

        public List<List<string>> Finish()
        {
            if (_field.Length > 0 || _fields.Count > 0)
                EndRecord();
            return _records;
        }

        // Inside quotes: "" is one quote, a single " ends the quoted part.
        private void PushQuoted(char c)
        {
            if (_quoteSeen && c == '"') { _field.Append('"'); _quoteSeen = false; return; }
            if (_quoteSeen) { _quoted = false; _quoteSeen = false; Push(c); return; }
            if (c == '"') { _quoteSeen = true; return; }
            _field.Append(c);
        }

        private void EndField()
        {
            _fields.Add(_field.ToString());
            _field.Clear();
        }

        private void EndRecord()
        {
            EndField();
            _records.Add(_fields);
            _fields = [];
        }
    }
}
