using JetBrains.Annotations;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public class HtmlComponentResult : ComponentResult
{
    public HtmlComponentResult(string content) : base(content, ContentType.HtmlData)
    {
    }

    public static ComponentResult FromHtmlBuilder(HtmlBuilder htmlBuilder)
    {
        return new HtmlComponentResult(htmlBuilder.ToString());
    }
}