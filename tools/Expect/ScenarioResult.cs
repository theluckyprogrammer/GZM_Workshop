namespace Expect;

public sealed record ScenarioResult(ScenarioMapping Mapping, SourceTable Source, IReadOnlyList<Checkpoint> Checkpoints)
{
    public static ScenarioResult Compute(string root, ScenarioMapping mapping)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(mapping);

        var source = SourceReader.Read(root, mapping);
        EnsureEncoding(mapping, source);
        var set = RecordBuilder.Build(mapping, source);
        var checkpoints = Checks.Import(mapping, set)
            .Concat(Checks.Filter(mapping, set))
            .Concat(Statistics.Compute(mapping, set))
            .Concat(Checks.Favorite(mapping, set))
            .ToList();
        return new ScenarioResult(mapping, source, checkpoints);
    }

    private static void EnsureEncoding(ScenarioMapping mapping, SourceTable source)
    {
        if (source.EncodingName != mapping.Source.Encoding)
            throw new InvalidDataException($"Kodowanie pliku: {source.EncodingName}, a w mapping.json: {mapping.Source.Encoding}.");
    }
}
