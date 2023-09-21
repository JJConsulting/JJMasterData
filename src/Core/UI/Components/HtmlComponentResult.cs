using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public abstract class HtmlComponentResult : ComponentResult
{
  
    internal HtmlBuilder HtmlBuilder { get; }
    public override string Content => HtmlBuilder.ToString(true);
    public HtmlComponentResult(HtmlBuilder htmlBuilder) 
    {
        HtmlBuilder = htmlBuilder;
    }
    
}