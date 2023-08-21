#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public class EntityParameters
{
    public IDictionary<string, object?> Parameters { get; init; } = new Dictionary<string, object?>();
    public OrderByData OrderBy { get; init; } = new();
    public int CurrentPage { get; init; } = 1;
    public int RecordsPerPage { get; init; } = int.MaxValue;
    
    public void Deconstruct(
        out IDictionary<string, object?> parameters,
        out OrderByData orderBy,
        out int currentPage,
        out int recordsPerPage)
    {
        parameters = Parameters;
        orderBy = OrderBy;
        currentPage = CurrentPage;
        recordsPerPage = RecordsPerPage;
    }
}