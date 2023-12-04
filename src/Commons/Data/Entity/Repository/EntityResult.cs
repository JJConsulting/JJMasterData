#nullable enable

namespace JJMasterData.Commons.Data.Entity.Repository;

public abstract class EntityResult<T>
{
    public T Data { get; }
    
    /// <summary>
    /// TotalOfRecords at the EntityResult source.
    /// </summary>
    public int TotalOfRecords { get; }

    protected EntityResult(T data, int totalOfRecords)
    {
        Data = data;
        TotalOfRecords = totalOfRecords;
    }
}