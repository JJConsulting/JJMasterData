using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;


public class JJLabel : HtmlComponent
{
    public string Tooltip { get; set; }

    public string LabelFor
    {
        get => GetAttr("for");
        set => SetAttr("for", value);
    }

    public string Text { get; set; }
    public string RequiredText { get; set; }
    public bool IsRequired { get; set; }

    internal JJLabel()
    {

    }

    internal override HtmlBuilder BuildHtml()
    {
        var element = new HtmlBuilder(HtmlTag.Label)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(BootstrapHelper.Label)
            .WithCssClass(CssClass)
            .AppendText(Text)
            .AppendIf(IsRequired, HtmlTag.Span, s =>
            {
                s.WithCssClass("required-symbol");
                s.AppendText("*");
                s.WithToolTip(RequiredText);
            })
            .AppendIf(!string.IsNullOrEmpty(Tooltip), HtmlTag.Span, s =>
            {
                s.WithCssClass("fa fa-question-circle help-description");
                s.WithToolTip(Tooltip);
            });
          
        return element;
    }
}
