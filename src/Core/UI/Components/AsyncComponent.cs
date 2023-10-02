#nullable enable

using System;
using System.Threading.Tasks;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// A ComponentBase with asynchronous programming support.
/// </summary>
public abstract class AsyncComponent : ComponentBase
{
    public async Task<ComponentResult> GetResultAsync()
    {
        if (Visible)
            return await BuildResultAsync();

        return new EmptyComponentResult();
    }

    protected abstract Task<ComponentResult> BuildResultAsync();

    public static bool CanSendResult(ComponentResult componentResult)
    {
        return componentResult is ContentComponentResult or JsonComponentResult or RedirectComponentResult;
    }
    
#if NET48
    public static void SendResult(ComponentResult componentResult)
    {
        JJMasterData.Core.Http.SystemWeb.SystemWebHelper.SendResult(componentResult);
    }
    
    /// <summary>
    /// This method uses Response.End and don't truly return a HTML everytime, please read the obsolete message.
    /// It only exists to legacy compatibility with WebForms.
    /// </summary>
    /// <returns>The rendered HTML component or nothing (AJAX response)</returns>
    [Obsolete("Please use GetResultAsync with CanSendResult and SendResult. If CanSendResult return false, use the Content property at your front-end, else, use the SendResult method.")]
    public string? GetHtml()
    {
        var result = GetResultAsync().GetAwaiter().GetResult();

        if (result is RenderedComponentResult)
            return result;

        JJMasterData.Core.Http.SystemWeb.SystemWebHelper.SendResult(result);

        return null;
    }
#endif
}