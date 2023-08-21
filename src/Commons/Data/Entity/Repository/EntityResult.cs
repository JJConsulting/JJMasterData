#nullable enable

using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Data;

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