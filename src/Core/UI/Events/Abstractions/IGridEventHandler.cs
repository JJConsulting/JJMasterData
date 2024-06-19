using System.Threading.Tasks;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

public interface IGridEventHandler : IEventHandler
{
    public ValueTask OnFilterLoadAsync(object sender, GridFilterLoadEventArgs eventArgs)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif
    
    public ValueTask OnRenderCellAsync(object sender, GridCellEventArgs eventArgs)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif

    public ValueTask OnRenderSelectedCellAsync(object sender, GridSelectedCellEventArgs eventArgs)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif
    
    public ValueTask OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif
    public ValueTask OnRenderActionAsync(object sender, ActionEventArgs eventArgs)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif
    ValueTask OnRenderToolbarActionAsync(object sender, GridToolbarActionEventArgs e)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif
    ValueTask OnRenderRowAsync(object sender, GridRowEventArgs e)
#if NET
    {
        return ValueTaskHelper.CompletedTask;
    }
#else
        ;
#endif
}