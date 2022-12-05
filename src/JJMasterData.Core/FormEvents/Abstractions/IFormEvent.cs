using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.FormEvents.Abstractions;

public interface IFormEvent
{
#if NETFRAMEWORK || NETSTANDARD
    public void OnBeforeInsert(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeDelete(object sender, FormBeforeActionEventArgs args);
    public void OnBeforeImport(object sender, FormBeforeActionEventArgs args);
    public void OnAfterInsert(object sender, FormAfterActionEventArgs args);
    public void OnAfterUpdate(object sender, FormAfterActionEventArgs args);
    public void OnAfterDelete(object sender, FormAfterActionEventArgs args);
    public void OnMetadataLoad(object sender, MetadataLoadEventArgs args);
#else
    public void OnBeforeInsert(object sender, FormBeforeActionEventArgs args){}
    public void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args){}
    public void OnBeforeDelete(object sender, FormBeforeActionEventArgs args){}
    public void OnBeforeImport(object sender, FormBeforeActionEventArgs args){}
    public void OnAfterInsert(object sender, FormAfterActionEventArgs args){}
    public void OnAfterUpdate(object sender, FormAfterActionEventArgs args){}
    public void OnAfterDelete(object sender, FormAfterActionEventArgs args){}
    public void OnMetadataLoad(object sender, MetadataLoadEventArgs args) {}
#endif
}