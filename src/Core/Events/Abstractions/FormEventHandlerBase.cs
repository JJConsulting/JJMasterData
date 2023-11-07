#if NETFRAMEWORK
using System.Threading.Tasks;
using JJMasterData.Core.Events.Args;

namespace JJMasterData.Core.Events.Abstractions;

public abstract class FormEventHandlerBase : IFormEventHandler
{
    public abstract string ElementName { get; }
    public virtual Task OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnAfterInsertAsync(object sender, FormAfterActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnAfterUpdateAsync(object sender, FormAfterActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnAfterDeleteAsync(object sender, FormAfterActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnFormElementLoadAsync(object sender, FormElementLoadEventArgs args) => Task.CompletedTask;
}
#endif