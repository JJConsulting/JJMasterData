using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Factories;

public class TextAreaFactory
{
    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextAreaFactory(IHttpContext httpContext, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
    }
    internal JJTextArea CreateTextArea(FormElementField field, object value)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = new JJTextArea(HttpContext,StringLocalizer);
        text.SetAttr(field.Attributes);
        text.ToolTip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Name = field.Name;

        return text;
    }
}