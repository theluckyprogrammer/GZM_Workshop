using System.Text.RegularExpressions;
using Expect;

namespace Cards;

// Reads the tables of scenarios/sNN/checkpoints.md. Values stay escaped (\| and \n), exactly as in the file.
public static partial class CheckpointFile
{
    public static IReadOnlyList<Checkpoint> Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return File.ReadLines(path)
            .Select(line => Row().Match(line))
            .Where(match => match.Success)
            .Select(match => new Checkpoint(match.Groups["id"].Value, match.Groups["label"].Value, match.Groups["value"].Value))
            .ToList();
    }

    public static string Value(this IReadOnlyList<Checkpoint> checkpoints, string id) =>
        checkpoints.FirstOrDefault(c => c.Id == id)?.Value
        ?? throw new InvalidDataException($"Brak wartości {id} w checkpoints.md.");

    [GeneratedRegex(@"^\| (?<id>E\d\.\S+) \| (?<label>.*?) \| (?<value>(?:[^|\\]|\\.)*) \|$")]
    private static partial Regex Row();
}
