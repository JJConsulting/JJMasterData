using System;
using System.Collections.Generic;

namespace JJMasterData.Core.UI.Events.Args;

public class GridFilterLoadEventArgs : EventArgs
{
    public required Dictionary<string,object> Filters { get; init; }
}