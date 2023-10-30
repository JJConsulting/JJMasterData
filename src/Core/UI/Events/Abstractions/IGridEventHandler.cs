using System.Threading.Tasks;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public interface IGridEventHandler : IEventHandler
{
    public void OnRenderCell(object sender, GridCellEventArgs eventArgs)
#if NET
    {
    }
#else
        ;
#endif
    public Task OnRenderCellAsync(object sender, GridCellEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif

    public void OnRenderSelectedCell(object sender, GridSelectedCellEventArgs eventArgs)
#if NET
    {
    }
#else
        ;
#endif

    public Task OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif

    public void OnDataLoad(object sender, GridDataLoadEventArgs eventArgs)
#if NET
    {
    }
#else
        ;
#endif
    public Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif

    public void OnRenderAction(object sender, ActionEventArgs eventArgs)
#if NET
    {
    }
#else
        ;
#endif

    public Task OnRenderActionAsync(object sender, ActionEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif
}