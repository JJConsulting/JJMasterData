#nullable enable
namespace JJMasterData.Core.UI.Components;

public class RedirectComponentResult : ComponentResult
{
    public override string Content { get; }

    public RedirectComponentResult(string url)
    {
        Content = url;
    }
}