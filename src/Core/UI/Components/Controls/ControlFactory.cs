#nullable enable

using System;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components.ColorPicker;
using JJMasterData.Core.UI.Components.TextRange;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.UI.Components;

public class ControlFactory(IServiceProvider serviceProvider,
    ExpressionsService expressionsService)
{
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ExpressionsService ExpressionsService { get; } = expressionsService;

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

    public ControlBase Create(
        FormElement formElement,
        FormElementField field,
        FormStateData formStateData,
        string parentComponentName,
        object? value = null)
    {
        var context = new ControlContext(formStateData, parentComponentName, value);
        if (formStateData.PageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            var factory = GetControlFactory<JJTextRange>();
            return factory.Create(formElement, field, context);
        }

        var control = Create(formElement, field, context);
        control.Enabled = ExpressionsService.GetBoolValue(field.EnableExpression, formStateData);
        
        if (field.ReadOnlyExpression != null)
            control.ReadOnly = ExpressionsService.GetBoolValue(field.ReadOnlyExpression, formStateData);
        
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

                if (field.Attributes.TryGetValue(FormElementField.IsSwitchAttribute, out var isSwitch))
                {
                    ((JJCheckBox)control).IsSwitch = isSwitch is true;
                }
                
                break;
            case FormComponent.RadioButtonGroup:
                control = Create<JJRadioButtonGroup>(formElement, field, context);
                break;
            case FormComponent.TextArea:
                control = Create<JJTextArea>(formElement, field, context);
                break;
            case FormComponent.Slider:
                control = Create<JJSlider>(formElement, field, context);
                break;
            case FormComponent.ColorPicker:
                control = Create<JJColorPicker>(formElement, field, context);
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
        control.SetAttr(field.Attributes);
        
        return control;
    }
}