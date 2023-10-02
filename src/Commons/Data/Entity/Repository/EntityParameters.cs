#nullable enable

using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class EntityParameters
{
    public IDictionary<string, object?> Filters { get; init; }
    public OrderByData OrderBy { get; init; }
    public int CurrentPage { get; init; }
    public int RecordsPerPage { get; init; }
    
    public EntityParameters()
    {
        Filters = new Dictionary<string, object?>();
        OrderBy = new OrderByData();
        CurrentPage = 1;
        RecordsPerPage = int.MaxValue;
    }
    
    public void Deconstruct(
        out IDictionary<string, object?> parameters,
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