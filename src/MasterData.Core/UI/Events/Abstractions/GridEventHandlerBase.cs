
using System.Threading.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public abstract class GridEventHandlerBase : IGridEventHandler
{
    public abstract string ElementName { get; }
    public virtual ValueTask OnFilterLoadAsync(object sender, GridFilterLoadEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual ValueTask OnRenderCellAsync(object sender, GridCellEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual ValueTask OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs) => Task.CompletedTask;
    public virtual ValueTask OnRenderActionAsync(object sender, ActionEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual ValueTask OnRenderToolbarActionAsync(object sender, GridToolbarActionEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual ValueTask OnRenderRowAsync(object sender, GridRowEventArgs eventArgs) => ValueTask.CompletedTask;
}
