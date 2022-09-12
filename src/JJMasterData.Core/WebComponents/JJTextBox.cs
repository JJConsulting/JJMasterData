using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

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
    public InputType InputType { get; set; }

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

    public JJTextBox()
    {
        Visible = true;
        Enable = true;
    }

    public JJTextBox(IDataAccess dataAccess) : base(dataAccess)
    {
        Visible = true;
        Enable = true;
    }

    public JJTextBox(FormElementField f)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        Visible = true;
        Enable = true;
        switch (f.Component)
        {
            case FormComponent.Password:
            case FormComponent.Email:
            case FormComponent.Number:
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
            case FormComponent.Cep:
            case FormComponent.QrCode:
                int type = (int)f.Component;
                InputType = (InputType)type;
                break;
            default:
                InputType = InputType.Textbox;
                break;
        }

        Name = f.Name;
        MaxLength = f.Size;
    }

    internal static JJTextBox GetInstance(FormElementField f,
                                object value,
                                bool enable = true,
                                bool readOnly = false,
                                string name = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        var textBox = new JJTextBox();
        switch (f.Component)
        {
            case FormComponent.Password:
            case FormComponent.Email:
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.CnpjCpf:
            case FormComponent.Cep:
            case FormComponent.Currency:
            case FormComponent.QrCode:
                int type = (int)f.Component;
                textBox.InputType = (InputType)type;
                break;
            case FormComponent.Number:
                textBox.MinValue = f.MinValue;
                textBox.MaxValue = f.MaxValue;
                textBox.InputType = InputType.Number;
                break;
            default:
                textBox.InputType = InputType.Textbox;
                break;
        }

        textBox.SetAttr(f.Attributes);
        textBox.MaxLength = f.Size;

        if (textBox.InputType == InputType.Currency)
            value = value?.ToString()?.Replace("R$", string.Empty).Trim();

        textBox.Text = value?.ToString() ?? string.Empty;

        textBox.Enable = enable;
        textBox.ReadOnly = readOnly;
        textBox.NumberOfDecimalPlaces = f.NumberOfDecimalPlaces;
        textBox.Name = name ?? f.Name;

        return textBox;
    }



    protected override string RenderHtml()
    {
        StringBuilder html = new StringBuilder();
        string cssClass = "form-control ";
        cssClass += !string.IsNullOrEmpty(CssClass) ? CssClass : "";


        //Actions
        var listAction = Actions.ToList().FindAll(x => !x.IsGroup && x.Visible);
        var listActionGroup = Actions.ToList().FindAll(x => x.IsGroup && x.Visible);
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        bool isCurrency = InputType == InputType.Currency; 
        bool hasAction = (listAction.Count > 0 || listActionGroup.Count > 0 || isCurrency);

        if (hasAction)
        {
            if (defaultAction != null && defaultAction.Enabled && !Enable)
                cssClass += "default-option ";

            html.Append($"<div class=\"{(BootstrapHelper.Version  == 3 ? "input-group" : string.Empty)} jjform-action\"> ");

            if (isCurrency)

                listAction.Add(new JJLinkButton
                {
                    Text = "R$"
                });
        }

        if(!hasAction)
            html.Append(GetInputHtml(cssClass, defaultAction));

        if (hasAction)
        {
            html.Append($"<div class=\"{BootstrapHelper.InputGroupBtn}\"> ");

            foreach (var action in listAction)
            {
                action.ShowAsButton = BootstrapHelper.Version == 3;
                action.Attributes.Add("style", "text-decoration:none");
                action.CssClass = BootstrapHelper.Version > 3 ? "input-group-text" : string.Empty;
                html.AppendLine(action.GetHtml());
            }

            if (BootstrapHelper.Version >= 4)
                html.Append(GetInputHtml(cssClass, defaultAction));


            if (listActionGroup.Count > 0)
            {
                html.AppendLine($"<button type=\"button\" class=\"{(BootstrapHelper.Version != 3 ? " form-control input-group-text" : BootstrapHelper.DefaultButton)} dropdown-toggle\" {BootstrapHelper.DataToggle}=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">");
                html.Append("  <span class=\"caret\" ");
                html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
                html.AppendFormat("title=\"{0}\">", Translate.Key("More Options"));
                html.AppendLine("</span>");
                html.AppendLine("</button>");
                html.AppendLine("<ul class=\"dropdown-menu dropdown-menu-right\">");

                foreach (var action in listActionGroup)
                {
                    if (action.DividerLine)
                        html.AppendLine("  <li role =\"separator\" class=\"divider\"></li>");

                    html.AppendLine("  <li class=\"dropdown-item\">");
                    html.Append(action.GetHtml());
                    html.AppendLine("  </li>");
                }
                html.AppendLine("</ul>");
            }

            html.Append("</div>");

            if(BootstrapHelper.Version == 3)
                html.Append(GetInputHtml(cssClass, defaultAction));

            html.Append("</div>");
        }

        return html.ToString();
    }

    private string GetInputHtml( string cssClass, JJLinkButton defaultAction)
    {
        var html = new StringBuilder();
        html.Append("<input id=\"");
        html.Append(Name);
        html.Append("\" name=\"");
        html.Append(Name);
        html.Append("\" ");

        switch (InputType)
        {
            case InputType.Number:
                {

                    html.Append("maxlength =\"22\" ");
                    html.Append($"min=\"{(MinValue == null ? "–2147483648" : MinValue)}\"" +
                                $"max=\"{(MaxValue == null ? "2147483648" : MaxValue)}\"" +
                                " step=\"1\" ");
                    html.Append("onclick=\"this.select();\" ");

                    switch (NumberOfDecimalPlaces)
                    {
                        case 0:
                            html.Append("type=\"number\" ");
                            html.Append("onkeypress=\"return jjutil.justNumber(event);\" ");
                            break;
                        case > 0:
                            html.Append("type=\"text\" ");
                            html.AppendFormat("jjdecimalplaces=\"{0}\" ", NumberOfDecimalPlaces);

                            cssClass += " jjdecimal";
                            break;
                    }

                    if (!string.IsNullOrEmpty(Text))
                    {
                        if (int.TryParse(Text.Replace(".", ""), out int @int))
                            Text = @int.ToString();
                    }
                    cssClass += BootstrapHelper.TextRight;
                    break;
                }

            case InputType.Currency:
                html.Append("type=\"number\" ");
                html.Append("maxlength =\"18\" ");
                html.Append("onclick=\"this.select();\" ");
                html.Append("onkeypress=\"return jjutil.justDecimal(event);\" ");
                cssClass += BootstrapHelper.TextRight;
                break;
            case InputType.Cnpj:
                html.Append("type=\"text\" ");
                html.Append("maxlength =\"18\" ");
                html.Append("onclick=\"this.select();\" ");
                html.Append("data-inputmask=\"'mask': '[99.999.999/9999-99]', 'placeholder':'', 'greedy': 'false'\" ");
                break;
            case InputType.Cpf:
                html.Append("type=\"text\" ");
                html.Append("maxlength =\"14\" ");
                html.Append("data-inputmask=\"'mask': '[999.999.999-99]', 'placeholder':'', 'greedy': 'false'\" ");
                break;
            case InputType.CnpjCpf:
                html.Append("type=\"text\" ");
                html.Append("maxlength =\"18\" ");
                break;
            case InputType.Password:
                html.Append("type=\"password\" ");
                if (MaxLength > 0)
                {
                    html.Append("maxlength =\"");
                    html.Append(MaxLength);
                    html.Append("\" ");
                }
                break;
            case InputType.Cep:
                html.Append("type=\"text\" ");
                html.Append("maxlength =\"9\" ");
                html.Append("data-inputmask=\"'mask': '[99999-999]', 'placeholder':'', 'greedy': 'false'\" ");
                break;
            default:
                html.Append("type=\"text\" ");
                if (MaxLength > 0)
                {
                    html.Append("maxlength =\"");
                    html.Append(MaxLength);
                    html.Append("\" ");
                }
                break;
        }

        html.Append("class=\"");
        html.Append(cssClass);
        html.Append("\" ");

        if (!string.IsNullOrEmpty(Text))
        {
            html.Append("value =\"");
            html.Append(Text);
            html.Append("\" ");
        }

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(ToolTip);
            html.Append("\" ");
        }

        if (defaultAction != null && !Enable)
        {
            html.Append(" onclick=\"");
            html.Append(defaultAction.OnClientClick);
            html.Append("\"");
        }

        if (ReadOnly)
            html.Append("readonly ");

        if (!Enable)
        {
            if (defaultAction != null)
                html.Append("readonly ");
            else
                html.Append("disabled ");
        }

        foreach (DictionaryEntry attr in Attributes)
        {

            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
            html.Append(" ");
        }

        html.Append("/>");

        return html.ToString();
    }
}
