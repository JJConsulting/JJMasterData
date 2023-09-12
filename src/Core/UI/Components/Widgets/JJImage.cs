#nullable enable

using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJImage : HtmlComponent
{
    public string Src { get; set; }
    public string? Title { get; set; }

    public JJImage(string src)
    {
        Src = src;
    }

    public static JJImage GetMasterDataLogo(IHttpContext httpContext)
    {
        var appPath = httpContext.Request.ApplicationPath;
        var baseUrl = string.IsNullOrEmpty(appPath) ? "/" : appPath;
        var logoSrc = $"{baseUrl}_content/JJMasterData.Web/images/JJMasterData.png";
        var image = new JJImage(logoSrc);
        image.SetAttr("style","width:8%;height:8%;");
        image.Title = "JJMasterData";
        return image;
    }
    
    internal override HtmlBuilder BuildHtml()
    {
        var element = new HtmlBuilder(HtmlTag.Img)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithAttribute("src", Src)
            .WithAttributeIfNotEmpty("alt", Title)
            .WithCssClass(CssClass);
            
        return element;
    }
}
