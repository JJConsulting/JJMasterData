using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.WebExample.FormEvents.CSharp;

[FormEvent("Example")]
public class MyFormEvent : BaseFormEvent
{
    public IEntityRepository Repository { get; }
    
    //You can also use dependency injection.
    public MyFormEvent(IEntityRepository repository)
    {
        Repository = repository;
    }
    
    public override void OnMetadataLoad(object sender, MetadataLoadEventArgs args)
    {
        args.Metadata.Form.SubTitle = "You can edit your metadata at runtime using FormEvents";
    }
}