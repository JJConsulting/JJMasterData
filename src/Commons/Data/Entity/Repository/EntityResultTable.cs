#nullable enable

using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Commons.Data.Entity;

public class EntityResultTable : EntityResult<DataTable>
{
    [SetsRequiredMembers]
    public EntityResultTable(DataTable data, int totalOfRecords) : base(data, totalOfRecords)
    {
    }
}