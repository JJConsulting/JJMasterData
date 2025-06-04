using System.Threading.Tasks;
using JJMasterData.Core.Events.Args;

namespace JJMasterData.Core.Events.Abstractions;

public interface IFormEventHandler : IEventHandler
{
    public ValueTask OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnAfterInsertAsync(object sender, FormAfterActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnAfterUpdateAsync(object sender, FormAfterActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnAfterDeleteAsync(object sender, FormAfterActionEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
    public ValueTask OnFormElementLoadAsync(object sender, FormElementLoadEventArgs args)
#if NET
    {
        return ValueTask.CompletedTask;
    }
#else
    ;
#endif
}