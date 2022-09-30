using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

public class JJTextGroup : JJBaseView
{

    private List<JJLinkButton> _actions;
    private JJTextBox _textBox;

    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions
    {
        get
        {
            if (_actions == null)
                _actions = new List<JJLinkButton>();

            return _actions;
        }
        set { _actions = value; }
    }

    /// <summary>
    /// Represent a input component
    /// </summary>
    public JJTextBox TextBox
    {
        get
        {
            if (_textBox == null)
                _textBox = new JJTextBox();

            return _textBox;
        }
        set { _textBox = value; }
    }

    /// <summary>
    /// Text info on left of component
    /// </summary>
    public InputAddons Addons { get; set; }


    public static JJTextGroup GetInstance(FormElementField f, string name = null)
    {
        return InputFactory.GetInstance(f, name);
    }

    internal override HtmlElement GetHtmlElement()
    {
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        if (!TextBox.Enable)
        {
            if (defaultAction != null)
            {
                TextBox.ReadOnly = true;
                TextBox.Enable = true;
            }
        }

        var input = TextBox.GetHtmlElement();
        bool hasAction = Actions.ToList().Exists(x => x.Visible);
        bool hasAddons = Addons != null;

        if (!hasAction && !hasAddons)
            return input;

       
        if (defaultAction != null && defaultAction.Enabled)
        {
            input.WithCssClass("default-option");
            input.WithAttribute("onchange", defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlElement(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithNameAndId(Name)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(CssClass);

        if (hasAddons)
            inputGroup.AppendElement(GetHtmlAddons());

        inputGroup.AppendElement(input);

        if (hasAction)
            AddActions(inputGroup);

        return inputGroup;
    }

    private void AddActions(HtmlElement inputGroup)
    {
        var listAction = Actions.ToList().FindAll(x => !x.IsGroup && x.Visible);
        var listActionGroup = Actions.ToList().FindAll(x => x.IsGroup && x.Visible);

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

        foreach (var action in listAction)
        {
            action.ShowAsButton = true;
            elementGroup.AppendElement(action.GetHtmlElement());
        }

        if (listActionGroup.Count > 0)
        {
            elementGroup.AppendElement(GetHtmlCaretButton());
            elementGroup.AppendElement(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu dropdown-menu-right");
                AddGroupActions(ul, listActionGroup);
            });
        }
    }

    private void AddGroupActions(HtmlElement ul, List<JJLinkButton> listAction)
    {
        foreach (var action in listAction)
        {
            if (action.DividerLine)
            {
                ul.AppendElement(HtmlTag.Li, li =>
                {
                    li.WithAttribute("role", "separator").WithCssClass("divider dropdown-divider");
                });
            }

            ul.AppendElement(HtmlTag.Li, li =>
            {
                li.WithCssClass("dropdown-item").AppendElement(action.GetHtmlElement());
            });
        }
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

    private HtmlElement GetHtmlCaretButton()
    {
        var html = new HtmlElement(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute(BootstrapHelper.DataToggle, "dropdown")
            .WithAttribute("aria-haspopup", "true")
            .WithAttribute("aria-expanded", "false")
            .WithCssClass(BootstrapHelper.DefaultButton)
            .WithCssClass("dropdown-toggle btn-outline-secondary")
            .AppendElementIf(BootstrapHelper.Version == 3, HtmlTag.Span, s =>
            {
                s.WithCssClass("caret")
                 .WithToolTip(Translate.Key("More Options"));
            });

        return html;
    }

}
