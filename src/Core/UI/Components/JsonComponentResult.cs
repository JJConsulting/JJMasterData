#nullable enable
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public class JsonComponentResult : ComponentResult
{
    public JsonComponentResult(string content) : base(content, ContentType.JsonData)
    {
    }

    public static JsonComponentResult FromObject(object @object)
    {
        return new JsonComponentResult(JsonConvert.SerializeObject(@object));
    }
}