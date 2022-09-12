using System;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface ITextWriter : IWriter
{
    string Delimiter { get; set; }

    event EventHandler<GridCellEventArgs> OnRenderCell;
}