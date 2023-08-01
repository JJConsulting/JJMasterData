using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Commons.Data.Entity;

public record EntityResult
(
    DataSource Data,
    int TotalOfRecords
)
{
    public DataSource Data { get; } = Data;
    public int TotalOfRecords { get; } = TotalOfRecords;
}