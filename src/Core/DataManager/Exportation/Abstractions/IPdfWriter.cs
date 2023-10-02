using System;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IPdfWriter : IExportationWriter
{
    event EventHandler<GridCellEventArgs> OnRenderCell;
    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
    public FormElement FormElement { get; set; }

    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }

    public bool IsLandscape { get; set; }
}