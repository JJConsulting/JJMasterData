#nullable enable

using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// A ComponentBase with asynchronous programming support.
/// </summary>
public abstract class AsyncComponent : ComponentBase
{
#if NET48
    /// <summary>
    /// This method uses Response.End and don't truly return a HTML everytime, please use GetResultAsync.
    /// It only exists to legacy compatibility with WebForms.
    /// </summary>
    /// <returns>The rendered HTML component or nothing (AJAX response)</returns>
    public string? GetHtml()
    {
        var result = GetResultAsync().GetAwaiter().GetResult();
        
        if (result is RenderedComponentResult)
            return result;
        
        JJMasterData.Core.Http.SystemWeb.SystemWebHelper.SendResult(result);

        return null;
    }
#endif
    
    public async Task<ComponentResult> GetResultAsync()
    {
        if (Visible)
            return await BuildResultAsync();
    
        return new EmptyComponentResult();
    }
    
    protected abstract Task<ComponentResult> BuildResultAsync();
}