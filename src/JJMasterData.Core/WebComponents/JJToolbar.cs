using JJMasterData.Core.Html;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents;

internal class JJToolbar : JJBaseView
{
    public List<HtmlBuilder> ElementList { get; set; }

    public JJToolbar()
    {
        ElementList = new List<HtmlBuilder>();
    }

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(BootstrapHelper.FormGroup)
            .WithCssClass(CssClass)
            .AppendElement(HtmlTag.Div, row =>
            {
                row.WithCssClass("row");
                row.AppendElement(GetHtmlCol());
            });

        return html;
    }

    private HtmlBuilder GetHtmlCol()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("col-sm-12");
        
        for (int i = 0; i < ElementList.Count; i++)
        {
            var element = ElementList[i];
            if (element == null)
                continue;

            if (i != 0)
                element.WithAttribute("style", "margin-left: 3px;");

            div.AppendElement(element);
        }

        return div;
    }

}
