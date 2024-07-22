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


        var hasTitle = !string.IsNullOrEmpty(Title);
        
        if (hasTitle)
        {
            html.Append(HtmlTag.H5, h5 =>
            {
                h5.WithCssClass("alert-heading");
                if (ShowIcon)
                {
                    AppendAlertIcon(h5);
                }
          
                h5.AppendText(Title);
            });
        }

        if (InnerHtml is not null)
        {
            if (!hasTitle && ShowIcon)
                AppendAlertIcon(html);
            html.Append(InnerHtml);
        }
        
        html.AppendDiv(div =>
        {
            div.WithCssClass("alert-content");
            
            if (!hasTitle && ShowIcon)
                AppendAlertIcon(div);
            
            if (Messages.Count > 1)
            {
                div.Append(HtmlTag.Ul, ul =>
                {
                    ul.WithCssClass("m-0");
                    foreach (var message in Messages)
                    {
                        ul.Append(HtmlTag.Li, li =>
                        {
                            li.AppendText(message);
                        });
                    }
                });
            }
            else if(Messages.Count == 1)
            {
                div.AppendText(Messages[0]);
            }
        });
        
        return html;
    }

    private void AppendAlertIcon(HtmlBuilder div)
    {
        if (Icon == null)
            return;
        
        var icon = new JJIcon(Icon.Value);
        icon.CssClass += $"{BootstrapHelper.MarginRight}-{1}";
        div.AppendComponent(icon);
    }

    private string GetClassType()
    {
        if (Color == BootstrapColor.Default)
            return BootstrapHelper.Version == 3 ? "well" : "alert-secondary";

        return $"alert-{Color.ToColorString()}";
    }

    internal static HtmlBuilder GetCloseButton(string dismissValue)
    {
        var btn = new HtmlBuilder(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute("aria-label", "Close")
            .WithDataAttribute("dismiss", dismissValue)
            .WithCssClass(BootstrapHelper.Close)
            .AppendIf(BootstrapHelper.Version == 3, HtmlTag.Span, span =>
            {
                span.WithAttribute("aria-hidden", "true");
                span.AppendText("&times;");
            });

        return btn;
    }

}