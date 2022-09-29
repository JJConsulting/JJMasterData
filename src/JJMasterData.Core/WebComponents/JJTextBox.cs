using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    public InputType InputType { get; private set; }

    public int NumberOfDecimalPlaces { get; set; }

    public float? MinValue { get; set; }

    public float? MaxValue { get; set; }

    /// <summary>
    /// Text info on left of component
    /// </summary>
    internal string AddonsText { get; set; }

    public JJTextBox() 
    {
        InputType = InputType.Textbox;
        Visible = true;
        Enable = true;
        SetDefaultAttrs(InputType);
    }

    public JJTextBox(InputType inputType) 
    {
        InputType = inputType;
        Visible = true;
        Enable = true;
        SetDefaultAttrs(inputType);
    }

    internal static JJTextBox GetInstance(FormElementField f,
                                object value,
                                bool enable = true,
                                bool readOnly = false,
                                string name = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(FormElementField));

        int type = (int)f.Component;
        var textBox = new JJTextBox((InputType)type);
        textBox.SetAttr(f.Attributes);
        textBox.MaxLength = f.Size;
        textBox.Enable = enable;
        textBox.ReadOnly = readOnly;
        textBox.NumberOfDecimalPlaces = f.NumberOfDecimalPlaces;
        textBox.Name = name ?? f.Name;
        textBox.MinValue = f.MinValue;
        textBox.MaxValue = f.MaxValue;

        if (textBox.InputType == InputType.Currency)
            value = value?.ToString()?.Replace("R$", string.Empty).Trim();

        textBox.Text = value?.ToString() ?? string.Empty;

        return textBox;
    }

    private void SetDefaultAttrs(InputType type)
    {
        var listClass = new List<string>();
        listClass.Add("form-control");

        switch (type)
        {
            case InputType.Currency:
                listClass.Add(BootstrapHelper.TextRight);
                MaxLength = 18;
                AddonsText = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
                SetAttr("type", "number");
                SetAttr("onclick", "this.select();");
                SetAttr("onkeypress", "return jjutil.justNumber(event);");
                break;
            case InputType.Number:
                listClass.Add(BootstrapHelper.TextRight);
                MaxLength = 22;
                SetAttr("step", "1");
                SetAttr("onclick", "this.select();");
                SetAttr("type", "number");
                break;
            case InputType.Cnpj:
                MaxLength = 18;
                SetAttr("type", "text");
                SetAttr("onclick", "this.select();");
                SetAttr("data-inputmask", "'mask': '[99.999.999/9999-99]', 'placeholder':'', 'greedy': 'false'");
                break;
            case InputType.Cpf:
                MaxLength = 14;
                SetAttr("type", "text");
                SetAttr("onclick", "this.select();");
                SetAttr("data-inputmask", "'mask': '[999.999.999-99]', 'placeholder':'', 'greedy': 'false'");
                break;
            case InputType.CnpjCpf:
                MaxLength = 18;
                SetAttr("type", "text");
                break;
            case InputType.Cep:
                MaxLength = 9;
                SetAttr("type", "text");
                SetAttr("data-inputmask", "'mask': '[99999-999]', 'placeholder':'', 'greedy': 'false'");
                break;
            case InputType.Password:
                SetAttr("type", "password");
                break;
            default:
                SetAttr("type", "text");
                break;

        }

        SetAttr("class", string.Join(" ", listClass));
    }

    internal override HtmlElement GetHtmlElement()
    {
        var input = GetHtmlInput();
        bool hasAction = Actions.ToList().Exists(x => x.Visible);
        bool hasAddons = !string.IsNullOrEmpty(AddonsText);

        if (!hasAction && !hasAddons)
            return input;
        
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        if (defaultAction != null && defaultAction.Enabled && Enable)
        {
            input.WithCssClass("default-option");
            input.WithAttribute("onchange", defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlElement(HtmlTag.Div)
            .WithCssClass("input-group jjform-action");

        if (hasAddons)
            inputGroup.AppendElement(HtmlTag.Span, s =>
            {
                s.WithCssClass(BootstrapHelper.InputGroupAddon)
                 .AppendText(AddonsText);
            });
            
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
        if (NumberOfDecimalPlaces > 0)
        {
            Attributes["type"] = "text";
            CssClass += " jjdecimal";
        }
        
        var html = new HtmlElement(HtmlTag.Input)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
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
