#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Html;
using JJMasterData.Core.UI.Components.TextRange;

using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Render components fields in a div
/// </summary>
internal sealed class DataPanelForm
{
    private readonly FieldFormattingService _fieldFormattingService;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;
    private readonly IComponentFactory _componentFactory;

    private readonly Dictionary<string, string> _errors;
    private readonly string _parentComponentName;
    private readonly FormUI _formUI;
    private readonly bool _isGridViewFilter;
    
    private DataPanelScripts? _panelScripts;
    
    private bool IsViewModeAsStatic => PageState == PageState.View && _formUI.ShowViewModeAsStatic;
    private PageState PageState => FormStateData.PageState;
    private Dictionary<string, object?>? UserValues => FormStateData.UserValues;
    private Dictionary<string, object?> Values => FormStateData.Values;
    private DataPanelScripts Scripts => _panelScripts ??= new DataPanelScripts(this);
    
    internal ExpressionsService ExpressionsService { get; }
    internal IEncryptionService EncryptionService { get; }
    
    public string Name { get; }
    public FormElement FormElement { get; }
    public FormStateData FormStateData { get; }
    public string? FieldNamePrefix { get; init; }
    
    public DataPanelForm(JJDataPanel dataPanel)
    {
        _parentComponentName = dataPanel.ParentComponentName ?? dataPanel.Name;
        _componentFactory = dataPanel.ComponentFactory;
        _errors = dataPanel.Errors;
        _fieldFormattingService = dataPanel.FieldFormattingService;
        _stringLocalizer = dataPanel.StringLocalizer;
        _formUI = dataPanel.FormUI;
        _isGridViewFilter = false;
        
        EncryptionService = dataPanel.EncryptionService;
        ExpressionsService = dataPanel.ExpressionsService;
        FieldNamePrefix = dataPanel.FieldNamePrefix;
        FormElement = dataPanel.FormElement;

        Name = dataPanel.Name;
        FormStateData = new FormStateData(dataPanel.Values, dataPanel.UserValues, dataPanel.PageState);
    }

    public DataPanelForm(JJGridView gridView, Dictionary<string, object?> values)
    {
        _parentComponentName = gridView.ParentComponentName ?? gridView.Name;
        _componentFactory = gridView.ComponentFactory;
        _errors = new Dictionary<string, string>();
        _fieldFormattingService = gridView.FieldFormattingService;
        _stringLocalizer = gridView.StringLocalizer;
        _formUI = new FormUI
        {
            IsVerticalLayout = gridView.FormElement.Options.Grid.UseVerticalLayoutAtFilter
        };
        _isGridViewFilter = true;
        
        EncryptionService = gridView.EncryptionService;
        ExpressionsService = gridView.ExpressionsService;
        FormElement = gridView.FormElement;
        Name = gridView.Name;
        FormStateData = new FormStateData(values, gridView.UserValues, PageState.Filter);
    }
    
    public Task<HtmlBuilder> GetHtmlForm(List<FormElementField> fields)
    {
        if (_formUI.IsVerticalLayout)
            return GetHtmlFormVertical(fields);

        return GetHtmlFormHorizontal(fields);
    }

    private async Task<HtmlBuilder> GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
        int cols = _formUI.FormCols;
        if (cols > 12)
            cols = 12;

        if (cols >= 1)
            colClass = $" col-sm-{12 / cols}";

        var html = new HtmlBuilder(HtmlTag.Div);
        int lineGroup = int.MinValue;
        HtmlBuilder? row = null;
        var formData = new FormStateData(Values, UserValues, PageState);
        
        foreach (var field in fields)
        {
            var visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
            if (!visible)
                continue;
            
            object? value = null;
            if (Values != null && Values.TryGetValue(field.Name, out value))
                value = FieldFormattingService.FormatValue(field, value);

            if (lineGroup != field.LineGroup)
            {
                lineGroup = field.LineGroup;
                row = new HtmlBuilder(HtmlTag.Div).WithCssClass("row");
                html.Append(row);
            }

            var formGroup = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.FormGroup);
            
            row?.Append(formGroup);

            string? fieldClass;
            
            if (!string.IsNullOrEmpty(field.CssClass))
            {
                fieldClass = field.CssClass;
            }
            else
            {
                if (field.Component is FormComponent.TextArea or FormComponent.CheckBox)
                    fieldClass = "col-sm-12";
                else
                    fieldClass = colClass;
            }
            formGroup.WithCssClass(fieldClass);

            if (BootstrapHelper.Version == 3 && _errors != null && _errors.ContainsKey(field.Name))
                formGroup.WithCssClass("has-error");

            if (PageState == PageState.View && _formUI.ShowViewModeAsStatic)
                formGroup.WithCssClass("jjborder-static");

            var useFloatingLabel = FormElement.Options.UseFloatingLabels && field.SupportsFloatingLabel();
            
            if (field.Component is not FormComponent.CheckBox && !useFloatingLabel)
            {
                var label = CreateLabel(field, IsRange(field, PageState));
                formGroup.AppendComponent(label);
            }
            
            if (IsViewModeAsStatic)
                formGroup.Append(await GetStaticField(field));
            else
            {
                var controlHtml = await GetControlFieldHtml(field, value);
                formGroup.Append(controlHtml);
            }
        }

        return html;
    }
    
    private async Task<HtmlBuilder> GetHtmlFormHorizontal(List<FormElementField> fields)
    {
        var cols = _formUI.FormCols;
        if (cols >= 4)
            cols = 4;
        
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.FormHorizontal);

        int colCount = 1;
        HtmlBuilder? row = null;
        var formData = new FormStateData(Values, UserValues, PageState);
        foreach (var field in fields)
        {
            var labelClass = "col-sm-2";
            var fieldClass = GetHorizontalFieldClass(cols);
            var hasCssClass = !string.IsNullOrEmpty(field.CssClass);
            
            //Visible expression
            bool visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
            if (!visible)
                continue;

            //Value
            object? value = null;
            if (Values != null && Values.TryGetValue(field.Name, out var nonFormattedValue))
                value = FieldFormattingService.FormatValue(field, nonFormattedValue);

            var isRange = IsRange(field, PageState);
            var label = CreateLabel(field, isRange);
            var cssClass = string.Empty;
            
            if (BootstrapHelper.Version == 3 && _errors != null && _errors.ContainsKey(field.Name))
                cssClass += " has-error";

            if (colCount == 1 || colCount >= cols)
            {
                colCount = 1;
                row = new HtmlBuilder(HtmlTag.Div)
                    .WithCssClass("form-group")
                    .WithCssClassIf(BootstrapHelper.Version > 3, "row mb-3");

                html.Append(row);
            }
            
            if (isRange)
            {
                if (field.Component is FormComponent.Date)
                    fieldClass = "col-sm-6";
                else
                    fieldClass = "col-sm-10";
            }
            
            if (hasCssClass)
                fieldClass = field.CssClass;
            
            if (field.Component is FormComponent.TextArea)
            {
                colCount = 1;
                fieldClass = GetHorizontalTextAreaClass(cols);
            }
            else if (field.Component is FormComponent.CheckBox)
            {
                colCount = 1;
                if (!IsViewModeAsStatic)
                    label.Text = string.Empty;
            }
            else
            {
                colCount++;
            }
            
            if (BootstrapHelper.Version > 3)
            {
                labelClass += " d-flex justify-content-end align-items-center";
                fieldClass += " d-flex justify-content-start align-items-center";
            }
            
            label.CssClass = labelClass;
            
            row?.WithCssClass(cssClass)
             .AppendComponent(label);

            var controlHtml =
                IsViewModeAsStatic ? await GetStaticField(field) : await GetControlFieldHtml(field, value);
            
            row?.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass(fieldClass);
                col.Append(controlHtml);
            });
        }

        return html;
    }
   
    private static string GetHorizontalFieldClass(int cols) => cols switch
    {
        //With spaces of 12 subtracting from the label, we consider:
        1 => "col-sm-10",// 10 * 1 = 10
        2 => "col-sm-5", // 5 * 2 = 10
        3 => "col-sm-3", // 3 * 3 = 9 
        4 => "col-sm-2", // 2 * 4 = 8 
        _ => throw new ArgumentException(@"Invalid number of columns", nameof(cols))
    };

    private static string GetHorizontalTextAreaClass(int cols) => cols switch
    {
        1 => "col-sm-9",
        2 => "col-sm-9",
        3 => "col-sm-9",
        4 => "col-sm-8",
        _ => throw new ArgumentException(@"Invalid number of columns", nameof(cols))
    };

    private static bool IsRange(ElementField field, PageState pageState)
    {
        return pageState is PageState.Filter && field.Filter.Type == FilterMode.Range;
    }

    private JJLabel CreateLabel(FormElementField field, bool isRange)
    {
        var label = _componentFactory.Label.Create(field);
        label.LabelFor = GetFieldNameWithPrefix(field);

        if (IsViewModeAsStatic)
            label.LabelFor = string.Empty;
        else if (isRange)
            label.LabelFor += "_from";

        return label;
    }
    
    private async ValueTask<HtmlBuilder> GetStaticField(FormElementField field)
    {
        var fieldSelector = new FormElementFieldSelector(FormElement, field.Name);
        var staticValue = await _fieldFormattingService.FormatGridValueAsync(fieldSelector, FormStateData);
        var html = new HtmlBuilder(HtmlTag.P)
            .WithCssClass("form-control-static")
            .AppendText(staticValue);

        return html;
    }

    private ValueTask<HtmlBuilder> GetControlFieldHtml(FormElementField field, object? value)
    {
        var formStateData = new FormStateData(Values, UserValues, PageState);
        var control = _componentFactory.Controls.Create(FormElement, field, formStateData, _parentComponentName, value);

        if (!string.IsNullOrEmpty(FieldNamePrefix))
            control.Name = GetFieldNameWithPrefix(field);

        control.Enabled = ExpressionsService.GetBoolValue(field.EnableExpression, formStateData);

        if (BootstrapHelper.Version > 3 && _errors.ContainsKey(field.Name))
            control.CssClass = "is-invalid";

        if (field.AutoPostBack && !_isGridViewFilter &&
            PageState is PageState.Insert or PageState.Update or PageState.Filter)
        {
            control.SetAttribute("onchange", GetScriptReload(field));
        }
        
        if(control is JJTextGroup textGroup && PageState is PageState.View)
            foreach (var textGroupAction in textGroup.Actions)
                textGroupAction.Enabled = false;
        
        if(control is JJTextFile file)
            file.ParentName = FormElement.Name;

        var useFloatingLabels = _formUI.IsVerticalLayout && FormElement.Options.UseFloatingLabels;
        
        if (useFloatingLabels && control is IFloatingLabelControl floatingLabelControl)
        {
            floatingLabelControl.FloatingLabel = string.IsNullOrEmpty(field.Label) ? field.Name : _stringLocalizer[field.Label!];
            floatingLabelControl.UseFloatingLabel = field.SupportsFloatingLabel();
        }
        
        if (PageState is not PageState.Filter) 
            return control.GetHtmlBuilderAsync();


        var isMultValues = field.Filter.Type is FilterMode.MultValuesEqual or FilterMode.MultValuesContain;
        switch (control)
        {
            case JJTextRange range:
                range.IsVerticalLayout = _formUI.IsVerticalLayout;
                break;
            case JJTextGroup when !isMultValues:
                break;
            case JJTextGroup:
                control.Attributes.Add("data-role", "tagsinput");
                control.MaxLength = 0;
                break;
            case JJComboBox comboBox:
            {
                if ((field.Filter.IsRequired || isMultValues) && _isGridViewFilter)
                    comboBox.DataItem.FirstOption = FirstOptionMode.None;
                else if (_isGridViewFilter)
                    comboBox.DataItem.FirstOption = FirstOptionMode.All;

                if (field.Filter.Type == FilterMode.MultValuesEqual)
                    comboBox.MultiSelect = true;
                
                break;
            }
        }

        return control.GetHtmlBuilderAsync();
    }

    private string GetScriptReload(FormElementField field)
    {
        var nameWithPrefix = GetFieldNameWithPrefix(field);

        return field.Component switch
        {
            FormComponent.Search => Scripts.GetReloadPanelWithTimeoutScript(field.Name, nameWithPrefix + "_text"),
            FormComponent.CheckBox => Scripts.GetReloadPanelScript(field.Name, nameWithPrefix + "-checkbox"),
            _ =>  Scripts.GetReloadPanelScript(field.Name, nameWithPrefix)
        };
    }
    
    private string GetFieldNameWithPrefix(FormElementField field) => FieldNamePrefix + field.Name;
}
