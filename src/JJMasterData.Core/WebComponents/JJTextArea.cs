using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System;

namespace JJMasterData.Core.WebComponents;

public class JJTextArea : JJBaseControl
{

    public int Rows { get; set; }

    public JJTextArea()
    {
        Attributes.Add("class", "form-control");
        Rows = 5;
    }

    internal static JJTextArea GetInstance(FormElementField field, object value)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = new JJTextArea();
        text.SetAttr(field.Attributes);
        text.ToolTip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Name = field.Name;

        return text;
    }


    internal override HtmlElement GetHtmlElement()
    {

        var html = new HtmlElement(HtmlTag.TextArea)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(CssClass)
            .WithToolTip(ToolTip)
            .WithAttributeIf(!string.IsNullOrWhiteSpace(PlaceHolder),"placeholder",PlaceHolder)
            .WithAttribute("rows", Rows.ToString())
            .WithAttribute("cols", "20")
            .WithAttribute("strvalid", Translate.Key("Maximum limit of {0} characters!"))
            .WithAttribute("strchars", Translate.Key("({0} characters remaining)"))
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .AppendText(Text);

        return html;
    }
}
