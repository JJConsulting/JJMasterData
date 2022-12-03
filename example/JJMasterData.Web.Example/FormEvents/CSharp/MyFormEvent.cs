using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Web.Example.FormEvents.CSharp;

[FormEvent("Example")]
public class MyFormEvent : IFormEvent
{
    public void OnMetadataLoad(object sender, MetadataLoadEventArgs args)
    {
        args.Metadata.Form.SubTitle = "You can edit your metadata at runtime using FormEvents";
    }
}