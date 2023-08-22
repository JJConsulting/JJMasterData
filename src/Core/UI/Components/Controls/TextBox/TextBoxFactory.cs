using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components.Widgets;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Factories;

internal class TextBoxFactory : IControlFactory<JJTextGroup>
{
    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private LinkButtonFactory LinkButtonFactory { get; }

    public TextBoxFactory(IHttpContext httpContext, 
                          IStringLocalizer<JJMasterDataResources> stringLocalizer, 
                          LinkButtonFactory linkButtonFactory)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
        LinkButtonFactory = linkButtonFactory;
    }
    
    public JJTextGroup? Create()
    {
        return new JJTextGroup(HttpContext);
    }
    
    public JJTextGroup Create(FormElementField field, object value)
    {
        var textGroup = Create(field);

        if (field.Component == FormComponent.Currency)
            value = value?.ToString()?.Replace(RegionInfo.CurrentRegion.CurrencySymbol, string.Empty).Trim();

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

        if (formStateData.PageState == PageState.Filter)
            textGroup.Actions = textGroup.Actions.Where(a => a.ShowInFilter).ToList();
        else
            //TODO: make this synchronous passing if the action is visible and enabled;
            AddUserActions(formElement, field, context, textGroup).GetAwaiter().GetResult();

        return textGroup;
    }


    private async Task AddUserActions(
                                FormElement formElement,
                                FormElementField field,
                                ControlContext controlContext, 
                                JJTextGroup textGroup)
    {
        var actions = field.Actions.GetAllSorted().FindAll(x => x.IsVisible);

        var actionContext = new ActionContext
        {
            FormElement = formElement,
            FormStateData = controlContext.FormStateData,
            ParentComponentName = controlContext.ParentComponentName,
            FieldName = field.Name,
            IsExternalRoute = textGroup.IsExternalRoute
        };
        
        foreach (var action in actions)
        {
            var link = await LinkButtonFactory.CreateFieldLinkAsync(action,actionContext);
            textGroup.Actions.Add(link);
        }
    }

    public JJTextGroup Create(FormElementField field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var textGroup = new JJTextGroup(HttpContext);
        textGroup.SetAttr(field.Attributes);
        textGroup.MaxLength = field.Size;
        textGroup.NumberOfDecimalPlaces = field.NumberOfDecimalPlaces;
        textGroup.Name = field.Name;
        SetDefaultAttrs(textGroup, field.Component);

        return textGroup;
    }

    public JJTextGroup CreateTextDate()
    {
        var textGroup = new JJTextGroup(HttpContext);
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
                    ToolTip = "Brasil",
                    Text = "+55"
                };
                textGroup.MaxLength = 15;
                textGroup.InputType = InputType.Tel;
                textGroup.SetAttr("data-inputmask",
                    "'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'");
                break;
            case FormComponent.Hour:
                var btn = GetDateAction();
                btn.IconClass = "fa fa-clock";
                textGroup.Actions.Add(btn);

                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 5;
                textGroup.GroupCssClass = "flatpickr date jjform-hour";
                textGroup.SetAttr("data-input", "date");
                break;
            case FormComponent.Date:
                textGroup.GroupCssClass = "flatpickr date jjform-date";
                textGroup.Actions.Add(GetDateAction());
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 10;
                textGroup.SetAttr("data-input", "date");
                break;
            case FormComponent.DateTime:
                textGroup.GroupCssClass = "flatpickr date jjform-datetime";
                textGroup.Actions.Add(GetDateAction());
                textGroup.InputType = InputType.Text;
                textGroup.MaxLength = 19;
                textGroup.SetAttr("data-input", "date");
                break;
            default:
                textGroup.InputType = InputType.Text;
                break;
        }

        textGroup.SetAttr("class", string.Join(" ", listClass));
    }

    private JJLinkButton GetDateAction()
    {
        var btn = new JJLinkButton
        {
            IconClass = $"fa fa-{BootstrapHelper.DateIcon}",
            ToolTip = StringLocalizer["Calendar"],
            ShowInFilter = true
        };

        btn.SetAttr("data-toggle", "date");
        btn.SetAttr("tabindex", "-1");
        return btn;
    }
}