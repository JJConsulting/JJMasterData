#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public record EntityParameters(
    IDictionary<string, object>? Parameters = null,
    OrderByData? OrderBy = null,
    PaginationData? PaginationData = null
);