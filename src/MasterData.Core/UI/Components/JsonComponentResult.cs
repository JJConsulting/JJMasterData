#nullable enable
using System.Text.Json;
using JJMasterData.Commons.Serialization;
#if NET
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
#endif


namespace JJMasterData.Core.UI.Components;

public sealed class JsonComponentResult(object objectResult) : ComponentResult
#if NET
    ,IActionResult
#endif
{
    private object ObjectResult { get; } = objectResult;
    public override string Content => JsonSerializer.Serialize(ObjectResult, MasterDataJsonSerializerOptions.Default);

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