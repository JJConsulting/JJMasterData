using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.WebExample.FormEvents.CSharp;

public class MyFormEventHandler : FormEventHandlerBase
{
    public IEntityRepository Repository { get; }
    
    //You can also use dependency injection.
    public MyFormEventHandler(IEntityRepository repository)
    {
        Repository = repository;
    }

    public override string ElementName => "Example";

    public override void OnFormElementLoad(object sender, FormElementLoadEventArgs args)
    {
        args.FormElement.SubTitle = "You can edit your metadata at runtime using FormEvents";
    }
}