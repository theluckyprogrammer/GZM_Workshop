using System.Text;

namespace Fetch;

// docs/ZAMIANY.md: every replacement with its reason, or a statement that there were none.
public static class ReplacementReport
{
    public static string Build(IReadOnlyList<FetchedDataset> datasets, IReadOnlyList<Slot> slots)
    {
        ArgumentNullException.ThrowIfNull(datasets);
        ArgumentNullException.ThrowIfNull(slots);

        var text = new StringBuilder("# Zamiany zbiorów danych\n\n");
        text.AppendLine("Generuje `tools/Fetch`. Reguły odrzucenia: prompt 02.\n");
        var replaced = slots.Where(s => s.Replaced is not null).ToList();
        text.AppendLine(replaced.Count == 0 ? "Zamian nie było: wszystkie 15 zbiorów głównych spełnia reguły." : "## Zamiany\n");
        foreach (var slot in replaced)
            text.AppendLine(Line(slot));

        var unfilled = slots.Where(s => !s.IsFilled).ToList();
        text.AppendLine(unfilled.Count == 0 ? string.Empty : "\n## Brak zbioru rezerwowego (DO DECYZJI)\n");
        foreach (var slot in unfilled)
            text.AppendLine($"- {slot.Code}: {slot.Dataset.Definition.Label} (`{slot.Dataset.Definition.Id}`) odrzucony – {string.Join("; ", slot.Dataset.Rejections)}. Rezerwy R1–R5 wyczerpane.");

        var rejectedReserves = datasets.Where(d => d.Definition.IsReserve && d.IsRejected).ToList();
        text.AppendLine(rejectedReserves.Count == 0 ? string.Empty : "\n## Odrzucone zbiory rezerwowe\n");
        foreach (var reserve in rejectedReserves)
            text.AppendLine($"- {reserve.Definition.Code} {reserve.Definition.Label}: {string.Join("; ", reserve.Rejections)}");
        return text.ToString().TrimEnd() + "\n";
    }

    private static string Line(Slot slot)
    {
        var rejected = slot.Replaced!.Definition;
        var reserve = slot.Dataset.Definition;
        return $"- {slot.Code}: {rejected.Label} (`{rejected.Id}`) odrzucony – {string.Join("; ", slot.Replaced.Rejections)}. "
            + $"Zastąpiony przez {reserve.Code} {reserve.Label} (`{reserve.Id}`).";
    }
}
