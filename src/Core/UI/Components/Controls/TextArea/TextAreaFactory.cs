using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class TextAreaFactory : IControlFactory<JJTextArea>
{
    private IFormValues FormValues { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    public TextAreaFactory(IFormValues formValues, IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        FormValues = formValues;

        StringLocalizer = stringLocalizer;
    }
    public JJTextArea Create()
    {
        return new JJTextArea(FormValues,StringLocalizer);
    }

    public JJTextArea Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = Create();
        text.SetAttr(field.Attributes);
        text.Tooltip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = context.Value != null ? context.Value.ToString() : "";
        text.Name = field.Name;

        return text;
    }
}