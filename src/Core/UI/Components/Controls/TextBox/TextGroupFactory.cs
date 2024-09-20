using System;
using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class TextGroupFactory(
        IFormValues formValues,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory<JJLinkButtonGroup> linkButtonGroupFactory,
        ActionButtonFactory actionButtonFactory)
    : IControlFactory<JJTextGroup>
{
    public JJTextGroup Create()
    {
        return new JJTextGroup(linkButtonGroupFactory,formValues);
    }
    
    public JJTextGroup Create(FormElementField field, object value)
    {
        var textGroup = Create(field);

        if (field.Component == FormComponent.Currency)
        {
            value = value?.ToString()?.Replace(RegionInfo.CurrentRegion.CurrencySymbol, string.Empty).Trim();
        }
        
        textGroup.Text = value?.ToString() ?? string.Empty;
        
        return textGroup;
    }


    public JJTextGroup Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var value = context.Value;
        var formStateData = context.FormStateData;
        var textGroup = Create(field);

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
        var actions = field.Actions.GetAllSorted().FindAll(x => x.IsVisible);
        
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

            var link = actionButtonFactory.CreateFieldButton(action,actionContext);
            
            textGroup.Actions.Add(link);
        }
    }

    public JJTextGroup Create(FormElementField field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var textGroup = Create();
        textGroup.SetAttr(field.Attributes);
        textGroup.MaxLength = field.Size;
        textGroup.NumberOfDecimalPlaces = field.NumberOfDecimalPlaces;
        textGroup.Name = field.Name;
        SetDefaultAttrs(textGroup, field.Component);

        return textGroup;
    }

    public JJTextGroup CreateTextDate()
    {
        var textGroup = new JJTextGroup(linkButtonGroupFactory,formValues);
        SetDefaultAttrs(textGroup, FormComponent.Date);
        return textGroup;
    }

    private void SetDefaultAttrs(JJTextGroup textGroup, FormComponent component)
    {
        var listClass = new List<string>
        {
            "form-control"
        };

        switch (component)
        {
            case FormComponent.Currency:
                listClass.Add(BootstrapHelper.TextRight);

                textGroup.MaxLength = 18;
                
                if (textGroup.Attributes.TryGetValue(FormElementField.CultureInfoAttribute, out var cultureInfoName) && !string.IsNullOrEmpty(cultureInfoName))
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
                textGroup.SetAttr("onkeypress", "return jjutil.justNumber(event);");
                break;
            case FormComponent.Number:
                listClass.Add(BootstrapHelper.TextRight);
                textGroup.MaxLength = 22;
                textGroup.InputType = InputType.Number;
                
                if(textGroup.NumberOfDecimalPlaces == 0 )
                    textGroup.SetAttr("onkeypress", "return jjutil.justNumber(event);");
                
                textGroup.SetAttr("step", textGroup.Attributes.TryGetValue("step", out var stepValue) ? stepValue : 1);
                textGroup.SetAttr("onclick", "this.select();");

                textGroup.MinValue ??= int.MinValue;
                textGroup.MaxValue ??= int.MaxValue;

                break;
            case FormComponent.Cnpj:
                textGroup.MaxLength = 18;
                textGroup.InputType = InputType.Text;
                textGroup.SetAttr("onclick", "this.select();");
                textGroup.SetAttr("data-inputmask",
                    "'mask': '[99.999.999/9999-99]', 'placeholder':'', 'greedy': 'false'");
                break;
            case FormComponent.Cpf:
                textGroup.MaxLength = 14;
                textGroup.InputType = InputType.Text;
                textGroup.SetAttr("onclick", "this.select();");
                textGroup.SetAttr("data-inputmask",
                    "'mask': '[999.999.999-99]', 'placeholder':'', 'greedy': 'false'");
                break;
            case FormComponent.CnpjCpf:
                textGroup.MaxLength = 18;
                textGroup.InputType = InputType.Text;
                break;
            case FormComponent.Cep:
                textGroup.MaxLength = 9;
                textGroup.InputType = InputType.Text;
                textGroup.SetAttr("data-inputmask", "'mask': '[99999-999]', 'placeholder':'', 'greedy': 'false'");
                break;
            case FormComponent.Password:
                textGroup.InputType = InputType.Password;
                break;
            case FormComponent.Tel:
                textGroup.Addons = new InputAddons
                {
                    Tooltip = "Brasil",
                    Text = "+55"
                };
                textGroup.MaxLength = 15;
                textGroup.InputType = InputType.Tel;
                textGroup.SetAttr("data-inputmask",
                    "'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'");
                break;
            case FormComponent.Hour:
                var btn = GetDateAction(component,textGroup.Enabled);
                btn.IconClass = "fa fa-clock";
                textGroup.Actions.Add(btn);

                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 5;
                textGroup.GroupCssClass = "flatpickr date jjform-hour";
                textGroup.SetAttr("data-inputmask-alias", "datetime");
                textGroup.SetAttr("data-inputmask-inputFormat", "HH:MM");
                textGroup.SetAttr("data-inputmask-displayFormat","HH:MM");
                textGroup.SetAttr("data-inputmask-placeholder", "");
                textGroup.SetAttr("data-input", "date");
                break;
            case FormComponent.Date:
                textGroup.GroupCssClass = "flatpickr date jjform-date";
                textGroup.Actions.Add(GetDateAction(component,textGroup.Enabled));
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 10;
                textGroup.SetAttr("data-inputmask-alias", "datetime");
                textGroup.SetAttr("data-inputmask-inputFormat", Format.DateFormat.ToLowerInvariant());
                textGroup.SetAttr("data-inputmask-displayFormat", Format.DateFormat.ToLowerInvariant());
                textGroup.SetAttr("data-inputmask-placeholder", "");
                textGroup.SetAttr("data-input", "date");
                break;
            case FormComponent.DateTime:
                textGroup.GroupCssClass = "flatpickr date jjform-datetime";
                textGroup.Actions.Add(GetDateAction(component,textGroup.Enabled));
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 19;
                textGroup.SetAttr("data-inputmask-alias", "datetime");
                textGroup.SetAttr("data-inputmask-inputFormat", $"{Format.DateFormat.ToLowerInvariant()} HH:MM");
                textGroup.SetAttr("data-inputmask-displayFormat", $"{Format.DateFormat.ToLowerInvariant()} HH:MM");
                textGroup.SetAttr("data-inputmask-placeholder", "");
                textGroup.SetAttr("data-input", "date");
                break;
            default:
                textGroup.InputType = InputType.Text;
                break;
        }

        textGroup.SetAttr("class", string.Join(" ", listClass));
    }

    private JJLinkButton GetDateAction(FormComponent component, bool isEnabled)
    {
        var btn = actionButtonFactory.Create();
        btn.IconClass =component is FormComponent.Hour ? IconType.SolidClock.GetCssClass() : "fa fa-calendar";
        btn.Tooltip = component is FormComponent.Hour ? stringLocalizer["Clock"] : stringLocalizer["Calendar"];
        btn.Enabled = isEnabled;
        btn.SetAttr("data-toggle", "date");
        btn.SetAttr("tabindex", "-1");
        return btn;
    }
}