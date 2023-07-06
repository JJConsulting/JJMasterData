using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public class FieldControlFactory
{
    private IExpressionsService ExpressionsService { get; }
    private IFieldVisibilityService FieldVisibilityService { get; }
    internal ComboBoxFactory ComboBoxFactory { get; }
    private SearchBoxFactory SearchBoxFactory { get; }
    internal LookupFactory LookupFactory { get; }
    private CheckBoxFactory CheckBoxFactory { get; }
    private SliderFactory SliderFactory { get; }
    private TextFileFactory TextFileFactory { get; }
    private TextAreaFactory TextAreaFactory { get; }
    private TextGroupFactory TextGroupFactory { get; }
    private TextRangeFactory TextRangeFactory { get; }
    public event EventHandler<ActionEventArgs> OnRenderAction;

    public FieldControlFactory(
        IExpressionsService expressionsService,
        IFieldVisibilityService fieldVisibilityService,
        ComboBoxFactory comboBoxFactory,
        SearchBoxFactory searchBoxFactory,
        LookupFactory lookupFactory,
        CheckBoxFactory checkBoxFactory,
        SliderFactory sliderFactory,
        TextFileFactory textFileFactory,
        TextAreaFactory textAreaFactory,
        TextGroupFactory textGroupFactory,
        TextRangeFactory textRangeFactory)
    {
        ExpressionsService = expressionsService;
        FieldVisibilityService = fieldVisibilityService;
        ComboBoxFactory = comboBoxFactory;
        SearchBoxFactory = searchBoxFactory;
        LookupFactory = lookupFactory;
        CheckBoxFactory = checkBoxFactory;
        SliderFactory = sliderFactory;
        TextFileFactory = textFileFactory;
        TextAreaFactory = textAreaFactory;
        TextGroupFactory = textGroupFactory;
        TextRangeFactory = textRangeFactory;
    }

    public JJBaseControl CreateControl(
        FormElement formElement,
        string componentName,
        FormElementField field, 
        PageState pageState, 
        IDictionary<string,dynamic> formValues,
        IDictionary<string,dynamic> userValues, 
        object value = null)
    {
        if (pageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            return TextRangeFactory.CreateTextRange(field, formValues);
        }
        
        var expOptions = new ExpressionOptions(userValues, formValues, pageState);
        var control = CreateControl(formElement,componentName,field, expOptions, value);

        control.Enabled = FieldVisibilityService.IsEnabled(field, pageState, formValues);

        return control;
    }

    public static bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter & field.Filter.Type == FilterMode.Range;
    }
    

    public JJBaseControl CreateControl(FormElement formElement, string componentName, FormElementField field,ExpressionOptions expressionOptions, object value)
    {
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        JJBaseControl baseView;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                baseView = ComboBoxFactory.CreateComboBox(field, expressionOptions, value);
                break;
            case FormComponent.Search:
                baseView = SearchBoxFactory.CreateSearchBox(field, expressionOptions, value, componentName);
                break;
            case FormComponent.Lookup:
                baseView = LookupFactory.CreateLookup(field, expressionOptions, value, componentName);
                break;
            case FormComponent.CheckBox:
                baseView = CheckBoxFactory.CreateCheckBox(field, value);

                if (expressionOptions.PageState != PageState.List)
                    ((JJCheckBox)baseView).Text = string.IsNullOrEmpty(field.Label) ? field.Name : field.Label;

                break;
            case FormComponent.TextArea:
                baseView = TextAreaFactory.CreateTextArea(field, value);
                break;
            case FormComponent.Slider:
                baseView = SliderFactory.CreateSlider(field, value);
                break;
            case FormComponent.File:
                if (expressionOptions.PageState == PageState.Filter)
                {
                    baseView = TextGroupFactory.CreateTextGroup(field, value);
                }
                else
                {
                    var textFile = TextFileFactory.CreateTextFile(formElement, field, expressionOptions, value, componentName);
                    baseView = textFile;
                }
                break;
            default:
                var textGroup = TextGroupFactory.CreateTextGroup(field,  value);


                if (expressionOptions.PageState == PageState.Filter)
                {
                    textGroup.Actions = textGroup.Actions.Where(a => a.ShowInFilter).ToList();
                }
                else
                {
                    AddUserActions(formElement, componentName, field, expressionOptions, textGroup);
                }

                baseView = textGroup;

                break;
        }
        baseView.ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly && expressionOptions.PageState != PageState.Filter;

        return baseView;
    }

    private void AddUserActions(FormElement formElement, string componentName, FormElementField field,
        ExpressionOptions expressionOptions, JJTextGroup textGroup)
    {
        var actions = field.Actions.GetAllSorted().FindAll(x => x.IsVisible);
        foreach (var action in actions)
        {
            var actionManager = new ActionManager(formElement, ExpressionsService, componentName);
            var link = actionManager.GetLinkField(action, expressionOptions.FormValues, expressionOptions.PageState,
                field.Name);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, expressionOptions.FormValues);
                onRender.Invoke(this, args);
            }

            if (link != null)
                textGroup.Actions.Add(link);
        }
    }
}


