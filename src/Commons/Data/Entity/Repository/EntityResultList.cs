#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Commons.Data.Entity;

public class EntityResultList : EntityResult<List<Dictionary<string, object>>>
{
    [SetsRequiredMembers]
    public EntityResultList(List<Dictionary<string, object>> data, int totalOfRecords) : base(data, totalOfRecords)
    {
    }
}