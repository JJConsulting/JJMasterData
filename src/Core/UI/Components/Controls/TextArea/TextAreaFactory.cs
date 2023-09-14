using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Factories;

internal class TextAreaFactory : IControlFactory<JJTextArea>
{
    private IFormValues FormValues { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextAreaFactory(IFormValues formValues, IStringLocalizer<JJMasterDataResources> stringLocalizer)
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