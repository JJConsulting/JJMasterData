using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Events.Args;

public class GridInsertActionEventArgs(HtmlBuilder htmlBuilder)
{
    public HtmlBuilder HtmlBuilder { get; } = htmlBuilder;
}