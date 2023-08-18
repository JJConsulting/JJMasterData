using System;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.FormEvents.Args;

/// <summary>
/// Argumentos do evento utilizado para customizar o conteúdo do checkbox ao selecionar a linha da Grid
/// </summary>
public class GridSelectedCellEventArgs : EventArgs
{
    /// <summary>
    /// Linha atual com o valor de todos os campos
    /// </summary>
    public IDictionary<string,dynamic> DataRow { get; internal set; }

    /// <summary>
    /// Objeto renderizado
    /// </summary>
    public JJCheckBox CheckBox { get; set; }
}
