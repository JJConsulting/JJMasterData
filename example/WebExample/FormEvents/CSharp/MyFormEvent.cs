using JJMasterData.Commons.Data.Entity.Abstractions;
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
    
    public override void OnFormElementLoad(object sender, FormElementLoadEventArgs args)
    {
        args.FormElement.SubTitle = "You can edit your metadata at runtime using FormEvents";
    }
}