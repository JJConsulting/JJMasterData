using JJMasterData.Commons.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExcelWriter : IExportationWriter
{
    bool ShowBorder { get; set; }
    bool ShowRowStriped { get; set; }
    
    event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
}
