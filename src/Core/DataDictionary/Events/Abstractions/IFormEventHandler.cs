using System.Threading.Tasks;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.FormEvents.Abstractions;

public interface IFormEventHandler : IEventHandler
{
    public void OnBeforeInsert(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeDelete(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeImport(object sender, FormBeforeActionEventArgs args);
    public void OnAfterInsert(object sender, FormAfterActionEventArgs args);
    public void OnAfterUpdate(object sender, FormAfterActionEventArgs args);
    public void OnAfterDelete(object sender, FormAfterActionEventArgs args);
    public void OnFormElementLoad(object sender, FormElementLoadEventArgs args);
    
    public Task OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args);
    public Task OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args);
    public Task OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args);
    public Task OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args);
    public Task OnAfterInsertAsync(object sender, FormAfterActionEventArgs args);
    public Task OnAfterUpdateAsync(object sender, FormAfterActionEventArgs args);
    public Task OnAfterDeleteAsync(object sender, FormAfterActionEventArgs args);
    public Task OnFormElementLoadAsync(object sender, FormElementLoadEventArgs args);
}