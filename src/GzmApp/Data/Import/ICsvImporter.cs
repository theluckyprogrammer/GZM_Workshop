namespace GzmApp.Data.Import;

public sealed record ImportResult(string Table, int TableRows, int WeirdRecords, TimeSpan Elapsed);

public interface ICsvImporter
{
    // Creates the table (db/sNN_create.sql) and imports the CSV. Running it again does not change the number of rows.
    Task<ImportResult> ImportAsync(CancellationToken ct = default);

    // At startup: imports only when the scenario table is empty. Returns null when the import was skipped.
    Task<ImportResult?> ImportIfEmptyAsync(CancellationToken ct = default);
}
