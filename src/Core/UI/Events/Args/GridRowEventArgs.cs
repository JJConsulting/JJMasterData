using System;
using System.Collections.Generic;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Events.Args;

public class GridRowEventArgs : EventArgs
{
    public required HtmlBuilder HtmlBuilder { get; init; }
    public required Dictionary<string,object> RowValues { get; init; }
}