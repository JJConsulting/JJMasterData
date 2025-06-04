using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class TextAreaFactory(
    IFormValues formValues, 
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJTextArea>
{
    public JJTextArea Create()
    {
        return new JJTextArea(formValues,stringLocalizer);
    }

    public JJTextArea Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = Create();
        text.SetAttr(field.Attributes);
        text.Tooltip = field.HelpDescription;
        text.MaxLength = field.Size;
        var rows = field.GetAttr(FormElementField.RowsAttribute)?.ToString();
        text.Rows = int.Parse(string.IsNullOrEmpty(rows) ? "5" : rows);
        text.Text = context.Value != null ? context.Value.ToString() : "";
        text.Name = field.Name;

        return text;
    }
}