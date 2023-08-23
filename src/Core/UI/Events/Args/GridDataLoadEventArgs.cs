#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Core.FormEvents.Args;

/// <summary>
/// Event fired when data is ready to be loaded at GridView
/// </summary>
public class GridDataLoadEventArgs : EventArgs
{
    /// <summary>
    /// Filters sended to the IEntityRepository
    /// </summary>
    public IDictionary<string, object?>? Filters { get; init; }
    
    public OrderByData? OrderBy { get; init; }
    
    public int RecordsPerPage { get; init; }
    
    public int CurrentPage { get; init; }
    
    /// <summary>
    /// Total count of records at the entity
    /// </summary>
    public int TotalOfRecords { get; set; }
    
    public IList<Dictionary<string,object?>>? DataSource { get; set; }
}
