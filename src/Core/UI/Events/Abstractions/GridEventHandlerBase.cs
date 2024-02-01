
using System.Threading.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public abstract class GridEventHandlerBase : IGridEventHandler
{
    public abstract string ElementName { get; }
    public virtual Task OnFilterLoadAsync(object sender, GridFilterLoadEventArgs eventArgs) => Task.CompletedTask;
    public virtual Task OnRenderCellAsync(object sender, GridCellEventArgs eventArgs) => Task.CompletedTask;
    public virtual Task OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs) => Task.CompletedTask;
    public virtual Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs) => Task.CompletedTask;
    public virtual Task OnRenderActionAsync(object sender, ActionEventArgs eventArgs) => Task.CompletedTask;
    public virtual Task OnRenderToolbarActionAsync(object sender, GridToolbarActionEventArgs eventArgs) => Task.CompletedTask;
    public virtual Task OnRenderRowAsync(object sender, GridRowEventArgs eventArgs) => Task.CompletedTask;
}
