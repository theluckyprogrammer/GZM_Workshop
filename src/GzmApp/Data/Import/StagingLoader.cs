using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace GzmApp.Data.Import;

// Records → #Staging. Up to 5000 rows: INSERT through Dapper; more: SqlBulkCopy.
public static class StagingLoader
{
    public const int BulkCopyThreshold = 5000;

    public static async Task LoadAsync(
        SqlRunner sql, SqlTransaction transaction, ScenarioMapping mapping, IReadOnlyList<ImportRecord> records, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(records);

        var columns = RecordTableBuilder.StagingColumns(mapping);
        if (records.Count > BulkCopyThreshold)
        {
            await sql.BulkCopyAsync(transaction, RecordTableBuilder.StagingTable, ToDataTable(mapping, columns, records), ct);
            return;
        }

        var insert = $"INSERT INTO {RecordTableBuilder.StagingTable} ({string.Join(", ", columns.Select(c => $"[{c}]"))}) " +
                     $"VALUES ({string.Join(", ", columns.Select((_, i) => $"@p{i}"))});";
        await sql.ExecuteManyAsync(transaction, insert, records.Select(r => (object)ToParameters(r)).ToList(), ct);
    }

    private static object?[] RowValues(ImportRecord record) =>
        [record.SourceRow, .. record.Values, record.RowHash, record.IsWeird];

    // DateTime as datetime2: the old datetime type does not accept dates before 1753.
    private static DynamicParameters ToParameters(ImportRecord record)
    {
        var parameters = new DynamicParameters();
        var values = RowValues(record);
        for (var i = 0; i < values.Length; i++)
            parameters.Add($"p{i}", values[i], values[i] is DateTime ? DbType.DateTime2 : null);
        return parameters;
    }

    private static DataTable ToDataTable(ScenarioMapping mapping, IReadOnlyList<string> columns, IReadOnlyList<ImportRecord> records)
    {
        var types = new List<Type> { typeof(int) };
        types.AddRange(mapping.Imported.Select(c => ClrType(SqlType.Parse(c.SqlType!))));
        types.AddRange([typeof(byte[]), typeof(bool)]);

        var table = new DataTable();
        table.Columns.AddRange(columns.Select((c, i) => new DataColumn(c, types[i])).ToArray());
        foreach (var record in records)
            table.Rows.Add(RowValues(record).Select(v => v ?? DBNull.Value).ToArray());
        return table;
    }

    private static Type ClrType(SqlType type) => type.Kind switch
    {
        SqlKind.Text => typeof(string),
        SqlKind.Int => typeof(int),
        SqlKind.BigInt => typeof(long),
        SqlKind.Decimal => typeof(decimal),
        SqlKind.Bit => typeof(bool),
        SqlKind.Time => typeof(TimeSpan),
        _ => typeof(DateTime),
    };
}
