#nullable enable

using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public class HtmlComponentResult : ComponentResult
{
    public override string Content { get; }
    public HtmlComponentResult(string content)
    {
        Content = content;
    }
    
    public static ComponentResult FromHtmlBuilder(HtmlBuilder htmlBuilder)
    {
        var html = htmlBuilder.ToString();
        return new HtmlComponentResult(html);
    }
}