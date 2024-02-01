using System.Threading.Tasks;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public interface IGridEventHandler : IEventHandler
{
    public Task OnFilterLoadAsync(object sender, GridFilterLoadEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
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

    public Task OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
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
    public Task OnRenderActionAsync(object sender, ActionEventArgs eventArgs)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif
    Task OnRenderToolbarActionAsync(object sender, GridToolbarActionEventArgs e)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif
    Task OnRenderRowAsync(object sender, GridRowEventArgs e)
#if NET
    {
        return Task.CompletedTask;
    }
#else
        ;
#endif
}