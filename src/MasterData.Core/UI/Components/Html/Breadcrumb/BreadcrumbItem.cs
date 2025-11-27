#nullable enable

using JJConsulting.Html;
using JJConsulting.Html.Extensions;

namespace JJMasterData.Core.UI.Components;

public class BreadcrumbItem
{
    public string? Url { get; set; }
    
    public HtmlBuilder HtmlContent { get; init; }

    public string Content
    {
        init => HtmlContent.AppendText(value);
    }

    public BreadcrumbItem() : this(new HtmlBuilder())
    {
    }
    
    public BreadcrumbItem(string text) : this()
    {
        HtmlContent.AppendText(text);
    }
    
    public BreadcrumbItem(string text, string url) : this(text)
    {
        Url = url;
    }

    public BreadcrumbItem(HtmlBuilder htmlContent)
    {
        HtmlContent = htmlContent;
    }
    
    public BreadcrumbItem(HtmlBuilder htmlContent, string url)
    {
        HtmlContent = htmlContent;
        Url = url;
    }
}