#nullable enable
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public class RenderedComponentResult : ComponentResult
{
    public new required HtmlBuilder Content { get; init; }
    private RenderedComponentResult(string content) : base(content, ContentType.RenderedComponent)
    {
    }
    
    public static ComponentResult FromHtmlBuilder(HtmlBuilder htmlBuilder)
    {
        var result = new RenderedComponentResult(htmlBuilder.ToString())
        {
            Content = htmlBuilder
        };
        return result;
    }
}