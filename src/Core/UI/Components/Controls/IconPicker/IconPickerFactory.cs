using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class IconPickerFactory(IStringLocalizer<MasterDataResources> stringLocalizer, IMasterDataUrlHelper urlHelper, IControlFactory<JJComboBox> comboBoxFactory, IFormValues formValues) : IControlFactory<JJIconPicker>
{
    public JJIconPicker Create() => new(stringLocalizer,urlHelper,comboBoxFactory,formValues);


    public JJIconPicker Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var picker = Create();
        picker.Name = field.Name;
        picker.Visible = true;
        if (!string.IsNullOrEmpty(context.Value?.ToString()))
        {
            picker.SelectedIcon = IconHelper.GetIconTypeFromField(field, context.Value);
        }

        return picker;
    }
}