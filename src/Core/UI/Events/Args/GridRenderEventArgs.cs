using System;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.FormEvents.Args;

public class GridRenderEventArgs : EventArgs
{
    public GridRenderEventArgs(JJGridView gridView)
    {
        GridView = gridView;
    }

    public JJGridView GridView { get; }
}