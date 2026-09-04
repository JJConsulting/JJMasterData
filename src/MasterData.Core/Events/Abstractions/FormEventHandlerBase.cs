
using System.Threading.Tasks;
using JJMasterData.Core.Events.Args;

namespace JJMasterData.Core.Events.Abstractions;

public abstract class FormEventHandlerBase : IFormEventHandler
{
    public virtual ValueTask OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnAfterInsertAsync(object sender, FormAfterActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnAfterUpdateAsync(object sender, FormAfterActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnAfterDeleteAsync(object sender, FormAfterActionEventArgs args) => ValueTask.CompletedTask;
    public virtual ValueTask OnFormElementLoadAsync(object sender, FormElementLoadEventArgs args) => ValueTask.CompletedTask;
}
