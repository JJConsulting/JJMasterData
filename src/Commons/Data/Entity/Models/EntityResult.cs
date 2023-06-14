using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public record EntityResult<T>
(
    IEnumerable<T> Data,
    int TotalOfRecords
)
{
    public IEnumerable<T> Data { get; } = Data;
    public int TotalOfRecords { get; } = TotalOfRecords;
}