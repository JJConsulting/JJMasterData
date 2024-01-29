using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
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
        picker.Name = context.ParentComponentName;
        picker.Visible = true;
        if (context.Value is not null)
        {
            picker.SelectedIcon = IconHelper.GetIconTypeFromField(field, context.Value);
        }

        return picker;
    }
}