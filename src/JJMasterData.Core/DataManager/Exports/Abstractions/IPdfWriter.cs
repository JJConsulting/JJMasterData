using System;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IPdfWriter : IExportationWriter
{
    event EventHandler<GridCellEventArgs> OnRenderCell;



    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }

    public bool IsLandscape { get; set; }
}