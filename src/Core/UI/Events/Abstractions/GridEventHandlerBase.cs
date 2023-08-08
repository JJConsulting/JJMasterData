using System.Threading.Tasks;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.FormEvents.Abstractions;

namespace JJMasterData.Core.UI.FormEvents.Abstractions;

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
    public virtual void OnRenderHtml(object sender, GridRenderEventArgs eventArgs)
    {
    }

    public virtual Task OnRenderHtmlAsync(object sender, GridRenderEventArgs eventArgs) => Task.CompletedTask;
}