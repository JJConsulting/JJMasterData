using System.Collections.Generic;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.Html;


namespace JJMasterData.Core.UI.Components;

public sealed class JJToolbar : HtmlComponent
{
    public List<HtmlBuilder> Items { get; set; }
    internal JJToolbar()
    {
        Items = [];
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .Append(HtmlTag.Div,this, static (toolbar, row) =>
            {
                row.WithCssClass("row");
                row.Append(toolbar.GetHtmlCol());
            });

        return html;
    }

    private HtmlBuilder GetHtmlCol()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("col-sm-12");
        
        for (var i = 0; i < Items.Count; i++)
        {
            var htmlBuilder = Items[i];
            if (htmlBuilder == null)
                continue;

            if (i != 0)
                htmlBuilder.WithStyle("margin-right: 3px;");

            div.Append(htmlBuilder);
        }

        return div;
    }
}
