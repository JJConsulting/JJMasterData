using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Extensions;

internal static class HtmlHelperExtensions
{
    public static bool IsDebug(this IHtmlHelper htmlHelper)
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

}