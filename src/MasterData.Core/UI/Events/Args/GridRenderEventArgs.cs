﻿using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Events.Args;

public class GridRenderEventArgs
{
    public required HtmlBuilder HtmlBuilder { get; init; }
}