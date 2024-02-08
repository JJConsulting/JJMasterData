using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Events.Args;

/// <summary>
/// Argumento utilizado para customizar o conteúdo renderizado em uma celula
/// </summary>
public class GridCellEventArgs : EventArgs
{
    /// <summary>
    /// Campo atual que esta sendo renderizado
    /// </summary>
    public FormElementField Field { get; set; }

    /// <summary>
    /// Linha atual com o valor de todos os campos
    /// </summary>
    public Dictionary<string,object> DataRow { get; set; }

    /// <summary>
    /// Objeto renderizado
    /// </summary>
    public ComponentBase Sender { get; set; }

    /// <summary>
    /// Retorno esperado com o conteudo HTML renderizado
    /// </summary>
    public HtmlBuilder HtmlResult { get; set; }
}
