using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class TextGroupFactory(IFormValues formValues,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ActionButtonFactory actionButtonFactory)
    : IControlFactory<JJTextGroup>
{
    private IFormValues FormValues { get; } = formValues;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private ActionButtonFactory ActionButtonFactory { get; } = actionButtonFactory;

    public JJTextGroup Create()
    {
        return new JJTextGroup(FormValues);
    }
    
    public JJTextGroup Create(FormElementField field, object value)
    {
        var textGroup = Create(field);

        if (field.Component == FormComponent.Currency)
            value = value?.ToString().Replace(RegionInfo.CurrentRegion.CurrencySymbol, string.Empty).Trim();

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

        if (formStateData.PageState is PageState.Filter)
            textGroup.Actions.AddRange(textGroup.Actions.Where(a => a.ShowInFilter));
        else
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

            var link = ActionButtonFactory.CreateFieldButton(action,actionContext);
            
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
        var textGroup = new JJTextGroup(FormValues);
        SetDefaultAttrs(textGroup, FormComponent.Date);
        return textGroup;
    }

    private void SetDefaultAttrs(JJTextGroup textGroup, FormComponent type)
    {
        var listClass = new List<string>
        {
            "form-control"
        };

        switch (type)
        {
            case FormComponent.Currency:
                listClass.Add(BootstrapHelper.TextRight);
                textGroup.Addons = new InputAddons(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
                textGroup.MaxLength = 18;
                textGroup.InputType = InputType.Number;
                textGroup.SetAttr("onclick", "this.select();");
                textGroup.SetAttr("onkeypress", "return jjutil.justNumber(event);");
                break;
            case FormComponent.Number:
                listClass.Add(BootstrapHelper.TextRight);
                textGroup.MaxLength = 22;
                textGroup.InputType = InputType.Number;
                textGroup.SetAttr("step", textGroup.Attributes.TryGetValue("step", out var stepValue) ? stepValue : 1);
                textGroup.SetAttr("onclick", "this.select();");

                if (!textGroup.MinValue.HasValue)
                    textGroup.MinValue = int.MinValue;

                if (!textGroup.MaxValue.HasValue)
                    textGroup.MaxValue = int.MaxValue;

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
                var btn = GetDateAction(textGroup.Enabled);
                btn.IconClass = "fa fa-clock";
                textGroup.Actions.Add(btn);

                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 5;
                textGroup.GroupCssClass = "flatpickr date jjform-hour";
                // textGroup.SetAttr("data-inputmask",
                //     $"'alias': 'datetime','inputFormat': '[{Format.TimeFormat.ToLower()}]', 'displayFormat': '[{Format.TimeFormat.ToLower()}]','placeholder':''");
                textGroup.SetAttr("data-input", "date");
                break;
            case FormComponent.Date:
                textGroup.GroupCssClass = "flatpickr date jjform-date";
                textGroup.Actions.Add(GetDateAction(textGroup.Enabled));
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 10;
                // textGroup.SetAttr("data-inputmask",
                //     $"'alias': 'datetime','inputFormat': '[{Format.DateFormat.ToLower()}]','displayFormat': '[{Format.DateFormat.ToLower()}]',  'placeholder':''");
                textGroup.SetAttr("data-input", "date");
                break;
            case FormComponent.DateTime:
                textGroup.GroupCssClass = "flatpickr date jjform-datetime";
                textGroup.Actions.Add(GetDateAction(textGroup.Enabled));
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 19;
                // textGroup.SetAttr("data-inputmask",
                //     $"'alias': 'datetime','inputFormat': '[{Format.DateTimeFormat.ToLower()}]','displayFormat': '[{Format.DateTimeFormat.ToLower()}]', 'placeholder':''");
                textGroup.SetAttr("data-input", "date");
                break;
            default:
                textGroup.InputType = InputType.Text;
                break;
        }

        textGroup.SetAttr("class", string.Join(" ", listClass));
    }

    private JJLinkButton GetDateAction(bool isEnabled)
    {
        var btn = ActionButtonFactory.Create();
        btn.IconClass = $"fa fa-{BootstrapHelper.DateIcon}";
        btn.Tooltip = StringLocalizer["Calendar"];
        btn.ShowInFilter = true;
        btn.Enabled = isEnabled;
        btn.SetAttr("data-toggle", "date");
        btn.SetAttr("tabindex", "-1");
        return btn;
    }
}