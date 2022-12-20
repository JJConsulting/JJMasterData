using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public class JJSpinner : JJBaseView
{
    public JJSpinner()
    {
        CssClass += "spinner-grow spinner-grow-sm text-info";

        if (BootstrapHelper.Version == 3)
            CssClass += " jj-blink";
    }

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Span)
            .WithNameAndId(Name)
            .WithCssClass(CssClass)
            .WithAttributes(Attributes)
            .WithAttribute("role", "status")
            .AppendElementIf(BootstrapHelper.Version == 3,()=> new JJIcon(IconType.Circle).RenderHtml())
            .AppendElementIf(BootstrapHelper.Version != 3, HtmlTag.Span, s =>
            {
                s.WithCssClass("visually-hidden");
                s.AppendText(Translate.Key("Background Process Loading..."));

            });

        return html;
    }
}