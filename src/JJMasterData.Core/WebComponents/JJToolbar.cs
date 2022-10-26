using JJMasterData.Core.Html;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents;

internal class JJToolbar : JJBaseView
{
    public List<HtmlElement> ListElement { get; set; }

    public JJToolbar()
    {
        ListElement = new List<HtmlElement>();
    }

    internal override HtmlElement RenderHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div)
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

    private HtmlElement GetHtmlCol()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClass("col-sm-12");

        int totElement = ListElement.Count;
        for (int i = 0; i < totElement; i++)
        {
            var element = ListElement[i];
            if (element == null)
                continue;

            if (i != 0)
                element.WithCssClassIf(BootstrapHelper.Version > 3, $" {BootstrapHelper.MarginLeft}-1");

            div.AppendElement(element);
        }

        return div;
    }

}
