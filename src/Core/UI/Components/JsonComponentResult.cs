#nullable enable
#if NET
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
#endif
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public class JsonComponentResult : ComponentResult
#if NET
    ,IActionResult
#endif
{
    private object ObjectResult { get; }
    public override string Content => JsonConvert.SerializeObject(ObjectResult);
    
    public JsonComponentResult(object objectResult)
    {
        ObjectResult = objectResult;
    }
#if NET
    public async Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        await new ContentResult
        {
            Content = Content,
            StatusCode = StatusCode,
            ContentType =  "application/json"
        }.ExecuteResultAsync(context);
    }
#endif
}