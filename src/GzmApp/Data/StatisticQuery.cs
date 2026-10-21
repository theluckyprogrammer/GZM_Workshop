using GzmApp.Data.Import;

namespace GzmApp.Data;

public sealed record StatisticGroup(string Label, int Count);

// Value: sum / avg; Groups: top5 / count-by. NULL values are left out, as in SQL aggregates.
public sealed record StatisticResult(string Description, string? Value, IReadOnlyList<StatisticGroup> Groups);

// The statistic tile from mapping.json ("statistic"), computed in SQL over the whole table.
public sealed class StatisticQuery
{
    private readonly SqlRunner _sql;
    private readonly ScenarioMapping _mapping;

    public StatisticQuery(SqlRunner sql, ScenarioMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(mapping);

        _sql = sql;
        _mapping = mapping;
    }

    public async Task<StatisticResult> RunAsync(CancellationToken ct = default) => _mapping.Statistic.Kind switch
    {
        "sum" => new(Description, DisplayValue.Format(await _sql.ExecuteScalarAsync<object>($"SELECT SUM({Field}) FROM {Table}", ct: ct)), []),
        "avg" => new(Description, DisplayValue.Average(await _sql.ExecuteScalarAsync<object>(
            $"SELECT ROUND(AVG(CAST({Field} AS decimal(18,2))), 2) FROM {Table}", ct: ct)), []),
        "top5" => new(Description, null, await GroupsAsync("TOP 5 ", ct)),
        _ => new(Description, null, await GroupsAsync(string.Empty, ct)),
    };

    private async Task<IReadOnlyList<StatisticGroup>> GroupsAsync(string top, CancellationToken ct)
    {
        var rows = await _sql.QueryAsync<dynamic>(
            $"SELECT {top}{Field} AS [Value], COUNT(*) AS [Count] FROM {Table} WHERE {Field} IS NOT NULL GROUP BY {Field} ORDER BY COUNT(*) DESC, {Field}", ct: ct);
        return rows.Select(r => (IDictionary<string, object?>)r)
            .Select(r => new StatisticGroup(DisplayValue.Format(r["Value"]), (int)r["Count"]!))
            .ToList();
    }

    private string Field => $"[{_mapping.Statistic.Field}]";

    private string Table => $"dbo.[{_mapping.Table}]";

    private string Description => _mapping.Statistic.Kind switch
    {
        "sum" => $"Suma: {_mapping.Statistic.Field}",
        "avg" => $"Średnia: {_mapping.Statistic.Field}",
        "top5" => $"Top 5: {_mapping.Statistic.Field}",
        _ => $"Liczba rekordów według: {_mapping.Statistic.Field}",
    };
}
