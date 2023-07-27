using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Web.Factories;

public class ControlsFactory
{
    private readonly IServiceProvider _provider;
    
    public CheckBoxFactory CheckBox => _provider.GetRequiredService<CheckBoxFactory>();
    public ComboBoxFactory ComboBox => _provider.GetRequiredService<ComboBoxFactory>();
    public LookupFactory Lookup => _provider.GetRequiredService<LookupFactory>();
    public SearchBoxFactory SearchBox => _provider.GetRequiredService<SearchBoxFactory>();
    public SliderFactory Slider => _provider.GetRequiredService<SliderFactory>();
    public TextAreaFactory TextArea => _provider.GetRequiredService<TextAreaFactory>();
    public TextBoxFactory TextBox => _provider.GetRequiredService<TextBoxFactory>();
    public TextFileFactory TextFile => _provider.GetRequiredService<TextFileFactory>();
    public TextRangeFactory TextRange => _provider.GetRequiredService<TextRangeFactory>();
    
    internal FileDownloaderFactory FileDownloader => _provider.GetRequiredService<FileDownloaderFactory>();
    
    public ControlsFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public JJBaseControl CreateControl(
        FormElement formElement,
        string componentName,
        FormElementField field,
        PageState pageState,
        IDictionary<string, dynamic> formValues,
        IDictionary<string, dynamic> userValues,
        object value = null)
    {
        if (pageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            return TextRange.CreateTextRange(field, formValues);
        }

        var expOptions = new ExpressionOptions(userValues, formValues, pageState);
        var control = CreateControl(formElement, componentName, field, expOptions, value);
        var fieldVisibilityService = _provider.GetRequiredService<IFieldVisibilityService>();
        control.Enabled = fieldVisibilityService.IsEnabled(field, pageState, formValues);

        return control;
    }

    public static bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter & field.Filter.Type == FilterMode.Range;
    }


    public JJBaseControl CreateControl(FormElement formElement, string componentName, FormElementField field, ExpressionOptions expressionOptions, object value)
    {
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        JJBaseControl baseView;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                baseView = ComboBox.CreateComboBox(field, expressionOptions, value);
                break;
            case FormComponent.Search:
                baseView = SearchBox.CreateSearchBox(field, expressionOptions, value, componentName);
                break;
            case FormComponent.Lookup:
                baseView = Lookup.CreateLookup(field, expressionOptions, value, componentName);
                break;
            case FormComponent.CheckBox:
                baseView = CheckBox.CreateCheckBox(field, value);

                if (expressionOptions.PageState != PageState.List)
                    ((JJCheckBox)baseView).Text = string.IsNullOrEmpty(field.Label) ? field.Name : field.Label;

                break;
            case FormComponent.TextArea:
                baseView = TextArea.CreateTextArea(field, value);
                break;
            case FormComponent.Slider:
                baseView = Slider.CreateSlider(field, value);
                break;
            case FormComponent.File:
                if (expressionOptions.PageState == PageState.Filter)
                {
                    baseView = TextBox.CreateText(field, value);
                }
                else
                {
                    var textFile = TextFile.CreateTextFile(formElement, field, expressionOptions, value, componentName);
                    baseView = textFile;
                }
                break;
            default:
                var textGroup = TextBox.CreateTextGroup(formElement, field, expressionOptions, value, componentName);
                baseView = textGroup;
                break;
        }
        
        baseView.ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly && expressionOptions.PageState != PageState.Filter;

        return baseView;
    }

    
}


