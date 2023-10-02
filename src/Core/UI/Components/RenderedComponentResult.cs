#nullable enable

using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class RenderedComponentResult : HtmlComponentResult
{
    public RenderedComponentResult(HtmlBuilder htmlBuilder) : base(htmlBuilder)
    {

    }
}