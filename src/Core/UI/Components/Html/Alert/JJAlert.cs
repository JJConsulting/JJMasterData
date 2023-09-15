using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJAlert : HtmlComponent
{
    public PanelColor Color { get; set; }
    public IconType? Icon { get; set; }
    public string Title { get; set; }
    public IList<string> Messages { get; set; } = new List<string>();
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
     

        if (!string.IsNullOrEmpty(Title))
            html.Append(HtmlTag.B, b => b.AppendText(Title));

        if (Messages == null) 
            return html;

        for (var index = 0; index < Messages.Count; index++)
        {
            if(index > 0 || !string.IsNullOrEmpty(Title)) 
                html.Append(HtmlTag.Br);
            
            var message = Messages[index];
            html.AppendText(message);
        }

        return html;
    }

    private string GetClassType()
    {
        if (Color == PanelColor.Default)
            return BootstrapHelper.Version == 3 ? "well" : "alert-secondary";

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