using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.FormEvents.Abstractions;

public abstract class BaseFormEvent : IFormEvent
{
    public virtual void OnAfterDelete(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnAfterImport(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnAfterInsert(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnAfterUpdate(object sender, FormAfterActionEventArgs args) { }
    public virtual void OnBeforeDelete(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnBeforeImport(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnBeforeInsert(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args) { }
    public virtual void OnMetadataLoad(object sender, MetadataLoadEventArgs args) {}
}