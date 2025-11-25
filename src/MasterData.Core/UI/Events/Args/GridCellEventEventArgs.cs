#nullable enable
using System;
using System.Collections.Generic;
using JJConsulting.Html;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;


namespace JJMasterData.Core.UI.Events.Args;

/// <summary>
/// Event arguments used to customize the rendered content in a grid cell.
/// </summary>
public class GridCellEventArgs : EventArgs
{
    /// <summary>
    /// The current form field being rendered.
    /// </summary>
    public required FormElementField Field { get; set; }

    /// <summary>
    /// The current row data containing values for all fields.
    /// </summary>
    public required Dictionary<string, object?> DataRow { get; set; }

    /// <summary>
    /// The component responsible for rendering the content.
    /// </summary>
    public required ComponentBase Sender { get; set; }

    /// <summary>
    /// The expected result containing the rendered HTML content.
    /// </summary>
    public HtmlBuilder? HtmlResult { get; set; }
}