#nullable enable
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public class JsonComponentResult : ComponentResult
{
    private object Object { get; }
    public override string Content => JsonConvert.SerializeObject(Object);

    public JsonComponentResult(object @object) : base(ContentType.JsonData)
    {
        Object = @object;
    }
}