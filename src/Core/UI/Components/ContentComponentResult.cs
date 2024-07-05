#nullable enable

using System.Threading.Tasks;
using JJMasterData.Core.UI.Html;
#if NET
using Microsoft.AspNetCore.Mvc;
#endif
namespace JJMasterData.Core.UI.Components;

public sealed class ContentComponentResult(HtmlBuilder htmlBuilder) : HtmlComponentResult(htmlBuilder)
#if NET
    ,IActionResult
#endif
{
#if NET 
    public Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        return new ContentResult
        {
            Content = Content,
            StatusCode = StatusCode,
            ContentType =  "text/plain"
        }.ExecuteResultAsync(context);
    }
    #endif
}