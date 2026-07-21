using JJConsulting.Html;


namespace JJMasterData.Web.Events.Args;

public class GridRenderEventArgs
{
    public required HtmlBuilder HtmlBuilder { get; init; }
}