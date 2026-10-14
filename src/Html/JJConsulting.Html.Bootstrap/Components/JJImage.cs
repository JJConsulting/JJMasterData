using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJImage(string src) : HtmlComponent
{
    public string Src { get; set; } = src;
    public string? Title { get; set; }

    protected override HtmlBuilder BuildHtml()
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