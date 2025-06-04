#nullable enable

namespace JJMasterData.Commons.Data.Entity.Repository;

public abstract class EntityResult<T>(T data, int totalOfRecords)
{
    public T Data { get; } = data;

    /// <summary>
    /// TotalOfRecords at the EntityResult source.
    /// </summary>
    public int TotalOfRecords { get; } = totalOfRecords;
}