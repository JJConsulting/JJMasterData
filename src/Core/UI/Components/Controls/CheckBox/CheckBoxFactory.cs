using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class CheckBoxFactory : IControlFactory<JJCheckBox>
{
    private IFormValues FormValues { get; }


    public CheckBoxFactory(IFormValues formValues)
    {
        FormValues = formValues;
    }

    public JJCheckBox Create() => new(FormValues);
    public JJCheckBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var checkBox = Create();
        checkBox.Name = field.Name;
        checkBox.IsChecked = StringManager.ParseBool(context.Value);
        checkBox.Tooltip = field.HelpDescription;
        return checkBox;
    }
}