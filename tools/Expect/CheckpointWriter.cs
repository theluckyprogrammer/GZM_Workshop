using System.Text;

namespace Expect;

// scenarios/sNN/checkpoints.md: one table per stage, column ID is stable (tests and print templates read it).
public static class CheckpointWriter
{
    private static readonly (string Prefix, string Title)[] Stages =
    [
        ("E1.", "E1 – import (dane → baza)"),
        ("E2.", "E2 – endpoint z filtrowaniem"),
        ("E3.", "E3 – statystyka"),
        ("E4.", "E4 – ulubione"),
    ];

    public static string Render(ScenarioResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var text = new StringBuilder();
        AppendHeader(text, result);
        foreach (var (prefix, title) in Stages)
            AppendStage(text, title, result.Checkpoints.Where(c => c.Id.StartsWith(prefix, StringComparison.Ordinal)));
        return text.ToString().ReplaceLineEndings("\n");
    }

    private static void AppendHeader(StringBuilder text, ScenarioResult result)
    {
        var mapping = result.Mapping;
        var source = mapping.Source;
        text.AppendLine($"# Wartości kontrolne – {mapping.Scenario}: {mapping.Dataset}");
        text.AppendLine();
        text.AppendLine("Plik wygenerował `tools/Expect`. Nie edytuj ręcznie: zmień `mapping.json` i uruchom narzędzie ponownie.");
        text.AppendLine();
        text.AppendLine($"- Plik: `{result.Source.Path}` ({result.Source.EncodingName}, separator `{source.Separator}`, nagłówek w linii {source.HeaderLine})");
        text.AppendLine($"- Tabela: `{mapping.Table}`, klucz: {KeyText(mapping.Key)}");
        text.AppendLine($"- Atrybucja: {mapping.Attribution}");
        text.AppendLine();
        text.AppendLine("Zasady: wiersze liczymy od 1 bez nagłówka (= `SourceRow`). Tekst jest przycięty (Trim), pusty tekst = NULL.");
        text.AppendLine("Filtr: fragment tekstu bez rozróżniania wielkości liter (`LIKE '%fraza%'`). „Dziwny” rekord: pole puste albo nieparsowalne.");
        text.AppendLine("Znak `|` w wartości jest zapisany jako `\\|`, a nowa linia jako `\\n`.");
    }

    private static void AppendStage(StringBuilder text, string title, IEnumerable<Checkpoint> checkpoints)
    {
        text.AppendLine();
        text.AppendLine($"## {title}");
        text.AppendLine();
        text.AppendLine("| ID | Co sprawdzić | Wartość |");
        text.AppendLine("|---|---|---|");
        foreach (var checkpoint in checkpoints)
            text.AppendLine($"| {checkpoint.Id} | {Escape(checkpoint.Label)} | {Escape(checkpoint.Value)} |");
    }

    private static string KeyText(KeyMapping key) =>
        key.IsRowHash ? "`RowHash` (skrót wszystkich importowanych pól)" : string.Join(" + ", key.Fields.Select(f => $"`{f}`"));

    public static string Escape(string value) =>
        value.Replace("|", "\\|").ReplaceLineEndings("\\n");
}
