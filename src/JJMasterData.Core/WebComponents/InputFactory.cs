using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace JJMasterData.Core.WebComponents
{
    internal static class InputFactory
    {
        internal static JJTextGroup GetInstance(FormElementField f,
                            object value,
                            bool enable = true,
                            bool readOnly = false,
                            string name = null)
        {
            
            var textGroup = GetInstance(f, name);
            var textBox = textGroup.TextBox;


            textBox.Enable = enable;
            textBox.ReadOnly = readOnly;

            if (f.Component == FormComponent.Currency)
                value = value?.ToString()?.Replace("R$", string.Empty).Trim();

            textBox.Text = value?.ToString() ?? string.Empty;

            return textGroup;
        }

        internal static JJTextGroup GetInstance(FormElementField f, string name = null)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(FormElementField));


            var textGroup = new JJTextGroup();
            var textBox = textGroup.TextBox;

            textBox.SetAttr(f.Attributes);
            textBox.MaxLength = f.Size;
            textBox.NumberOfDecimalPlaces = f.NumberOfDecimalPlaces;
            textBox.Name = name ?? f.Name;
            textBox.MinValue = f.MinValue;
            textBox.MaxValue = f.MaxValue;

            SetDefaultAttrs(textGroup, f.Component);

            return textGroup;
        }

        internal static void SetDefaultAttrs(JJTextGroup textGroup, FormComponent type)
        {
            var textBox = textGroup.TextBox;
            var listClass = new List<string>();
            
            listClass.Add("form-control");

            switch (type)
            {
                case FormComponent.Currency:
                    listClass.Add(BootstrapHelper.TextRight);
                    textGroup.Addons = new InputAddons(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
                    textBox.MaxLength = 18;
                    textBox.InputType = InputType.Number;
                    textBox.SetAttr("onclick", "this.select();");
                    textBox.SetAttr("onkeypress", "return jjutil.justNumber(event);");
                    break;
                case FormComponent.Number:
                    listClass.Add(BootstrapHelper.TextRight);
                    textBox.MaxLength = 22;
                    textBox.InputType = InputType.Number;
                    textBox.SetAttr("step", "1");
                    textBox.SetAttr("onclick", "this.select();");
                    break;
                case FormComponent.Cnpj:
                    textBox.MaxLength = 18;
                    textBox.InputType = InputType.Text;
                    textBox.SetAttr("onclick", "this.select();");
                    textBox.SetAttr("data-inputmask", "'mask': '[99.999.999/9999-99]', 'placeholder':'', 'greedy': 'false'");
                    break;
                case FormComponent.Cpf:
                    textBox.MaxLength = 14;
                    textBox.InputType = InputType.Text;
                    textBox.SetAttr("onclick", "this.select();");
                    textBox.SetAttr("data-inputmask", "'mask': '[999.999.999-99]', 'placeholder':'', 'greedy': 'false'");
                    break;
                case FormComponent.CnpjCpf:
                    textBox.MaxLength = 18;
                    textBox.InputType = InputType.Text;
                    break;
                case FormComponent.Cep:
                    textBox.MaxLength = 9;
                    textBox.InputType = InputType.Text;
                    textBox.SetAttr("data-inputmask", "'mask': '[99999-999]', 'placeholder':'', 'greedy': 'false'");
                    break;
                case FormComponent.Password:
                    textBox.InputType = InputType.Password;
                    break;
                case FormComponent.Tel:
                    textGroup.Addons = new InputAddons
                    {
                        ToolTip = "Brasil",
                        Text = "+55"
                    };
                    textBox.MaxLength = 15;
                    textBox.InputType = InputType.Tel;
                    textBox.SetAttr("data-inputmask", "'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'");
                    break;
                case FormComponent.Hour:
                    var btn = GetDateAction();
                    btn.IconClass = "fa fa-clock";
                    textGroup.Actions.Add(btn);

                    textBox.InputType = InputType.Text;
                    textBox.MaxLength = 5;
                    textGroup.CssClass = "flatpickr date jjform-hour";
                    textBox.SetAttr("data-input", "date");
                    break;
                case FormComponent.Date:
                    textGroup.CssClass = "flatpickr date jjform-date";
                    textGroup.Actions.Add(GetDateAction());
                    textBox.InputType = InputType.Text;
                    textBox.MaxLength = 10;
                    textBox.SetAttr("data-input", "date");
                    break;
                case FormComponent.DateTime:
                    textGroup.CssClass = "flatpickr date jjform-datetime";
                    textGroup.Actions.Add(GetDateAction());
                    textBox.InputType = InputType.Text;
                    textBox.MaxLength = 19;
                    textBox.SetAttr("data-input", "date");
                    break;
                default:
                    textBox.InputType = InputType.Text;
                    break;

            }

            textBox.SetAttr("class", string.Join(" ", listClass));
        }


        private static JJLinkButton GetDateAction()
        {
            var btn = new JJLinkButton();
            btn.IconClass = $"fa fa-{BootstrapHelper.DateIcon}";
            btn.ToolTip = Translate.Key("Calendar");
            btn.SetAttr("data-toggle", "date");
            btn.SetAttr("tabindex", "-1");
            return btn;
        }

    }
}
