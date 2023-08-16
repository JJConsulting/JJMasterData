#nullable enable
namespace JJMasterData.Core.UI.Components;

public class JsonComponentResult : ComponentResult
{
    public JsonComponentResult(string content) : base(content, ContentType.JsonData)
    {
    }
}