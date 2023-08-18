#nullable enable

using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public class EntityResult<T>
{
    public IList<T> Data { get; }
    
    /// <summary>
    /// TotalOfRecords at the EntityResource source.
    /// </summary>
    public int TotalOfRecords { get;  }

    public int CurrentCount => Data.Count;
    
    public EntityResult(IList<T> data, int totalOfRecords)
    {
        Data = data;
        TotalOfRecords = totalOfRecords;
    }
}