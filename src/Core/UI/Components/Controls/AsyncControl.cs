#nullable enable

using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Abstractions;

public abstract class AsyncControl : ControlBase
{
    
    protected AsyncControl(IHttpContext currentContext) : base(currentContext)
    {
    }
    
#if NET48
    [Obsolete("This method uses Response.End, please use GetResultAsync.")]
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