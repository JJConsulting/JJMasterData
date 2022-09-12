using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IPdfWriter : IWriter
{
    event EventHandler<GridCellEventArgs> OnRenderCell;

    public FormElement FormElement { get; set; }

    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }

    public bool IsLandscape { get; set; }
}