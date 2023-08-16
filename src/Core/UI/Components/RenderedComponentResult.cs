#nullable enable
namespace JJMasterData.Core.UI.Components;

public class RenderedComponentResult : ComponentResult
{
    public RenderedComponentResult(string content) : base(content, ContentType.RenderedComponent)
    {
    }
}