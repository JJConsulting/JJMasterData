#nullable enable

using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class EntityParameters
{
    public Dictionary<string, object?> Filters { get; init; } = new Dictionary<string, object?>();
    public OrderByData OrderBy { get; init; } = new();
    public int CurrentPage { get; init; } = 1;
    public int RecordsPerPage { get; init; } = int.MaxValue;

    public void Deconstruct(
        out Dictionary<string, object?> parameters,
        out OrderByData orderBy,
        out int currentPage,
        out int recordsPerPage)
    {
        parameters = Filters;
        orderBy = OrderBy;
        currentPage = CurrentPage;
        recordsPerPage = RecordsPerPage;
    }
}