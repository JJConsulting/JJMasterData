using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace JJMasterData.Web.Extensions;

public static class HtmlHelperExtensions
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