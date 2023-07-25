using System.Collections.Generic;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class JJToolbar : JJBaseView
{
    public List<HtmlBuilder> Items { get; set; }

    public JJToolbar()
    {
        Items = new List<HtmlBuilder>();
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

        int totElement = Items.Count;
        for (int i = 0; i < totElement; i++)
        {
            var element = Items[i];
            if (element == null)
                continue;

            if (i != 0)
                element.WithAttribute("style", "margin-left: 3px;");

            div.AppendElement(element);
        }

        return div;
    }



}
