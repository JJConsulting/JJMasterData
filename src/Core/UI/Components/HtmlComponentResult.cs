using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public abstract class HtmlComponentResult : ComponentResult
{
    internal HtmlBuilder HtmlBuilder { get; }
    public override string Content => HtmlBuilder.ToString(true);
    protected HtmlComponentResult(HtmlBuilder htmlBuilder) 
    {
        HtmlBuilder = htmlBuilder;
    }
    
}