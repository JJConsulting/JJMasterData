using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJLinkButtonGroup : JJComponentBase
{
    private List<JJLinkButton> _actions;

    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions
    {
        get => _actions ??= new List<JJLinkButton>();
        set => _actions = value;
    }

    public bool ShowAsButton { get; set; }

    public string CaretText { get; set; }
    
    public string MoreActionsText { get; set; }
    
    internal override HtmlBuilder RenderHtml()
    {
        var inputGroup = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass(BootstrapHelper.InputGroupBtn)
            .WithCssClass(CssClass);

        AddActionsAt(inputGroup);

        return inputGroup;
    }

    internal void AddActionsAt(HtmlBuilder inputGroup)
    {
        var listAction = Actions.ToList().FindAll(x => !x.IsGroup && x.Visible);
        var listActionGroup = Actions.ToList().FindAll(x => x.IsGroup && x.Visible);

        if (listAction.Count == 0 && listActionGroup.Count == 0)
            return;

        foreach (var action in listAction)
        {
            action.ShowAsButton = ShowAsButton;
            inputGroup.AppendComponent(action);
        }

        if (listActionGroup.Count > 0)
        {
            inputGroup.Append(GetHtmlCaretButton());
            inputGroup.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu dropdown-menu-right");
                AddGroupActions(ul, listActionGroup);
            });
        }
    }

    private void AddGroupActions(HtmlBuilder ul, List<JJLinkButton> listAction)
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
                li.WithCssClass("dropdown-item").AppendComponent(action);
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
            .WithCssClassIf(ShowAsButton, BootstrapHelper.DefaultButton)
            .AppendTextIf(!string.IsNullOrEmpty(CaretText), CaretText)
            .AppendIf(BootstrapHelper.Version == 3, HtmlTag.Span, s =>
            {
                s.WithCssClass("caret")
                    .WithToolTip(MoreActionsText ?? "More Options");
            });
            
        return html;
    }

}