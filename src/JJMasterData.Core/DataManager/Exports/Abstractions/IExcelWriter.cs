using System;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IExcelWriter : IWriter
{
    bool ShowBorder { get; set; }
    bool ShowRowStriped { get; set; }

    event EventHandler<GridCellEventArgs> OnRenderCell;
}
