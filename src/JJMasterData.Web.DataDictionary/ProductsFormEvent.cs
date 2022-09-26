using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.DataDictionary
{
    public class ProductsFormEvent : BaseFormEvent
    {

        public override void OnInstanceCreated(JJFormView sender)
        {
            sender.FormElement.Title = "My Custom title";
        }
    }
}
