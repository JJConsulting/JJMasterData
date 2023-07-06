using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class TextAreaFactory
{
    private IHttpContext HttpContext { get; }

    public TextAreaFactory(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }
    internal JJTextArea CreateTextArea(FormElementField field, object value)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = new JJTextArea(HttpContext);
        text.SetAttr(field.Attributes);
        text.ToolTip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Name = field.Name;

        return text;
    }
}