using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

public class JJTextGroup : JJTextBox
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

    /// <summary>
    /// Text info on left of component
    /// </summary>
    public InputAddons Addons { get; set; }


    public string GroupCssClass { get; set; }

    public static JJTextGroup GetInstance(FormElementField f, string name = null)
    {
        return WebControlTextFactory.CreateTextGroup(f, name);
    }

    internal override HtmlElement GetHtmlElement()
    {
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        if (!Enabled)
        {
            if (defaultAction != null)
            {
                ReadOnly = true;
                Enabled = true;
            }
        }

        var input = base.GetHtmlElement();
        bool hasAction = Actions.ToList().Exists(x => x.Visible);
        bool hasAddons = Addons != null;

        if (!hasAction && !hasAddons)
            return input;


        if (defaultAction is { Enabled: true })
        {
            input.WithCssClass("default-option");
            input.WithAttribute("onchange", defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlElement(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass);

        if (hasAddons)
            inputGroup.AppendElement(GetHtmlAddons());

        inputGroup.AppendElement(input);

        if (hasAction)
            AddActionsAt(inputGroup);

        return inputGroup;
    }

    private void AddActionsAt(HtmlElement inputGroup)
    {
        HtmlElement elementGroup;
        if (BootstrapHelper.Version >= 5)
        {
            elementGroup = inputGroup;
        }
        else
        {
            elementGroup = new HtmlElement(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.InputGroupBtn);

            inputGroup.AppendElement(elementGroup);
        }

        var btnGroup = new JJLinkButtonGroup();
        btnGroup.Actions = Actions;
        btnGroup.ShowAsButton = true;

        //Add element Actions
        btnGroup.AddActionsAt(elementGroup);
    }


    private HtmlElement GetHtmlAddons()
    {
        var html = new HtmlElement(HtmlTag.Span)
             .WithCssClass(BootstrapHelper.InputGroupAddon)
             .WithToolTip(Addons.ToolTip)
             .AppendElementIf(Addons.Icon != null, Addons.Icon?.GetHtmlElement())
             .AppendTextIf(!string.IsNullOrEmpty(Addons.Text), Addons.Text);

        return html;
    }


}
