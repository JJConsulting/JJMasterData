#nullable enable
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJAlert : HtmlComponent
{
    public BootstrapColor Color { get; set; }
    public IconType? Icon { get; set; }
    
    [LocalizationRequired]
    public string? Title { get; set; }
    
    public List<string> Messages { get; } = [];
    public HtmlBuilder? InnerHtml { get; set; }
    
    public bool ShowCloseButton { get; set; }
    
    /// <remarks>
    /// Default = true
    /// </remarks>
    public bool ShowIcon { get; set; } = true;

    internal JJAlert()
    {
    }
    
    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass("mt-3")
            .WithCssClass("alert")
            .WithCssClassIf(BootstrapHelper.Version > 3, "alert-dismissible")
            .WithCssClass(GetClassType())
            .WithAttribute("role", "alert");

        if (ShowCloseButton)
            html.Append(GetCloseButton("alert"));

        if (ShowIcon && Icon is not null)
        {
            var icon = new JJIcon(Icon.Value);
            icon.CssClass += $"{BootstrapHelper.MarginRight}-{1}";
            html.AppendComponent(icon);
        }
     

        if (Title is not null && !string.IsNullOrEmpty(Title))
            html.Append(HtmlTag.B, b => b.AppendText(Title));

        if (InnerHtml is not null || Messages.Any())
            html.Append(HtmlTag.Br);
        
        if (InnerHtml is not null)
            html.Append(InnerHtml);
        
        foreach (var message in Messages)
        {
            html.AppendText(message);
            html.Append(HtmlTag.Br);
        }

        return html;
    }

    private string GetClassType()
    {
        if (Color == BootstrapColor.Default)
            return BootstrapHelper.Version == 3 ? "well" : "alert-outline-primary";

        return $"alert-{Color.ToString().ToLower()}";
    }

    internal static HtmlBuilder GetCloseButton(string dimissValue)
    {
        var btn = new HtmlBuilder(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute("aria-label", "Close")
            .WithDataAttribute("dismiss", dimissValue)
            .WithCssClass(BootstrapHelper.Close)
            .AppendIf(BootstrapHelper.Version == 3, HtmlTag.Span, span =>
            {
                span.WithAttribute("aria-hidden", "true");
                span.AppendText("&times;");
            });

        return btn;
    }

}