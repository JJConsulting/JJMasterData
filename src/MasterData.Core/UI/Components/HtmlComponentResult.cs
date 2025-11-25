using JJConsulting.Html;


namespace JJMasterData.Core.UI.Components;

public abstract class HtmlComponentResult(HtmlBuilder htmlBuilder) : ComponentResult
{
    internal HtmlBuilder HtmlBuilder { get; } = htmlBuilder;
    public override string Content => HtmlBuilder.ToString(true);
}