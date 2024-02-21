using System.ComponentModel;
using JetBrains.Annotations;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Represents a clickable button
/// </summary>

public class JJLinkButton : HtmlComponent
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;

    private JJSpinner _spinner;

    public JJSpinner Spinner
    {
        get =>
            _spinner ??= new JJSpinner
            {
                Visible = false
            };
        set => _spinner = value;
    }
    
    [Localizable(false)]
    public string Text { get; set; }

    public HtmlBuilder InnerHtml { get; } = new();
    
    [Localizable(false)]
    public string Tooltip { get; set; }

    /// <summary>
    /// Action will be fired clicking at any place at the row.
    /// </summary>
    public bool IsDefaultOption { get; set; }

    /// <summary>
    /// Show the action inside a group.
    /// Default = false
    /// </summary>
    public bool IsGroup { get; set; }

    /// <summary>
    /// Show a divider before this action
    /// Default = false
    /// </summary>
    /// <remarks>
    /// Only valid if IsGroup is true.
    /// </remarks>
    public bool DividerLine { get; set; }
    
    public bool Enabled { get; set; } = true;

    public IconType Icon
    {
        set => IconClass = value.GetCssClass();
    }

    /// <remarks>
    /// FontAwesome 2022 icon class.
    /// </remarks>
    public string IconClass { get; set; }
    
    /// <summary>
    /// Add btn class on render. 
    /// Default = false
    /// </summary>
    public bool ShowAsButton { get; set; }

    public LinkButtonType Type { get; set; } = LinkButtonType.Link;

    public string OnClientClick { get; set; }
    
    public string UrlAction { get; set; }

    public PanelColor Color { get; set; } = PanelColor.Default;
    
    public bool OpenInNewTab { get; set; }

    internal JJLinkButton(IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }
    
    internal override HtmlBuilder BuildHtml()
    {
        var icon = GetIcon();
        var html = new HtmlBuilder(Type is LinkButtonType.Submit or LinkButtonType.Button ? HtmlTag.Button : HtmlTag.A);

        if (Type == LinkButtonType.Submit)
        {
            html.WithAttribute("type", "submit");
            html.WithAttributeIfNotEmpty("formaction", UrlAction);
            html.WithAttributeIf(ShowAsButton, "role", "button");
        }
        else if (Type == LinkButtonType.Button)
        {
            html.WithAttribute("type", "button");
        }
        else
        {
            if (Enabled && !string.IsNullOrEmpty(UrlAction))
                html.WithAttribute("href", UrlAction);
            else
                html.WithAttribute("href", "javascript: void(0);");
            
            if(!ShowAsButton)
                html.WithCssClass($"link-{Color.ToString().ToLower()}");
        }


        html.WithAttributeIf(OpenInNewTab, "target", "_blank");
        html.WithNameAndId(Name);
        html.WithCssClass(GetCssClassWithCompatibility());
        html.WithAttributes(Attributes);
        html.WithToolTip(_stringLocalizer[Tooltip]);
        bool isOnClickEnabled = Enabled && !string.IsNullOrEmpty(OnClientClick) && UrlAction is null;
        html.WithAttributeIf(isOnClickEnabled, "onclick", OnClientClick);
        html.WithCssClassIf(!Enabled, "disabled");

        if (icon is not null)
        {
            html.AppendComponent(icon);
            html.WithCssClassIf(Enabled,"icon-link-hover");
        }
        
        switch (!string.IsNullOrEmpty(Text))
        {
            case true when ShowAsButton || Type is LinkButtonType.Button or LinkButtonType.Submit:
                html.Append(HtmlTag.Span, s =>
                {
                    s.AppendText("&nbsp;" +_stringLocalizer[Text]);
                });
                break;
            case true:
                html.AppendText(_stringLocalizer[Text]);
                break;
        }

        
        html.Append(InnerHtml);
        
        if (_spinner != null)
            html.AppendComponent(Spinner);
        
        return html;
    }

    private string GetCssClassWithCompatibility()
    {
        string cssClass = CssClass ?? string.Empty;

        if (BootstrapHelper.Version >= 4)
        {
            cssClass = cssClass.Replace("pull-left", BootstrapHelper.PullLeft);
            cssClass = cssClass.Replace("pull-right", BootstrapHelper.PullRight);
        }
        else
        {
            cssClass = cssClass.Replace("float-start", "pull-left");
            cssClass = cssClass.Replace("float-end", "pull-right");
        }

        if (ShowAsButton)
        {
            if (!cssClass.Contains("btn ") &&
                !cssClass.Contains(" btn") &&
                !cssClass.Equals("btn"))
            {
                cssClass += $" btn btn-{Color.ToString().ToLower()}"; 
            }
        }
        else if (!ShowAsButton && Type is LinkButtonType.Submit)
        {
            cssClass += " btn btn-link";
        }

        return cssClass;
    }

    [CanBeNull]
    private JJIcon GetIcon()
    {
        if (string.IsNullOrEmpty(IconClass)) 
            return null;
        
        var icon = new JJIcon(IconClass);
        if (!string.IsNullOrEmpty(IconClass) && IconClass.Contains("fa-") && IsGroup)
        {
            icon.CssClass = "fa-fw";
        }

        icon.CssClass += " bi";

        return icon;
    }

}