using JJMasterData.Core.DataManager.Exportation;
﻿using JJMasterData.Commons.Tasks;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface ITextWriter : IExportationWriter
{
    string Delimiter { get; set; }
    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
}