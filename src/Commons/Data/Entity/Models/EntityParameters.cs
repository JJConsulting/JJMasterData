#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public record EntityParameters(
    IDictionary<string,dynamic>? Parameters = null,
    OrderByData? OrderBy = null,
    PaginationData? PaginationData = null
);