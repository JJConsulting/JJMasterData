using System.Threading.Tasks;
using JJMasterData.Core.Events.Args;

namespace JJMasterData.Core.Events.Abstractions;

public interface IFormEventHandler : IEventHandler
{
    public Task OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnAfterInsertAsync(object sender, FormAfterActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnAfterUpdateAsync(object sender, FormAfterActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnAfterDeleteAsync(object sender, FormAfterActionEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
    public Task OnFormElementLoadAsync(object sender, FormElementLoadEventArgs args)
#if NET
    {
        return Task.CompletedTask;
    }
#else
    ;
#endif
}