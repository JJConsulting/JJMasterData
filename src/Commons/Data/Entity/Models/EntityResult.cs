#nullable enable

using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity;

public class EntityResult<T>
{
    public List<T> Data { get; }
    
    /// <summary>
    /// TotalOfRecords at the EntityResource source.
    /// </summary>
    public int TotalOfRecords { get;  }

    public int CurrentCount => Data.Count;
    
    public EntityResult(List<T> data, int totalOfRecords)
    {
        Data = data;
        TotalOfRecords = totalOfRecords;
    }
}