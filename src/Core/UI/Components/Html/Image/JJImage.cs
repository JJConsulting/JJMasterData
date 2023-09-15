#nullable enable

using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJImage : HtmlComponent
{
    public string Src { get; set; }
    public string? Title { get; set; }

    internal JJImage(string src)
    {
        Src = src;
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
