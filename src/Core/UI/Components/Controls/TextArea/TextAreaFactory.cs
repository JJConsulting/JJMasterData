using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;

namespace JJMasterData.Core.Web.Factories;

internal class TextAreaFactory : IControlFactory<JJTextArea>
{
    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextAreaFactory(IHttpContext httpContext, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
    }
    public JJTextArea Create()
    {
        return new JJTextArea(HttpContext, StringLocalizer);
    }

    public JJTextArea Create(FormElement formElement,FormElementField field, FormStateData formStateData, string parentName, object value)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = Create();
        text.SetAttr(field.Attributes);
        text.ToolTip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Name = field.Name;

        return text;
    }
}