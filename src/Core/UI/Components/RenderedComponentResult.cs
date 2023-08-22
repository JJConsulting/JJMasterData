using Azure.Core;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public class RenderedComponentResult : ComponentResult
{
    internal HtmlBuilder HtmlBuilder { get; }
    public override string Content => HtmlBuilder.ToString();
    public RenderedComponentResult(HtmlBuilder htmlBuilder) 
    
    {
        HtmlBuilder = htmlBuilder;
    }
}