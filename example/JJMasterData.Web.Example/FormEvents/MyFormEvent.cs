using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Example.FormEvents;

[FormEvent("Example")]
public class MyFormEvent : IFormEvent
{
    public void OnInstanceCreated(JJFormView sender)
    {
        sender.FormElement.SubTitle = "You can edit your form at runtime using FormEvents";
    }
}