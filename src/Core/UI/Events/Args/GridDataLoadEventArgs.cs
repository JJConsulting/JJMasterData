using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Core.FormEvents.Args;

/// <summary>
/// Evento utilizado para recuperar os dados da tabela
/// </summary>
public class GridDataLoadEventArgs : EventArgs
{
    /// <summary>
    /// Filtros atuais
    /// </summary>
    public IDictionary<string,dynamic> Filters { get; internal set; }

    /// <summary>
    /// Ordem atual
    /// </summary>
    public string OrderBy { get; internal set; }

    /// <summary>
    /// Records per page
    /// </summary>
    public int RegporPag { get; internal set; }

    /// <summary>
    /// Página atual
    /// </summary>
    public int CurrentPage { get; internal set; }

    /// <summary>
    /// Retorno do total de Registros
    /// </summary>
    public int Tot { get; set; }

    /// <summary>
    /// Retorna Objeto DataTable com os dados prontos para preencher a grid
    /// </summary>
    public DataTable DataSource { get; set; }
}
