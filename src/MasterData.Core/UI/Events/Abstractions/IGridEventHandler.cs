using System.Threading.Tasks;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events.Abstractions;

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
    
    public void OnRenderCell(object? sender, GridCellEventArgs eventArgs)
#if NET
    {
    }
#else
        ;
#endif

    public void OnRenderSelectedCell(object? sender, GridSelectedCellEventArgs eventArgs)
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
    public void OnRenderAction(object? sender, ActionEventArgs eventArgs)
#if NET
    {
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
    void OnRenderRow(object? sender, GridRowEventArgs e)
#if NET
    {
    }
#else
        ;
#endif
}
