using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJAlert : JJBaseView
{
    public PanelColor Color { get; set; }
    public IconType? Icon { get; set; }
    public string Title { get; set; }
    public IList<string> Messages { get; set; }
    public bool ShowCloseButton { get; set; }

    /// <remarks>
    /// Default = true
    /// </remarks>
    public bool ShowIcon { get; set; } = true;

    internal JJAlert()
    {
        Messages = new List<string>();
    }

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithCssClass("alert")
            .WithCssClassIf(BootstrapHelper.Version > 3, "alert-dismissible")
            .WithCssClass(GetClassType())
            .WithAttribute("role", "alert");

        if (ShowCloseButton)
            html.AppendElement(GetCloseButton("alert"));

        if (ShowIcon && Icon is not null)
            html.AppendElement(new JJIcon(Icon.Value));

        if (!string.IsNullOrEmpty(Title))
            html.AppendElement(HtmlTag.B, b => b.AppendText($"&nbsp;&nbsp;{Title}"));

        if (Messages == null) 
            return html;

        for (var index = 0; index < Messages.Count; index++)
        {
            var message = Messages[index];
            html.AppendText(message);
            
            if(index > 0) 
                html.AppendElement(HtmlTag.Br);
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
            .AppendElementIf(BootstrapHelper.Version == 3, HtmlTag.Span, span =>
            {
                span.WithAttribute("aria-hidden", "true");
                span.AppendText("&times;");
            });

        return btn;
    }

}