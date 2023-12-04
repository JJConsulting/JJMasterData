#if NET48

using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Http.SystemWeb;

public static class SystemWebHelper
{
    public static bool CanSendResult(ComponentResult result)
    {
        return result is not EmptyComponentResult && result is not RenderedComponentResult;
    }

    public static void SendResult(ComponentResult result)
    {
        var currentContext = System.Web.HttpContext.Current;

        if (result is RedirectComponentResult redirectComponentResult)
            currentContext.Response.Redirect(redirectComponentResult.Content);
        else
        {
            if (result is JsonComponentResult)
            {
                currentContext.Response.ContentType = "application/json";
            }

            currentContext.Response.Clear();
            currentContext.Response.Write(result.Content!);
            currentContext.Response.End();
        }
    }
}
#endif