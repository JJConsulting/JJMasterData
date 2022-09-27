using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System;

namespace JJMasterData.Core.WebComponents;

public class JJTextTel : JJBaseControl
{
    public new int MaxLength { get; private set; }


    internal static JJTextTel GetInstance(FormElementField f,
                                object value,
                                bool enable = true,
                                bool readOnly = false,
                                string name = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        JJTextTel text = new JJTextTel();
        text.SetAttr(f.Attributes);
        text.ToolTip = f.HelpDescription;
        text.Text = value != null ? value.ToString() : "";
        text.Enable = enable;
        text.ReadOnly = readOnly;
        text.Name = name == null ? f.Name : name;

        return text;
    }

    public JJTextTel()
    {
        MaxLength = 19;
    }

    internal override HtmlElement GetHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div)
            .WithCssClass("input-group")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.InputGroupAddon);
                div.AppendElement(new JJIcon(IconType.Phone).GetHtmlElement());
                div.AppendElement(HtmlTag.Span, s =>
                {
                    s.WithAttribute("title", "Brasil")
                     .AppendText("&nbsp;&nbsp;+55");
                });
            })
            .AppendElement(HtmlTag.Input, i =>
            {
                i.WithNameAndId(Name)
                 .WithToolTip(Translate.Key(ToolTip))
                 .WithAttributes(Attributes)
                 .WithCssClass(CssClass)
                 .WithCssClass("form-control")
                 .WithAttribute("type", "tel")
                 .WithAttribute("maxlength", MaxLength.ToString())
                 .WithDataAttribute("inputmask", "'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'")
                 .WithAttributeIf(!string.IsNullOrEmpty(Text), "value", Text)
                 .WithAttributeIf(ReadOnly, "readonly", "readonly")
                 .WithAttributeIf(!Enable, "disabled", "disabled");
            });

        return html;
    }

}
