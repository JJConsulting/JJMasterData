using System.Threading.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public abstract class GridEventHandlerBase : IGridEventHandler
{
    public abstract string ElementName { get; }

    public virtual void OnRenderCell(object sender, GridCellEventArgs eventArgs)
    {
    }

    public virtual Task OnRenderCellAsync(object sender, GridCellEventArgs eventArgs) => Task.CompletedTask;

    public virtual void OnRenderSelectedCell(object sender, GridSelectedCellEventArgs eventArgs)
    {
    }

    public virtual Task OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs) => Task.CompletedTask;

    public virtual void OnDataLoad(object sender, GridDataLoadEventArgs eventArgs)
    {
    }

    public virtual Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs) => Task.CompletedTask;

    public virtual void OnRenderAction(object sender, ActionEventArgs eventArgs)
    {
    }

    public virtual Task OnRenderActionAsync(object sender, ActionEventArgs eventArgs) => Task.CompletedTask;
}