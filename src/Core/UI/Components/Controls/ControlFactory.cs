using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Web.Factories;

public class ControlFactory
{
    private IServiceScopeFactory ServiceScopeFactory { get; }
    private IFieldVisibilityService FieldVisibilityService { get; }
    
    
    public ControlFactory(IServiceScopeFactory serviceScopeFactory, IFieldVisibilityService fieldVisibilityService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        FieldVisibilityService = fieldVisibilityService;
    }

    public IServiceProvider ServiceProvider
    {
        get
        {
            var scope = ServiceScopeFactory.CreateScope();
            return scope.ServiceProvider;
        }
    }
    
    public TFactory GetFactory<TFactory>() where TFactory : IControlFactory
    {
        return ServiceProvider.GetRequiredService<TFactory>();
    }

    public TControl Create<TControl>() where TControl : JJBaseControl
    {
        var factory = ServiceProvider.GetRequiredService<IControlFactory<TControl>>();

        return factory.Create();
    }

    public TControl Create<TControl>(FormElement formElement,FormElementField field, ControlContext controlContext) where TControl : JJBaseControl
    {
        var factory = ServiceProvider.GetRequiredService<IControlFactory<TControl>>();

        return factory.Create(
            formElement,
            field,
            controlContext);
    }

    public JJBaseControl Create(FormElement formElement,
        FormElementField field,
        IDictionary<string, dynamic> formValues,
        IDictionary<string, dynamic> userValues,
        PageState pageState,
        string parentName,
        object value = null)
    {
        var stateData = new FormStateData(userValues, formValues, pageState);

        if (pageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            return GetFactory<IControlFactory<JJTextRange>>().Create(formElement, field, new ControlContext(stateData, parentName, value));
        }

        var control = Create(formElement, field, stateData, parentName, value);

        control.Enabled = FieldVisibilityService.IsEnabled(field, pageState, formValues);

        return control;
    }

    public static bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter && field.Filter.Type == FilterMode.Range;
    }

    public JJBaseControl Create(
        FormElement formElement,
        FormElementField field,
        FormStateData formStateData,
        string parentName,
        object value)
    {
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        var context = new ControlContext(formStateData, parentName, value);

        JJBaseControl control;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                control = Create<JJComboBox>(formElement, field,context);
                break;
            case FormComponent.Search:
                control = Create<JJSearchBox>(formElement, field,context);
                break;
            case FormComponent.Lookup:
                control = Create<JJLookup>(formElement, field,context);
                break;
            case FormComponent.CheckBox:
                control = Create<JJCheckBox>(formElement, field,context);

                if (formStateData.PageState != PageState.List)
                    ((JJCheckBox)control).Text = string.IsNullOrEmpty(field.Label) ? field.Name : field.Label;

                break;
            case FormComponent.TextArea:
                control = Create<JJTextArea>(formElement, field,context);
                break;
            case FormComponent.Slider:
                control = Create<JJSlider>(formElement, field,context);
                break;
            case FormComponent.File:
                if (formStateData.PageState == PageState.Filter)
                {
                    control = Create<JJTextBox>(formElement, field,context);
                }
                else
                {
                    control = Create<JJTextFile>(formElement, field,context);
                }

                break;
            default:
                control = Create<JJTextGroup>(formElement, field,context);
                break;
        }

        control.ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly && formStateData.PageState != PageState.Filter;

        return control;
    }
}