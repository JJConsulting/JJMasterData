#nullable enable
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public class JsonComponentResult : ComponentResult
{
    private object ObjectResult { get; }
    public override string Content => JsonConvert.SerializeObject(ObjectResult);
    
    public JsonComponentResult(object objectResult)
    {
        ObjectResult = objectResult;
    }
}