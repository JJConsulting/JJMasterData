using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJLinkButtonGroup(IStringLocalizer<MasterDataResources> stringLocalizer) : HtmlComponent
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private List<JJLinkButton> _actions;

    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions
    {
        get => _actions ??= [];
        set => _actions = value;
    }

    public bool ShowAsButton { get; set; }

    public string CaretText { get; set; }
    
    public string MoreActionsText { get; set; }

    internal override HtmlBuilder BuildHtml()
    {
        var parentElement = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClassIf(BootstrapHelper.Version is 3, "input-group-btn")
            .WithCssClass(CssClass);

        AddActionsAt(parentElement);

        if (BootstrapHelper.Version is 5 && !ShowAsButton)
            parentElement.WithToolTip(MoreActionsText ?? StringLocalizer["More"]);
        
        return parentElement;
    }

    internal void AddActionsAt(HtmlBuilder html)
    {
        var listAction = Actions.Where(x => !x.IsGroup && x.Visible).ToList();
        var listActionGroup = Actions.Where(x => x.IsGroup && x.Visible).ToList();

        if (listAction.Count == 0 && listActionGroup.Count == 0)
            return;

        foreach (var action in listAction)
        {
            action.ShowAsButton = ShowAsButton;
            html.AppendComponent(action);
        }

        if (listActionGroup.Count > 0)
        {
            html.Append(GetHtmlCaretButton());
            html.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu dropdown-menu-right dropdown-menu-end");
                AddGroupActions(ul, listActionGroup);
            });
        }
    }

    private static void AddGroupActions(HtmlBuilder ul, List<JJLinkButton> listAction)
    {
        foreach (var action in listAction)
        {
            action.ShowAsButton = false;

            if (action.DividerLine)
            {
                ul.Append(HtmlTag.Li, li =>
                {
                    li.WithAttribute("role", "separator").WithCssClass("divider dropdown-divider");
                });
            }

            ul.Append(HtmlTag.Li, li =>
            {
                action.CssClass += " dropdown-item";
                li.AppendComponent(action);
            });
        }
    }

    private HtmlBuilder GetHtmlCaretButton()
    {
        var html = new HtmlBuilder(HtmlTag.A)
            .WithAttribute("href", "#")
            .WithAttribute(BootstrapHelper.DataToggle, "dropdown")
            .WithAttribute("aria-haspopup", "true")
            .WithAttribute("aria-expanded", "false")
            .WithCssClass("dropdown-toggle")
            .WithCssClassIf(ShowAsButton, BootstrapHelper.BtnDefault)
            .AppendTextIf(!string.IsNullOrEmpty(CaretText), CaretText!)
            .AppendIf( BootstrapHelper.Version is 3,HtmlTag.Span, s =>
            {
                s.WithCssClass("caret")
                    .WithToolTip(MoreActionsText ?? StringLocalizer["More"]);
            });
            
        return html;
    }

}