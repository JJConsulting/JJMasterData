using System.Threading.Tasks;

#if NET
#endif

namespace JJMasterData.Web.Components;

public class RedirectComponentResult(string url) : ComponentResult
#if NET
,IActionResult
#endif  
{
    public override string Content { get; } = url;

#if NET 
    public Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
    {
        return new RedirectResult(Content).ExecuteResultAsync(context);
    }
#endif
}