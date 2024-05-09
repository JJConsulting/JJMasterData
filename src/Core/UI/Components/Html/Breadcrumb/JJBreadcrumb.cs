using System.Collections.Generic;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJBreadcrumb : HtmlComponent
{
    public List<BreadcrumbItem> Items { get; set; }

    public JJBreadcrumb()
    {
        Items = new List<BreadcrumbItem>();
        CssClass = "border-bottom mb-3";
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .Append(HtmlTag.Nav, nav =>
            {
                nav.WithCssClass("mb-2 pb-2")
                    .WithAttribute("aria-label", "breadcrumb")
                    .Append(GetHtmlOlItems());
            });

        return html;
    }

    private HtmlBuilder GetHtmlOlItems()
    {
        var ol = new HtmlBuilder(HtmlTag.Ol);
        ol.WithCssClass("breadcrumb mb-0");

        int totItems = Items.Count;
        for (var index = 0; index < totItems; index++)
        {
            var item = Items[index];
            var isLast = index == totItems - 1;
            ol.Append(GetHtmlItem(item, isLast));
        }

        return ol;
    }

    private HtmlBuilder GetHtmlItem(BreadcrumbItem item, bool isLast)
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