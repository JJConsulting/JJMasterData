#nullable enable

using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// A ComponentBase with asynchronous programming support.
/// </summary>
public abstract class AsyncComponent : ComponentBase
{
#if NET48
    [Obsolete("This method uses Response.End and don't truly return a HTML everytime, please use GetResultAsync.")]
    public string? GetHtml()
    {
        var result = GetResultAsync().GetAwaiter().GetResult();
        
        if (result is RenderedComponentResult)
            return result;
        
        var httpContext = StaticServiceLocator.Provider.GetScopedDependentService<IHttpContext>();
        
        httpContext.Response.SendResponse(result.Content);

        return null;
    }
#endif
    
    public async Task<ComponentResult> GetResultAsync()
    {
        if (Visible)
            return await BuildResultAsync();
    
        return ComponentResult.Empty;
    }
    
    protected abstract Task<ComponentResult> BuildResultAsync();
}