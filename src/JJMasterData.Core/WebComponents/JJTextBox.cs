using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

public class JJTextBox : JJBaseControl
{

    private List<JJLinkButton> _actions;

    /// <summary>
    /// Ações do campo
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
    /// Tipo do componente
    /// </summary>
    public InputType InputType { get; private set; }

    /// <summary>
    /// Numero de casas decimais
    /// Default(2)
    /// </summary>
    /// <remarks>
    /// Propriedade válida somente para tipos numéricos
    /// </remarks>
    public int NumberOfDecimalPlaces { get; set; }

    public float? MinValue { get; set; }
    public float? MaxValue { get; set; }

    private bool HasAction
    {
        get
        {
            return Actions.ToList().Exists(x => x.Visible);
        }
    }

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
                Actions.Add(new JJLinkButton
                {
                    Text = "R$"
                });

                SetAttr("type", "number");
                SetAttr("onclick", "this.select();");
                SetAttr("onkeypress", "return jjutil.justNumber(event);");
                break;
            case InputType.Number:
                listClass.Add(BootstrapHelper.TextRight);
                MaxLength = 22;
                SetAttr("step", "1");
                SetAttr("onclick", "this.select();");

                if (NumberOfDecimalPlaces > 0)
                {
                    SetAttr("type", "text");
                    listClass.Add("jjdecimal");
                }
                else
                {
                    SetAttr("type", "number");
                    SetAttr("onkeypress", "return jjutil.justNumber(event);");
                }
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
        if (!HasAction)
            return input;

        //Actions
        var listAction = Actions.ToList().FindAll(x => !x.IsGroup && x.Visible);
        var listActionGroup = Actions.ToList().FindAll(x => x.IsGroup && x.Visible);
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);

        if (defaultAction != null && defaultAction.Enabled && Enable)
        {
            input.WithCssClass("default-option");
            input.WithAttribute("onchange", defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlElement(HtmlTag.Div)
            .WithCssClass("input-group jjform-action")
            .AppendElement(input);

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

        return inputGroup;
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
        var html = new HtmlElement(HtmlTag.Input)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .WithToolTip(Translate.Key(ToolTip))
            .WithAttributeIf(MaxLength > 0, "maxlength", MaxLength.ToString())
            .WithAttributeIf(NumberOfDecimalPlaces > 0, "jjdecimalplaces", NumberOfDecimalPlaces.ToString())
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
