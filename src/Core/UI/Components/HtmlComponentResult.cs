#nullable enable
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public class HtmlComponentResult : ComponentResult
{
    public HtmlComponentResult(string content) : base(content, ContentType.HtmlData)
    {
    }
    public static ComponentResult FromHtmlBuilder(HtmlBuilder htmlBuilder)
    {
        var html = htmlBuilder.ToString();
        return new HtmlComponentResult(html);
    }
}