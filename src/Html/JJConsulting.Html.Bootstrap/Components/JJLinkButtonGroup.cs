using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Utils;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJLinkButtonGroup : HtmlComponent
{
    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions
    {
        get => field ??= [];
        set;
    }

    public bool ShowAsButton { get; set; }

    public HtmlBuilder? CaretHtml { get; set; }

    public string? MoreActionsText { get; set; }

    protected override HtmlBuilder BuildHtml()
    {
        var parentElement = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClassIf(BootstrapHelper.Version is 3, "input-group-btn")
            .WithCssClass(CssClass);

        AddActionsAt(parentElement);

        if (BootstrapHelper.Version is 5 && !ShowAsButton)
            parentElement.WithAttribute("title", MoreActionsText);

        return parentElement;
    }

    public void AddActionsAt(HtmlBuilder html)
    {
        var actionList = Actions.FindAll(x => x is { IsGroup: false, Visible: true });
        var actionListGroup = Actions.FindAll(x => x is { IsGroup: true, Visible: true });

        if (actionList.Count == 0 && actionListGroup.Count == 0)
            return;

        foreach (var action in actionList)
        {
            action.ShowAsButton = ShowAsButton;
            html.AppendComponent(action);
        }

        if (actionListGroup.Count > 0)
        {
            html.Append(GetHtmlCaretButton());
            html.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu dropdown-menu-right dropdown-menu-end");
                AddGroupActions(ul, actionListGroup);
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
                ul.AppendLi(li => { li.WithAttribute("role", "separator").WithCssClass("divider dropdown-divider"); });
            }

            ul.AppendLi(li =>
            {
                action.CssClass += " dropdown-item";
                li.AppendComponent(action);
            });
        }
    }

    private HtmlBuilder GetHtmlCaretButton()
    {
        var html = HtmlBuilder.A()
            .WithAttribute("href", "#")
            .WithAttribute(BootstrapHelper.DataToggle, "dropdown")
            .WithAttribute("aria-haspopup", "true")
            .WithAttribute("aria-expanded", "false")
            .WithCssClass("dropdown-toggle")
            .WithCssClassIf(ShowAsButton, BootstrapHelper.BtnDefault)
            .Append(CaretHtml)
            .AppendIf(BootstrapHelper.Version is 3, HtmlTag.Span, s =>
            {
                s.WithCssClass("caret")
                    .WithToolTip(MoreActionsText);
            });

        return html;
    }
}