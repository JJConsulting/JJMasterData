using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using static JJMasterData.Core.UI.DateFormatMapper;

namespace JJMasterData.Core.UI.Components;

public sealed class TextGroupFactory(
    IFormValues formValues,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ActionButtonFactory actionButtonFactory)
    : IControlFactory<JJTextGroup>
{
    public JJTextGroup Create()
    {
        return new JJTextGroup(formValues);
    }


    public JJTextGroup Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var value = context.Value;
        var formStateData = context.FormStateData;
        var textGroup = Create(field, context.FormStateData.PageState);

        if (field.Component == FormComponent.Currency)
            value = value?.ToString()?.Replace(RegionInfo.CurrentRegion.CurrencySymbol, string.Empty).Trim();

        textGroup.Text = value?.ToString() ?? string.Empty;

        if (formStateData.PageState is not PageState.Filter)
            AddUserActions(formElement, field, context, textGroup);

        return textGroup;
    }


    private void AddUserActions(
        FormElement formElement,
        FormElementField field,
        ControlContext controlContext,
        JJTextGroup textGroup)
    {
        var actions = field.Actions.GetAllSorted().Where(x => x.IsVisible);

        foreach (var action in actions)
        {
            var actionContext = new ActionContext
            {
                Action = action,
                FormElement = formElement,
                FormStateData = controlContext.FormStateData,
                FieldName = field.Name,
                ParentComponentName = controlContext.ParentComponentName
            };

            var link = actionButtonFactory.CreateFieldButton(action, actionContext);

            textGroup.Actions.Add(link);
        }
    }

    public JJTextGroup Create(FormElementField field, PageState pageState)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var textGroup = Create();
        textGroup.SetAttributes(field.Attributes);
        textGroup.MaxLength = field.Size;
        textGroup.NumberOfDecimalPlaces = field.NumberOfDecimalPlaces;
        textGroup.Name = field.Name;

        SetDefaultAttributes(textGroup, field, pageState);

        return textGroup;
    }

    private void SetDefaultAttributes(JJTextGroup textGroup, FormElementField field, PageState pageState)
    {
        HashSet<string> cssClassList =
        [
            "form-control"
        ];

        var component = field.Component;
        var isMultiFilter = field.Filter.Type.IsMultiValues && pageState == PageState.Filter;

        switch (component)
        {
            case FormComponent.Currency:
                cssClassList.Add(BootstrapHelper.TextRight);

                textGroup.MaxLength = 18;

                if (textGroup.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) &&
                    !string.IsNullOrEmpty(cultureInfoName))
                {
                    var cultureInfo = CultureInfo.GetCultureInfo(cultureInfoName);
                    textGroup.CultureInfo = cultureInfo;
                    textGroup.Addons = new InputAddons(cultureInfo.NumberFormat.CurrencySymbol);
                }
                else
                {
                    textGroup.Addons = new InputAddons(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
                }

                textGroup.InputType = InputType.Currency;
                textGroup.Attributes["onkeypress"] = "return jjutil.justNumber(event);";
                break;
            case FormComponent.Number:
                cssClassList.Add(BootstrapHelper.TextRight);
                textGroup.MaxLength = 22;
                textGroup.InputType = InputType.Number;

                if (textGroup.NumberOfDecimalPlaces == 0)
                    textGroup.Attributes["onkeypress"] = "return jjutil.justNumber(event);";

                textGroup.Attributes["step"] = textGroup.Attributes.TryGetValue("step", out var stepValue) ? stepValue : 1.ToString();
                textGroup.Attributes["onclick"] = "this.select();";

                textGroup.MinValue ??= int.MinValue;
                textGroup.MaxValue ??= int.MaxValue;
                break;
            case FormComponent.Cnpj:
                textGroup.MaxLength = 18;
                textGroup.InputType = InputType.Text;
                textGroup.Attributes["onclick"] = "this.select();";
                textGroup.Attributes["data-inputmask"] =
                    "'mask': '[**.***.***/****-99]', 'placeholder':'', 'greedy': 'false'";
                break;
            case FormComponent.Cpf:
                textGroup.MaxLength = 14;
                textGroup.InputType = InputType.Text;
                textGroup.Attributes["onclick"] = "this.select();";
                textGroup.Attributes["data-inputmask"] =
                    "'mask': '[999.999.999-99]', 'placeholder':'', 'greedy': 'false'";
                break;
            case FormComponent.CnpjCpf:
                textGroup.MaxLength = 18;
                textGroup.InputType = InputType.Text;
                break;
            case FormComponent.Cep:
                textGroup.MaxLength = 9;
                textGroup.InputType = InputType.Text;
                textGroup.Attributes["data-inputmask"] = "'mask': '[99999-999]', 'placeholder':'', 'greedy': 'false'";
                break;
            case FormComponent.Password:
                textGroup.InputType = InputType.Password;
                break;
            case FormComponent.Tel:
                textGroup.Addons = new InputAddons
                {
                    Tooltip = stringLocalizer["Brazil Phone Number"],
                    Text = "+55"
                };
                textGroup.MaxLength = 15;
                textGroup.InputType = InputType.Tel;
                textGroup.Attributes["data-inputmask"] =
                    "'mask': '[(99) 9999[9]-9999]', 'placeholder':'', 'greedy': 'false'";
                break;
            case FormComponent.Hour:
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 5;
                textGroup.GroupCssClass = "date";

                if (!isMultiFilter)
                {
                    textGroup.Attributes["data-inputmask-alias"] = "datetime";
                    textGroup.Attributes["data-inputmask-inputFormat"] = "HH:MM";
                    textGroup.Attributes["data-inputmask-displayFormat"] = "HH:MM";
                    textGroup.Attributes["data-inputmask-placeholder"] = "";
                }

                textGroup.Attributes["data-input"] = "date";
                textGroup.Attributes["data-mode"] = isMultiFilter ? "multiple" : "single";

                break;

            case FormComponent.Date:
            {
                var currentCulture = CultureInfo.CurrentUICulture;
                var inputmask = GetInputmask(currentCulture);

                textGroup.GroupCssClass = "flatpickr date jjform-date";
                textGroup.Actions.Add(GetDateAction(textGroup.Enabled));
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 10;

                textGroup.Attributes["data-flatpickr-mask"] = GetFlatpickr(currentCulture);

                if (!isMultiFilter)
                {
                    textGroup.Attributes["data-inputmask-alias"] = "datetime";
                    textGroup.Attributes["data-inputmask-inputFormat"] = inputmask;
                    textGroup.Attributes["data-inputmask-displayFormat"] = inputmask;
                    textGroup.Attributes["data-inputmask-placeholder"] = "";
                }

                textGroup.Attributes["data-input"] = "date";
                textGroup.Attributes["data-mode"] = isMultiFilter ? "multiple" : "single";

                break;
            }

            case FormComponent.DateTime:
            {
                var currentCulture = CultureInfo.CurrentUICulture;
                var inputmask = GetInputmask(currentCulture, includeTime: true);

                textGroup.GroupCssClass = "flatpickr date jjform-datetime";
                textGroup.Actions.Add(GetDateAction(textGroup.Enabled));
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 19;
                textGroup.Attributes["data-flatpickr-mask"] = GetFlatpickr(currentCulture, includeTime: true);

                if (!isMultiFilter)
                {
                    textGroup.Attributes["data-inputmask-alias"] = "datetime";
                    textGroup.Attributes["data-inputmask-inputFormat"] = inputmask;
                    textGroup.Attributes["data-inputmask-displayFormat"] = inputmask;
                    textGroup.Attributes["data-inputmask-placeholder"] = "";
                }

                textGroup.Attributes["data-input"] = "date";
                textGroup.Attributes["data-mode"] = isMultiFilter ? "multiple" : "single";

                break;
            }
            case FormComponent.Percentage:
                cssClassList.Add(BootstrapHelper.TextRight);
                textGroup.MaxLength = 18;
                textGroup.Addons = new InputAddons(CultureInfo.CurrentCulture.NumberFormat.PercentSymbol);
                textGroup.InputType = InputType.Percentage;
                textGroup.Attributes["onkeypress"] = "return jjutil.justNumber(event);";
                break;
            default:
                textGroup.InputType = InputType.Text;
                if (pageState == PageState.Filter && isMultiFilter)
                {
                    textGroup.Attributes.Add("data-role", "tagsinput");
                    textGroup.MaxLength = 0;
                }
                break;
        }

        textGroup.Attributes["class"] = string.Join(" ", cssClassList);
    }


    private JJLinkButton GetDateAction(bool isEnabled)
    {
        var btn = new JJLinkButton
        {
            IconClass = "fa fa-calendar",
            Tooltip = stringLocalizer["Calendar"],
            Enabled = isEnabled,
            Attributes =
            {
                { "data-toggle", "date" },
                { "tabindex", "-1" }
            }
        };

        return btn;
    }
}
