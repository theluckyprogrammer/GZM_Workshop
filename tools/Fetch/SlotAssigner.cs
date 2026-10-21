namespace Fetch;

// Slot = scenario sNN. A rejected main dataset is replaced by the next accepted reserve.
// Without a reserve left the slot stays unfilled (Dataset is the rejected main) and waits for a decision.
public sealed record Slot(string Code, FetchedDataset Dataset, FetchedDataset? Replaced)
{
    public bool IsFilled => !Dataset.IsRejected;
}

public static class SlotAssigner
{
    public static IReadOnlyList<Slot> Assign(IReadOnlyList<FetchedDataset> mains, IReadOnlyList<FetchedDataset> reserves)
    {
        ArgumentNullException.ThrowIfNull(mains);
        ArgumentNullException.ThrowIfNull(reserves);

        var available = new Queue<FetchedDataset>(reserves.Where(r => !r.IsRejected));
        return mains.Select(main => ToSlot(main, available)).ToList();
    }

    private static Slot ToSlot(FetchedDataset main, Queue<FetchedDataset> available)
    {
        if (!main.IsRejected)
            return new Slot(main.Definition.Code, main, null);
        if (available.Count == 0)
            return new Slot(main.Definition.Code, main, null);

        return new Slot(main.Definition.Code, available.Dequeue(), main);
    }
}
