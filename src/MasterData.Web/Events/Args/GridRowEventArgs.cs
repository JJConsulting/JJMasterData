using System;
using System.Collections.Generic;
using JJConsulting.Html;


namespace JJMasterData.Web.Events.Args;

public class GridRowEventArgs : EventArgs
{
    public required HtmlBuilder HtmlBuilder { get; init; }
    public required Dictionary<string,object> RowValues { get; init; }
}