using JJMasterData.Commons.Dao;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Web.Example.FormEvents.CSharp;

[FormEvent("Example")]
public class MyFormEvent : IFormEvent
{
    public IEntityRepository Repository { get; }
    
    //You can also use dependency injection.
    public MyFormEvent(IEntityRepository repository)
    {
        Repository = repository;
    }
    
    public void OnMetadataLoad(object sender, MetadataLoadEventArgs args)
    {
        args.Metadata.Form.SubTitle = "You can edit your metadata at runtime using FormEvents";
    }
}