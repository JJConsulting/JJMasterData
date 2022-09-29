using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace JJMasterData.Core.WebComponents
{
    internal static class TextBoxFactory
    {
        internal static JJTextBox GetInstance(FormElementField f,
                            object value,
                            bool enable = true,
                            bool readOnly = false,
                            string name = null)
        {
            
            var textBox = GetInstance(f, name);

            textBox.Enable = enable;
            textBox.ReadOnly = readOnly;

            if (f.Component == FormComponent.Currency)
                value = value?.ToString()?.Replace("R$", string.Empty).Trim();

            textBox.Text = value?.ToString() ?? string.Empty;

            return textBox;
        }

        internal static JJTextBox GetInstance(FormElementField f, string name = null)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(FormElementField));


            var textBox = new JJTextBox();
            textBox.SetAttr(f.Attributes);
            textBox.MaxLength = f.Size;
            textBox.NumberOfDecimalPlaces = f.NumberOfDecimalPlaces;
            textBox.Name = name ?? f.Name;
            textBox.MinValue = f.MinValue;
            textBox.MaxValue = f.MaxValue;

            SetDefaultAttrs(textBox, f.Component);

            return textBox;
        }

        internal static void SetDefaultAttrs(JJTextBox textBox, FormComponent type)
        {
            var listClass = new List<string>();
            listClass.Add("form-control");

            switch (type)
            {
                case FormComponent.Currency:
                    listClass.Add(BootstrapHelper.TextRight);
                    textBox.MaxLength = 18;
                    textBox.Addons = new InputAddons(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
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
                    textBox.MaxLength = 15;
                    textBox.InputType = InputType.Tel;
                    textBox.Addons = new InputAddons
                    {
                        ToolTip = "Brasil",
                        Text = "+55"
                    };
                    textBox.SetAttr("data-inputmask", "'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'");
                    break;
                case FormComponent.Hour:
                    textBox.InputType = InputType.Text;
                    textBox.MaxLength = 5;
                    textBox.CssInputGroup = "flatpickr date jjform-hour";
                    textBox.SetAttr("data-input", "date");

                    var btn = GetDateAction();
                    btn.IconClass = "fa fa-clock";
                    textBox.Actions.Add(btn);

                    break;
                case FormComponent.Date:
                    textBox.InputType = InputType.Text;
                    textBox.MaxLength = 10;
                    textBox.CssInputGroup = "flatpickr date jjform-date";
                    textBox.SetAttr("data-input", "date");
                    textBox.Actions.Add(GetDateAction());
                    break;
                case FormComponent.DateTime:
                    textBox.InputType = InputType.Text;
                    textBox.MaxLength = 19;
                    textBox.CssInputGroup = "flatpickr date jjform-datetime";
                    textBox.SetAttr("data-input", "date");
                    textBox.Actions.Add(GetDateAction());
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
