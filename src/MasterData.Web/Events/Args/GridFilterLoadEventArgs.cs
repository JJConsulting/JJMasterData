using System;
using System.Collections.Generic;

namespace JJMasterData.Web.Events.Args;

public class GridFilterLoadEventArgs : EventArgs
{
    public required Dictionary<string,object> Filters { get; init; }
}