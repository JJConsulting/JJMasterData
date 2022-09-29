using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

public class JJTextBox : JJBaseControl
{

    private List<JJLinkButton> _actions;

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

    public InputType InputType { get; set; }

    public int NumberOfDecimalPlaces { get; set; }

    public float? MinValue { get; set; }

    public float? MaxValue { get; set; }

    /// <summary>
    /// Text info on left of component
    /// </summary>
    internal InputAddons Addons { get; set; }

    internal string CssInputGroup { get; set; }

    public JJTextBox()
    {
        InputType = InputType.Text;
        Visible = true;
        Enable = true;
    }

    public static JJTextBox GetInstance(FormElementField f, string name = null)
    {
        return TextBoxFactory.GetInstance(f, name);
    }

    internal override HtmlElement GetHtmlElement()
    {
        var input = GetHtmlInput();
        bool hasAction = Actions.ToList().Exists(x => x.Visible);
        bool hasAddons = Addons != null;

        if (!hasAction && !hasAddons)
            return input;

        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        if (defaultAction != null && defaultAction.Enabled && Enable)
        {
            input.WithCssClass("default-option");
            input.WithAttribute("onchange", defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlElement(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(CssInputGroup);

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


    private HtmlElement GetHtmlInput()
    {
        string inputType = InputType.ToString().ToLower();
        if (NumberOfDecimalPlaces > 0)
        {
            inputType = "text";
            CssClass += " jjdecimal";
        }

        var html = new HtmlElement(HtmlTag.Input)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithAttribute("type", inputType)
            .WithCssClass(CssClass)
            .WithToolTip(Translate.Key(ToolTip))
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(NumberOfDecimalPlaces > 0, "jjdecimalplaces", NumberOfDecimalPlaces.ToString())
            .WithAttributeIf(NumberOfDecimalPlaces == 0, "onkeypress", "return jjutil.justNumber(event);")
            .WithAttributeIf(MinValue != null, "min", MinValue?.ToString())
            .WithAttributeIf(MaxValue != null, "max", MaxValue?.ToString())
            .WithAttributeIf(!string.IsNullOrEmpty(Text), "value", Text);

        if (!Enable)
        {
            bool hasDefaultAction = Actions.Exists(x => x.IsDefaultOption && x.Visible);
            if (hasDefaultAction)
                html.WithAttribute("readonly", "readonly");
            else
                html.WithAttribute("disabled", "disabled");
        }
        else
        {
            html.WithAttributeIf(ReadOnly, "readonly", "readonly");
        }

        return html;
    }
}
