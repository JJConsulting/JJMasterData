using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Core.FormEvents.Args;

/// <summary>
/// Evento utilizado para recuperar os dados da tabela
/// </summary>
public class GridDataLoadEventArgs : EventArgs
{
    /// <summary>
    /// Filtros atuais
    /// </summary>
    public IDictionary<string, object> Filters { get; internal set; }

    /// <summary>
    /// Ordem atual
    /// </summary>
    public OrderByData OrderBy { get; internal set; }

    /// <summary>
    /// Records per page
    /// </summary>
    public int RecordsPerPage { get; internal set; }

    /// <summary>
    /// Página atual
    /// </summary>
    public int CurrentPage { get; internal set; }

    /// <summary>
    /// Retorna Objeto DataTable com os dados prontos para preencher a grid
    /// </summary>
    public DictionaryListResult DataSource { get; set; }
}
