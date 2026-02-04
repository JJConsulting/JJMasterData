using JJConsulting.Html.Bootstrap.TagHelpers.Adapters;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Html;

namespace JJMasterData.Web.Extensions;

public static class ComponentResultExtensions
{
    extension(ComponentResult result)
    {
        public IHtmlContent HtmlContent
        {
            get
            {
                if (result is RenderedComponentResult html)
                    return new HtmlContentAdapter(html.HtmlBuilder);
                
                return new HtmlString(string.Empty);
            }
        }
    }
}