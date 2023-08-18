#nullable enable

using System.Threading.Tasks;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Abstractions;

public abstract class AsyncControl : ControlBase
{
    
    protected AsyncControl(IHttpContext currentContext) : base(currentContext)
    {
    }
    
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
        
        Http.SystemWebHelper.HandleResult(result);

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