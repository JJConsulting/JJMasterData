using System.Threading.Tasks;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.FormEvents.Abstractions;

public interface IGridEventHandler : IEventHandler
{
    public void OnRenderCell(object sender, GridCellEventArgs eventArgs);
    public Task OnRenderCellAsync(object sender, GridCellEventArgs eventArgs);
    
    public void OnRenderSelectedCell(object sender, GridSelectedCellEventArgs eventArgs);
    public Task OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs);
    
    public void OnDataLoad(object sender, GridDataLoadEventArgs eventArgs);
    public Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs);
    
    public void OnRenderAction(object sender, ActionEventArgs eventArgs);
    public Task OnRenderActionAsync(object sender, ActionEventArgs eventArgs);
    public void OnGridViewCreated(JJGridView gridView);
}