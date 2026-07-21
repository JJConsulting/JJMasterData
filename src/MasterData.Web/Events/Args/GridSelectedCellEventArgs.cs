#nullable disable warnings
using System;
using System.Collections.Generic;
using JJMasterData.Web.Components;

namespace JJMasterData.Web.Events.Args;

/// <summary>
/// Argumentos do evento utilizado para customizar o conteúdo do checkbox ao selecionar a linha da Grid
/// </summary>
public class GridSelectedCellEventArgs : EventArgs
{
    /// <summary>
    /// Linha atual com o valor de todos os campos
    /// </summary>
    public Dictionary<string,object> DataRow { get; internal set; }

    /// <summary>
    /// Objeto renderizado
    /// </summary>
    public JJCheckBox CheckBox { get; set; }
}
