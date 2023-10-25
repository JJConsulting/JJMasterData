using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class CheckBoxFactory : IControlFactory<JJCheckBox>
{
    private IFormValues FormValues { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }


    public CheckBoxFactory(IFormValues formValues, IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        FormValues = formValues;
        StringLocalizer = stringLocalizer;
    }

    public JJCheckBox Create() => new(FormValues,StringLocalizer);
    public JJCheckBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var checkBox = Create();
        checkBox.Name = field.Name;
        checkBox.IsChecked = StringManager.ParseBool(context.Value);
        checkBox.Tooltip = field.HelpDescription;
        return checkBox;
    }
}