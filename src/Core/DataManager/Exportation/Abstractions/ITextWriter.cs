using System;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface ITextWriter : IExportationWriter
{
    string Delimiter { get; set; }

    event EventHandler<GridCellEventArgs> OnRenderCell;
    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
}