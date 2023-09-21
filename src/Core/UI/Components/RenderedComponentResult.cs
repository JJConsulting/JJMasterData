#nullable enable

using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.UI.Components;

public class RenderedComponentResult : HtmlComponentResult
{
    public RenderedComponentResult(HtmlBuilder htmlBuilder) : base(htmlBuilder)
    {

    }
}