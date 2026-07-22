
using System.Threading.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public abstract class GridEventHandlerBase : IGridEventHandler
{
    public abstract string ElementName { get; }
    public virtual ValueTask OnFilterLoadAsync(object sender, GridFilterLoadEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual void OnRenderCell(object? sender, GridCellEventArgs eventArgs) { }
    public virtual void OnRenderSelectedCell(object? sender, GridSelectedCellEventArgs eventArgs) { }
    public virtual Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs) => Task.CompletedTask;
    public virtual void OnRenderAction(object? sender, ActionEventArgs eventArgs) { }
    public virtual ValueTask OnRenderToolbarActionAsync(object sender, GridToolbarActionEventArgs eventArgs) => ValueTask.CompletedTask;
    public virtual void OnRenderRow(object? sender, GridRowEventArgs eventArgs) { }
}
