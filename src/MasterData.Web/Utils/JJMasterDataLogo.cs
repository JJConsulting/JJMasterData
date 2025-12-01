using JJConsulting.Html.Bootstrap.Components;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Web.Utils;

using JJConsulting.Html;

public class JJMasterDataLogo(IHttpContextAccessor httpContextAccessor)
{
    public HtmlBuilder GetHtmlBuilder()
    {
        var logoSrc = GetMasterDataLogoSrc();
        
        var image = new JJImage(logoSrc)
        {
            CssClass = "img-responsive md-logo",
            Title = "JJMasterData"
        };

        return image.GetHtmlBuilder();
    }
    
    private string GetMasterDataLogoSrc()
    {
        var appPath = httpContextAccessor.HttpContext?.Request.PathBase;
        var baseUrl = string.IsNullOrEmpty(appPath) ? "/" : appPath.ToString() ?? "/";

        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";
        
        var logoSrc = $"{baseUrl}_content/JJMasterData.Web/images/JJMasterData.png";
        return logoSrc;
    }
}