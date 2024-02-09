#nullable enable

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components.TextRange;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Render components fields in a div
/// </summary>
internal class DataPanelControl
{

    private DataPanelScripts? _panelScripts;
    private bool _isViewModeAsStatic => PageState == PageState.View && FormUI.ShowViewModeAsStatic;


    public string ParentComponentName { get; }

    public string Name { get; }

    public FormElement FormElement { get; }

    public FormUI FormUI { get; }

    public IComponentFactory ComponentFactory { get; }
    
    public Dictionary<string, string> Errors { get; }

    public PageState PageState => FormStateData.PageState;

    public Dictionary<string, object?>? UserValues => FormStateData.UserValues;

    public Dictionary<string, object?> Values => FormStateData.Values;

    public FormStateData FormStateData { get; }

    public string? FieldNamePrefix { get; init; }


    internal FieldsService FieldsService { get; }
    internal ExpressionsService ExpressionsService { get; }
    internal IEncryptionService EncryptionService { get; }
    internal DataPanelScripts Scripts => _panelScripts ??= new DataPanelScripts(this);

    public DataPanelControl(JJDataPanel dataPanel)
    {
        FormElement = dataPanel.FormElement;
        ParentComponentName = dataPanel.ParentComponentName ?? dataPanel.Name;
        FormUI = dataPanel.FormUI;
        ComponentFactory = dataPanel.ComponentFactory;
        Errors = dataPanel.Errors;
        EncryptionService = dataPanel.EncryptionService;
        FieldsService = dataPanel.FieldsService;
        Name = dataPanel.Name;
        ExpressionsService = dataPanel.ExpressionsService;
        FieldNamePrefix = dataPanel.FieldNamePrefix;
        FormStateData = new FormStateData(dataPanel.Values, dataPanel.UserValues, dataPanel.PageState);
    }

    public DataPanelControl(JJGridView gridView, Dictionary<string, object?> values)
    {
        ParentComponentName = gridView.ParentComponentName ?? gridView.Name;
        FormElement = gridView.FormElement;
        FormUI = new FormUI
        {
            IsVerticalLayout = gridView.FormElement.Options.Grid.UseVerticalLayoutAtFilter
        };
        EncryptionService = gridView.EncryptionService;
        Errors = new Dictionary<string, string>();
        Name = gridView.Name;
        ComponentFactory = gridView.ComponentFactory;
        ExpressionsService = gridView.ExpressionsService;
        FieldsService = gridView.FieldsService;
        FormStateData = new FormStateData(values, gridView.UserValues, PageState.Filter);
    }

    public Task<HtmlBuilder> GetHtmlForm(List<FormElementField> fields)
    {
        if (FormUI.IsVerticalLayout)
            return GetHtmlFormVertical(fields);

        return GetHtmlFormHorizontal(fields);
    }

    private async Task<HtmlBuilder> GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
        int cols = FormUI.FormCols;
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
            bool visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
            if (!visible)
                continue;

            object? value = null;
            if (Values != null && Values.ContainsKey(field.Name))
                value = FieldsService.FormatValue(field, Values[field.Name]);

            if (lineGroup != field.LineGroup)
            {
                lineGroup = field.LineGroup;
                row = new HtmlBuilder(HtmlTag.Div)
                    .WithCssClass("row");
                html.Append(row);
            }

            var htmlField = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.FormGroup);

            
            row?.Append(htmlField);

            string fieldClass;
            
            if (field.CssClass != null && !string.IsNullOrEmpty(field.CssClass))
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
            htmlField.WithCssClass(fieldClass);

            if (BootstrapHelper.Version == 3 && Errors != null && Errors.ContainsKey(field.Name))
                htmlField.WithCssClass("has-error");

            if (PageState == PageState.View && FormUI.ShowViewModeAsStatic)
                htmlField.WithCssClass("jjborder-static");

            if (field.Component is not FormComponent.CheckBox)
            {
                var label = CreateLabel(field, IsRange(field, PageState));
                htmlField.AppendComponent(label);
            }
                
            if (_isViewModeAsStatic)
                htmlField.Append(await GetStaticField(field));
            else
                htmlField.Append(await GetControlFieldHtml(field, value));
        }

        return html;
    }

   

    private async Task<HtmlBuilder> GetHtmlFormHorizontal(List<FormElementField> fields)
    {
        string labelClass = "";
        string? fieldClass = "";
        string fullClass = "";

        int cols = FormUI.FormCols;
        if (cols == 1)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-9";
            fullClass = "col-sm-9";
        }
        else if (cols == 2)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-4";
            fullClass = "col-sm-9";
        }
        else if (cols == 3)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-2";
            fullClass = "col-sm-9";
        }
        else if (cols >= 4)
        {
            cols = 4;
            labelClass = "col-sm-1";
            fieldClass = "col-sm-2";
            fullClass = "col-sm-8";
        }


        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.FormHorizontal);

        int colCount = 1;
        HtmlBuilder? row = null;
        var formData = new FormStateData(Values, UserValues, PageState);
        foreach (var f in fields)
        {
            if (!string.IsNullOrEmpty(f.CssClass))
                fieldClass = f.CssClass;

            if (BootstrapHelper.Version > 3)
            {
                labelClass += " d-flex justify-content-end align-items-center";
                fieldClass += " d-flex justify-content-start align-items-center";
            }

            //Visible expression
            bool visible = ExpressionsService.GetBoolValue(f.VisibleExpression, formData);
            if (!visible)
                continue;

            //Value
            object? value = null;
            if (Values != null && Values.TryGetValue(f.Name, out var nonFormattedValue))
                value = FieldsService.FormatValue(f, nonFormattedValue);
            
            var label = CreateLabel(f, IsRange(f, PageState));
            label.CssClass = labelClass;
            
            var cssClass = string.Empty;
            if (BootstrapHelper.Version == 3 && Errors != null && Errors.ContainsKey(f.Name))
                cssClass += " has-error";

            if (colCount == 1 || colCount >= cols)
            {
                colCount = 1;
                row = new HtmlBuilder(HtmlTag.Div)
                    .WithCssClass("form-group")
                    .WithCssClassIf(BootstrapHelper.Version > 3, "row mb-3");

                html.Append(row);
            }

            string? colClass = fieldClass;
            if (f.Component == FormComponent.TextArea)
            {
                colCount = 1;
                colClass = fullClass;
            }
            else if (f.Component == FormComponent.CheckBox)
            {
                colCount = 1;
                if (!_isViewModeAsStatic)
                    label.Text = string.Empty;
            }
            else
            {
                colCount++;
            }

            row?.WithCssClass(cssClass)
             .AppendComponent(label);

   
            await row?.AppendAsync(HtmlTag.Div, async col =>
            {
                col.WithCssClass(colClass);
                col.Append(_isViewModeAsStatic ? await GetStaticField(f) : await GetControlFieldHtml(f, value));
            })!;
            
        }

        return html;
    }

    private static bool IsRange(ElementField field, PageState pageState)
    {
        return pageState == PageState.Filter && field.Filter.Type == FilterMode.Range;
    }

    private JJLabel CreateLabel(FormElementField field, bool isRange)
    {
        var label = ComponentFactory.Html.Label.Create(field);
        label.LabelFor = FieldNamePrefix + field.Name;

        if (_isViewModeAsStatic)
            label.LabelFor = null;
        else if (isRange)
            label.LabelFor += "_from";

        return label;
    }

    private async Task<HtmlBuilder> GetStaticField(FormElementField f)
    {
        var html = new HtmlBuilder(HtmlTag.P)
            .WithCssClass("form-control-static")
            .AppendText(await FieldsService.FormatGridValueAsync(f, Values, UserValues));

        return html;
    }

    private Task<HtmlBuilder> GetControlFieldHtml(FormElementField field, object? value)
    {
        var formStateData = new FormStateData(Values, UserValues, PageState);
        var control = ComponentFactory.Controls.Create(FormElement, field, formStateData, ParentComponentName, value);

        if (!string.IsNullOrEmpty(FieldNamePrefix))
            control.Name = FieldNamePrefix + field.Name;

        control.Enabled = ExpressionsService.GetBoolValue(field.EnableExpression, formStateData);

        if (BootstrapHelper.Version > 3 && Errors.ContainsKey(field.Name))
            control.CssClass = "is-invalid";

        if (field.AutoPostBack && PageState is PageState.Insert or PageState.Update)
            control.SetAttr("onchange", GetScriptReload(field));

        if(control is JJTextGroup textGroup && PageState is PageState.View)
            foreach (var textGroupAction in textGroup.Actions)
                textGroupAction.Enabled = false;
        
        if(control is JJTextFile file)
            file.ParentName = FormElement.Name;
        
        if (PageState != PageState.Filter) 
            return control.GetHtmlBuilderAsync();
        
        switch (control)
        {
            case JJTextRange range:
                range.IsVerticalLayout = FormUI.IsVerticalLayout;
                break;
            case JJTextGroup when field.Filter.Type is not (FilterMode.MultValuesContain or FilterMode.MultValuesEqual):
                return control.GetHtmlBuilderAsync();
            case JJTextGroup:
                control.Attributes.Add("data-role", "tagsinput");
                control.MaxLength = 0;
                break;
            case JJComboBox comboBox:
            {
                if (field.Filter.IsRequired || field.Filter.Type is FilterMode.MultValuesEqual or FilterMode.MultValuesContain)
                    comboBox.DataItem.FirstOption = FirstOptionMode.None;
                else
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
        var reloadPanelScript = Scripts.GetReloadPanelScript(field.Name);
        
        //Workaround to trigger event on search component
        if (field.Component is not FormComponent.Search) 
            return reloadPanelScript;
        
        var script = new StringBuilder();
        script.Append("setTimeout(function() { ");
        script.Append(reloadPanelScript);
        script.Append("}, 200);");
        return script.ToString();

    }

}
