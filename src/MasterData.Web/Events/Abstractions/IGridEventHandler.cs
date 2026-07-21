using JJMasterData.Core.DataManager.Exportation;
using System.Threading.Tasks;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Web.Events.Args;

namespace JJMasterData.Web.Events.Abstractions;

public interface IGridEventHandler : IEventHandler
{
    public ValueTask OnFilterLoadAsync(object sender, GridFilterLoadEventArgs eventArgs)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
        ;
#endif
    
    public ValueTask OnRenderCellAsync(object sender, GridCellEventArgs eventArgs)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
        ;
#endif

    public ValueTask OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs)
#if NET
    {
        return ValueTask.CompletedTask;
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
    public ValueTask OnRenderActionAsync(object sender, ActionEventArgs eventArgs)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
        ;
#endif
    ValueTask OnRenderToolbarActionAsync(object sender, GridToolbarActionEventArgs e)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
        ;
#endif
    ValueTask OnRenderRowAsync(object sender, GridRowEventArgs e)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
        ;
#endif
}