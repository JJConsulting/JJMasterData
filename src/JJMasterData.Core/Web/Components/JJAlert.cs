using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJAlert : JJBaseView
{
    public PanelColor Color { get; set; }
    public IconType Icon { get; set; }
    public string Title { get; set; }
    public IList<string> Messages { get; set; }
    public bool ShowCloseButton { get; set; }

    /// <remarks>
    /// Default = true
    /// </remarks>
    public bool ShowIcon { get; set; } = true;

    public JJAlert()
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

        if (ShowIcon)
            html.AppendElement(new JJIcon(Icon));

        if (!string.IsNullOrEmpty(Title))
            html.AppendElement(HtmlTag.B)
                .AppendText($"&nbsp;&nbsp;{Translate.Key(Title)}");

        if (Messages != null)
        {
            foreach (string message in Messages)
            {
                html.AppendElement(HtmlTag.Br);
                html.AppendText(Translate.Key(message));
            }
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
            .WithAttribute("aria-label", Translate.Key("Close"))
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