using Microsoft.AspNetCore.Html;

namespace JJMasterData.Web.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// Converts the current string to a renderable HTML string.
    /// </summary>
    /// <param name="string">String to be rendered.</param>
    /// <returns>The HTML renderable string.</returns>
    public static HtmlString ToHtmlString(this string @string)
    {
        return new HtmlString(@string);
    }
}