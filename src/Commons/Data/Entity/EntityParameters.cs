#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public record EntityParameters(
    IDictionary<string,dynamic>? Parameters = null,
    OrderByData? OrderBy = null,
    PaginationData? PaginationData = null
);

public record OrderByData(
    string FieldName,
    OrderByDirection Direction = OrderByDirection.Asc
)
{
    public override string ToString()
    {
        return FieldName + " " + Direction.ToString().ToUpper();
    }
}

public enum OrderByDirection
{
    Asc,
    Desc
}

public record PaginationData(int Page = 1, int RecordsPerPage = 5);