using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.FormEvents.Abstractions;

#if NETFRAMEWORK || NETSTANDARD
public interface IFormEvent
{
    public void OnBeforeInsert(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeDelete(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeImport(object sender, FormBeforeActionEventArgs args);
    public void OnAfterInsert(object sender, FormAfterActionEventArgs args);
    public void OnAfterUpdate(object sender, FormAfterActionEventArgs args);
    public void OnAfterDelete(object sender, FormAfterActionEventArgs args);
    public void OnInstanceCreated(JJFormView sender);
}
#else
public interface IFormEvent
{
    public virtual void OnBeforeInsert(object sender, FormBeforeActionEventArgs args){}
    public virtual void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args){}
    public virtual void OnBeforeDelete(object sender, FormBeforeActionEventArgs args){}
    public virtual void OnBeforeImport(object sender, FormBeforeActionEventArgs args){}
    public virtual void OnAfterInsert(object sender, FormAfterActionEventArgs args){}
    public virtual void OnAfterUpdate(object sender, FormAfterActionEventArgs args){}
    public virtual void OnAfterDelete(object sender, FormAfterActionEventArgs args){}
    public virtual void OnInstanceCreated(JJFormView sender){}
}
#endif
