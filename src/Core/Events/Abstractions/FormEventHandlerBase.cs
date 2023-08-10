using System.Threading.Tasks;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.FormEvents.Abstractions;

public abstract class FormEventHandlerBase : IFormEventHandler
{
    public abstract string ElementName { get; }
    public virtual void OnAfterDelete(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnAfterImport(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnAfterInsert(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnAfterUpdate(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnBeforeDelete(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnBeforeImport(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnBeforeInsert(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnFormElementLoad(object sender, FormElementLoadEventArgs args) {}
    public virtual Task OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnAfterInsertAsync(object sender, FormAfterActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnAfterUpdateAsync(object sender, FormAfterActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnAfterDeleteAsync(object sender, FormAfterActionEventArgs args) => Task.CompletedTask;
    public virtual Task OnFormElementLoadAsync(object sender, FormElementLoadEventArgs args) => Task.CompletedTask;
}