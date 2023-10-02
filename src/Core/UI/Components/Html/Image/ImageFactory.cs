using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class ImageFactory
{
    private readonly IHttpContext _httpContext;

    public ImageFactory(IHttpContext httpContext)
    {
        _httpContext = httpContext;
    }
    
    public JJImage Create(string src)
    {
        return new JJImage(src);
    }

    public JJImage CreateMasterDataLogo()
    {
        var appPath = _httpContext.Request.ApplicationPath;
        var baseUrl = string.IsNullOrEmpty(appPath) ? "/" : appPath;
        var logoSrc = $"{baseUrl}_content/JJMasterData.Web/images/JJMasterData.png";
        var image = Create(logoSrc);
        image.SetAttr("style","width:8%;height:8%;");
        image.Title = "JJMasterData";
        return image;
    }
}