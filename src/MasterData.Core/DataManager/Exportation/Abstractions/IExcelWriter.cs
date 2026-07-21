using JJMasterData.Core.DataManager.Exportation;
﻿using JJMasterData.Commons.Tasks;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExcelWriter : IExportationWriter
{
    bool ShowBorder { get; set; }
    bool ShowRowStriped { get; set; }

    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
}
