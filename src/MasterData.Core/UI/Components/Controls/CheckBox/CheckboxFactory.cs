#nullable disable warnings
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class CheckboxFactory(IHttpContextAccessor formValues, IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJCheckBox>
{
    public JJCheckBox Create() => new(formValues,stringLocalizer);
    public JJCheckBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var checkBox = Create();
        checkBox.Name = field.Name;
        checkBox.IsChecked = StringManager.ParseBool(context.Value);
        checkBox.Tooltip = field.HelpDescription;
        return checkBox;
    }
}