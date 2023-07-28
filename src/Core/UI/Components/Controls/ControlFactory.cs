using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Web.Factories;

public class ControlFactory
{
    private IEnumerable<IControlFactory> Factories { get; }
    private IFieldVisibilityService FieldVisibilityService { get; }

    public ControlFactory(
        IEnumerable<IControlFactory> factories,
        IFieldVisibilityService fieldVisibilityService)
    {
        Factories = factories;
        FieldVisibilityService = fieldVisibilityService;
    }

    public TFactory GetFactory<TFactory>() where TFactory : IControlFactory
    {
        return (TFactory)Factories.First(f => f is TFactory);
    }

    public TControl Create<TControl>() where TControl : JJBaseControl
    {
        var factory = (IControlFactory<TControl>)Factories.First(f => f is IControlFactory<TControl>);

        return factory.Create();
    }

    public TControl Create<TControl>(ControlContext controlContext) where TControl : JJBaseControl
    {
        var factory = (IControlFactory<TControl>)Factories.First(f => f is IControlFactory<TControl>);

        return factory.Create(
            controlContext.FormElement,
            controlContext.Field,
            controlContext.FormStateData,
            controlContext.ParentName,
            controlContext.Value);
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
            return GetFactory<TextRangeFactory>().Create(formElement, field, stateData, parentName, value);
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

        var context = new ControlContext(formElement, field, formStateData, parentName, value);

        JJBaseControl control;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                control = Create<JJComboBox>(context);
                break;
            case FormComponent.Search:
                control = Create<JJSearchBox>(context);
                break;
            case FormComponent.Lookup:
                control = Create<JJLookup>(context);
                break;
            case FormComponent.CheckBox:
                control = Create<JJCheckBox>(context);

                if (formStateData.PageState != PageState.List)
                    ((JJCheckBox)control).Text = string.IsNullOrEmpty(field.Label) ? field.Name : field.Label;

                break;
            case FormComponent.TextArea:
                control = Create<JJTextArea>(context);
                break;
            case FormComponent.Slider:
                control = Create<JJSlider>(context);
                break;
            case FormComponent.File:
                if (formStateData.PageState == PageState.Filter)
                {
                    control = Create<JJTextBox>(context);
                }
                else
                {
                    control = Create<JJTextFile>(context);
                }

                break;
            default:
                control = Create<JJTextGroup>(context);
                break;
        }

        control.ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly && formStateData.PageState != PageState.Filter;

        return control;
    }
}

public class ControlFactory<T> : IControlFactory<T> where T : JJBaseControl
{
    private IServiceProvider ServiceProvider { get; }

    public ControlFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IControlFactory<TComponent> GetFactory<TComponent>() where TComponent : JJBaseControl
    {
        var factories = ServiceProvider.GetRequiredService<IEnumerable<IComponentFactory>>();
        return (IControlFactory<TComponent>)factories.First(f => f is IControlFactory<TComponent>);
    }

    public T Create()
    {
        var factory = GetFactory<T>();
        return factory.Create();
    }

    public T Create(FormElement formElement, FormElementField field, FormStateData formStateData = null,
        string parentName = null,
        object value = null)
    {
        var factory = GetFactory<T>();
        return factory.Create(formElement, field, formStateData, parentName, value);
    }
}