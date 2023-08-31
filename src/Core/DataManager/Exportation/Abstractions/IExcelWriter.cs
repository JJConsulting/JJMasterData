using System;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IExcelWriter : IExportationWriter
{
    bool ShowBorder { get; set; }
    bool ShowRowStriped { get; set; }

    event EventHandler<GridCellEventArgs> OnRenderCell;
    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
}
