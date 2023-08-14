using System;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.FormEvents.Args;

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
    public DataRow DataRow { get; set; }

    /// <summary>
    /// Objeto renderizado
    /// </summary>
    public ComponentBase Sender { get; set; }

    /// <summary>
    /// Retorno esperado com o conteudo HTML renderizado
    /// </summary>
    public string HtmlResult { get; set; }
}
