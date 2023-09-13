using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class CheckBoxFactory : IControlFactory<JJCheckBox>
{
    private IHttpRequest HttpRequest { get; }


    public CheckBoxFactory(IHttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public JJCheckBox Create() => new(HttpRequest);
    public JJCheckBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var checkBox = Create();
        checkBox.Name = field.Name;
        checkBox.IsChecked = StringManager.ParseBool(context.Value);
        checkBox.Tooltip = field.HelpDescription;
        return checkBox;
    }
}