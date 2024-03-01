#nullable enable
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Core.UI.Events.Args;

/// <summary>
/// Event fired when data is ready to be loaded at GridView
/// </summary>
public class GridDataLoadEventArgs : EventArgs
{
    /// <summary>
    /// Filters sended to the IEntityRepository
    /// </summary>
    public required Dictionary<string, object> Filters { get; init; }
    
    public OrderByData? OrderBy { get; init; }
    
    public int RecordsPerPage { get; init; }
    
    public int CurrentPage { get; init; }
    
    /// <summary>
    /// Total count of records at the entity
    /// </summary>
    public int TotalOfRecords { get; set; }
    
    public List<Dictionary<string, object>>? DataSource { get; set; }
}
