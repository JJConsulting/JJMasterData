using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.FormEvents.Args;

/// <summary>
/// Evento utilizado para recuperar os dados da tabela
/// </summary>
public class GridDataLoadEventArgs : EventArgs
{
    public IDictionary<string,dynamic> Filters { get; internal set; }
    
    /// <summary>
    /// Current order
    /// </summary>
    public string OrderBy { get; internal set; }

    /// <summary>
    /// Records per page
    /// </summary>
    public int RecordsPerPage { get; internal set; }

    /// <summary>
    /// Current page
    /// </summary>
    public int CurrentPage { get; internal set; }

    /// <summary>
    /// DataSource containing data ready to populate the grid
    /// </summary>
    public DataSource DataSource { get; set; }

}
