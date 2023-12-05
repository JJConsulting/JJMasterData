#nullable enable
#if NET
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
#endif
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public class JsonComponentResult(object objectResult) : ComponentResult
#if NET
    ,IActionResult
#endif
{
    private object ObjectResult { get; } = objectResult;
    public override string Content => JsonConvert.SerializeObject(ObjectResult);

#if NET
    public Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        return new ContentResult
        {
            Content = Content,
            StatusCode = StatusCode,
            ContentType =  "application/json"
        }.ExecuteResultAsync(context);
    }
#endif
}