using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class ImageFactory(IHttpContext httpContext)
{
    public JJImage Create(string src)
    {
        return new JJImage(src);
    }

    public JJImage CreateMasterDataLogo()
    {
        var appPath = httpContext.Request.ApplicationPath;
        var baseUrl = string.IsNullOrEmpty(appPath) ? "/" : appPath;

        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";
        
        var logoSrc = $"{baseUrl}_content/JJMasterData.Web/images/JJMasterData.png";
        var image = Create(logoSrc);
        image.SetAttr("style","width:15%;height:15%;");
        image.Title = "JJMasterData";
        return image;
    }
    
}