using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJMasterDataLogo(IHttpContext httpContext)
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
        var appPath = httpContext.Request.ApplicationPath;
        var baseUrl = string.IsNullOrEmpty(appPath) ? "/" : appPath;

        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";
        
        var logoSrc = $"{baseUrl}_content/JJMasterData.Web/images/JJMasterData.png";
        return logoSrc;
    }
}