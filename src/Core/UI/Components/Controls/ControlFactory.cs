#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Factories;

public class ControlFactory 
{
    private IServiceScopeFactory ServiceScopeFactory { get; }
    private IExpressionsService ExpressionsService { get; }

    public IControlFactory<JJCheckBox> CheckBox => GetControlFactory<JJCheckBox>();
    public IControlFactory<JJComboBox> ComboBox => GetControlFactory<JJComboBox>();
    public IControlFactory<JJLookup> Lookup => GetControlFactory<JJLookup>();
    public IControlFactory<JJSearchBox> SearchBox => GetControlFactory<JJSearchBox>();
    public IControlFactory<JJSlider> Slider => GetControlFactory<JJSlider>();
    public IControlFactory<JJTextArea> TextArea => GetControlFactory<JJTextArea>();
    public IControlFactory<JJTextBox> TextBox => GetControlFactory<JJTextBox>();
    public IControlFactory<JJTextGroup> TextGroup => GetControlFactory<JJTextGroup>();
    public IControlFactory<JJTextFile> TextFile => GetControlFactory<JJTextFile>();
    public IControlFactory<JJTextRange> TextRange => GetControlFactory<JJTextRange>();

    public ControlFactory(IServiceScopeFactory serviceScopeFactory,
        IExpressionsService expressionsService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        ExpressionsService = expressionsService;
    }

    private IServiceProvider ServiceProvider
    {
        get
        {
            var scope = ServiceScopeFactory.CreateScope();
            return scope.ServiceProvider;
        }
    }

    private IControlFactory<TControl> GetControlFactory<TControl>() where TControl : ControlBase
    {
        return ServiceProvider.GetRequiredService<IControlFactory<TControl>>();
    }

    internal TControl Create<TControl>(FormElement formElement, FormElementField field, ControlContext controlContext)
        where TControl : ControlBase
    {
        var factory = ServiceProvider.GetRequiredService<IControlFactory<TControl>>();

        return factory.Create(
            formElement,
            field,
            controlContext);
    }

    public async Task<ControlBase> CreateAsync(
        FormElement formElement,
        FormElementField field,
        FormStateData formStateData,
        object? value = null)
    {
        var context = new ControlContext(formStateData, value);
        if (formStateData.PageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            var factory = GetControlFactory<JJTextRange>();
            return factory.Create(formElement, field, context);
        }

        var control = Create(formElement, field, context);
        control.Enabled = await ExpressionsService.GetBoolValueAsync(field.EnableExpression, formStateData);

        return control;
    }


    internal ControlBase Create(
        FormElement formElement,
        FormElementField field,
        ControlContext context)
    {
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        var formStateData = context.FormStateData;

        ControlBase control;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                control = Create<JJComboBox>(formElement, field, context);
                break;
            case FormComponent.Search:
                control = Create<JJSearchBox>(formElement, field, context);
                break;
            case FormComponent.Lookup:
                control = Create<JJLookup>(formElement, field, context);
                break;
            case FormComponent.CheckBox:
                control = Create<JJCheckBox>(formElement, field, context);

                if (formStateData.PageState != PageState.List)
                    ((JJCheckBox)control).Text = field.LabelOrName;

                break;
            case FormComponent.TextArea:
                control = Create<JJTextArea>(formElement, field, context);
                break;
            case FormComponent.Slider:
                control = Create<JJSlider>(formElement, field, context);
                break;
            case FormComponent.File:
                if (formStateData.PageState == PageState.Filter)
                {
                    control = Create<JJTextBox>(formElement, field, context);
                }
                else
                {
                    control = Create<JJTextFile>(formElement, field, context);
                }

                break;
            default:
                control = Create<JJTextGroup>(formElement, field, context);
                break;
        }

        control.ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly && formStateData.PageState != PageState.Filter;

        return control;
    }
}