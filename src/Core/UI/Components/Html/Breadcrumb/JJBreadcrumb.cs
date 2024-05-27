using System.Collections.Generic;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJBreadcrumb : HtmlComponent
{
    public List<BreadcrumbItem> Items { get; }

    public JJBreadcrumb()
    {
        Items = [];
        CssClass = "border-bottom pb-2 mb-2";
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new Div()
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .Append(HtmlTag.Nav, nav =>
            {
                    nav.WithAttribute("aria-label", "breadcrumb")
                    .Append(GetHtmlOlItems());
            });

        return html;
    }

    private HtmlBuilder GetHtmlOlItems()
    {
        var ol = new Ol();
        ol.WithCssClass("breadcrumb mb-0");

        var totalItems = Items.Count;
        for (var index = 0; index < totalItems; index++)
        {
            var item = Items[index];
            var isLast = index == totalItems - 1;
            ol.Append(GetHtmlItem(item, isLast));
        }

        return ol;
    }

    private static HtmlBuilder GetHtmlItem(BreadcrumbItem item, bool isLast)
    {
        var li = new Li();
        li.WithCssClass("breadcrumb-item");
        li.WithCssClassIf(isLast, "active");
        
        if (item.Url is null || isLast)
        {
            li.Append(item.HtmlContent);
        }
        else
        {
            li.Append(HtmlTag.A, a =>
            {
                a.WithAttribute("href", item.Url);
                a.Append(item.HtmlContent);
            });
        }

        return li;
    }
}