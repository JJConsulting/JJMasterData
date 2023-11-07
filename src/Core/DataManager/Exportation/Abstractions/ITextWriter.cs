using System;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface ITextWriter : IExportationWriter
{
    string Delimiter { get; set; }
    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
}