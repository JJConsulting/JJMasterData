#nullable enable
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.UI.Components.Controls;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render components fields in a div
/// </summary>
internal class DataPanelControl
{

    private DataPanelScripts? _panelScripts;

    public string Name { get; }

    public FormElement FormElement { get; }

    public FormUI FormUI { get; private set; }

    public ControlFactory ControlFactory { get; }
    
    public IDictionary<string, object> Errors { get; private set; }

    public PageState PageState => FormState.PageState;

    public IDictionary<string, object>? UserValues => FormState.UserValues;

    public IDictionary<string, object> Values => FormState.FormValues;

    public FormStateData FormState { get; set; }

    public string? FieldNamePrefix { get; set; }

    public bool IsExternalRoute { get; }

    private bool IsViewModeAsStatic => PageState == PageState.View && FormUI.ShowViewModeAsStatic;
    internal IExpressionsService ExpressionsService { get; }
    private IFieldsService FieldsService { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal DataPanelScripts Scripts => _panelScripts ??= new DataPanelScripts(this);

    public DataPanelControl(JJDataPanel dataPanel)
    {
        FormElement = dataPanel.FormElement;
        FormUI = dataPanel.FormUI;
        ControlFactory = dataPanel.ComponentFactory.Controls;
        Errors = dataPanel.Errors;
        EncryptionService = dataPanel.EncryptionService;
        UrlHelper = dataPanel.UrlHelper;
        FieldsService = dataPanel.FieldsService;
        Name = dataPanel.Name;
        ExpressionsService = dataPanel.ExpressionsService;
        IsExternalRoute = dataPanel.IsExternalRoute;
        FieldNamePrefix = dataPanel.FieldNamePrefix;
        FormState = new FormStateData(dataPanel.Values, dataPanel.UserValues, dataPanel.PageState);
    }

    public DataPanelControl(JJGridView gridView, IDictionary<string, object> values)
    {
        FormElement = gridView.FormElement;
        FormUI = new FormUI
        {
            IsVerticalLayout = false
        };
        EncryptionService = gridView.EncryptionService;
        UrlHelper = gridView.UrlHelper;
        Errors = new Dictionary<string, object>();
        Name = gridView.Name;
        ControlFactory = gridView.ComponentFactory.Controls;
        ExpressionsService = gridView.ExpressionsService;
        FieldsService = gridView.FieldsService;
        IsExternalRoute = gridView.IsExternalRoute;
        FormState = new FormStateData(values, gridView.UserValues, PageState.Filter);
    }

    public async Task<HtmlBuilder> GetHtmlForm(List<FormElementField> fields)
    {
        if (FormUI.IsVerticalLayout)
            return await GetHtmlFormVertical(fields);

        return await GetHtmlFormHorizontal(fields);
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
            bool visible = await ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData);
            if (!visible)
                continue;

            object? value = null;
            if (Values != null && Values.ContainsKey(field.Name))
            {
                if (field.Component != FormComponent.Currency)
                {
                    value = FieldsService.FormatValue(field, Values[field.Name]);
                }
                else
                {
                    value = Values[field.Name];
                }
            }


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
            if (ControlFactory.IsRange(field, PageState))
            {
                fieldClass = string.Empty;
            }
            else if (!string.IsNullOrEmpty(field.CssClass))
            {
                fieldClass = field.CssClass;
            }
            else
            {
                if (field.Component == FormComponent.TextArea | field.Component == FormComponent.CheckBox)
                    fieldClass = "col-sm-12";
                else
                    fieldClass = colClass;
            }
            htmlField.WithCssClass(fieldClass);

            if (BootstrapHelper.Version == 3 && Errors != null && Errors.ContainsKey(field.Name))
                htmlField.WithCssClass("has-error");

            if (PageState == PageState.View && FormUI.ShowViewModeAsStatic)
                htmlField.WithCssClass("jjborder-static");

            if (field.Component != FormComponent.CheckBox)
                htmlField.AppendComponent(new JJLabel(field));

            if (IsViewModeAsStatic)
                htmlField.Append(await GetStaticField(field));
            else
                htmlField.Append(await GetControlField(field, value));
        }

        return html;
    }

    private async Task<HtmlBuilder> GetHtmlFormHorizontal(List<FormElementField> fields)
    {
        string labelClass = "";
        string fieldClass = "";
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
            bool visible = await ExpressionsService.GetBoolValueAsync(f.VisibleExpression, formData);
            if (!visible)
                continue;

            //Value
            object? value = null;
            if (Values != null && Values.TryGetValue(f.Name, out var nonFormattedValue))
                value = FieldsService.FormatValue(f, nonFormattedValue);

            var label = new JJLabel(f)
            {
                CssClass = labelClass
            };

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

            string colClass = fieldClass;
            if (f.Component == FormComponent.TextArea)
            {
                colCount = 1;
                colClass = fullClass;
            }
            else if (f.Component == FormComponent.CheckBox)
            {
                colCount = 1;
                if (!IsViewModeAsStatic)
                    label.Text = string.Empty;
            }
            else
            {
                colCount++;
            }

            row?.WithCssClass(cssClass)
             .AppendComponent(label);

            if (ControlFactory.IsRange(f, PageState))
            {
                row?.Append(await GetControlField(f, value));
            }
            else
            {
                await row?.AppendAsync(HtmlTag.Div, async col =>
                {
                    col.WithCssClass(colClass);
                    col.Append(IsViewModeAsStatic ? await GetStaticField(f) : await GetControlField(f, value));
                })!;
            }

        }

        return html;
    }


    private async Task<HtmlBuilder> GetStaticField(FormElementField f)
    {
        var tag = BootstrapHelper.Version == 3 ? HtmlTag.P : HtmlTag.Span;
        var html = new HtmlBuilder(tag)
            .WithCssClass("form-control-static")
            .AppendText(await FieldsService.FormatGridValueAsync(f, Values, UserValues));

        return html;
    }

    private async Task<HtmlBuilder> GetControlField(FormElementField field, object? value)
    {
        var control = await ControlFactory.CreateAsync(FormElement, field, Values, UserValues, PageState, Name, value);
        control.IsExternalRoute = IsExternalRoute;

        if (!string.IsNullOrEmpty(FieldNamePrefix))
            control.Name = FieldNamePrefix + field.Name;

        var formData = new FormStateData(Values, UserValues, PageState);
        control.Enabled = await ExpressionsService.GetBoolValueAsync(field.EnableExpression, formData);

        if (BootstrapHelper.Version > 3 && Errors != null && Errors.ContainsKey(field.Name))
        {
            control.CssClass = "is-invalid";
        }

        if (field.AutoPostBack && PageState is PageState.Insert or PageState.Update)
        {
            control.SetAttr("onchange", GetScriptReload(field));
        }

        if (PageState == PageState.Filter)
        {
            if (control is JJTextGroup textGroup)
            {
                if (field.Filter.Type is FilterMode.MultValuesContain or FilterMode.MultValuesEqual)
                {
                    textGroup.Attributes.Add("data-role", "tagsinput");
                    textGroup.MaxLength = 0;
                }
            }
            else if (control is JJComboBox comboBox)
            {
                if (field.Filter.IsRequired || field.Filter.Type is FilterMode.MultValuesEqual or FilterMode.MultValuesContain)
                    comboBox.DataItem.FirstOption = FirstOptionMode.None;
                else
                    comboBox.DataItem.FirstOption = FirstOptionMode.All;

                if (field.Filter.Type == FilterMode.MultValuesEqual)
                    comboBox.MultiSelect = true;
            }
        }

        switch (control)
        {
            case HtmlControl htmlControl:
                return htmlControl.GetHtmlBuilder();
            case AsyncControl asyncControl:
            {
                var result = await asyncControl.GetResultAsync();
                if (result is RenderedComponentResult renderedComponentResult)
                    return renderedComponentResult.HtmlBuilder;
                break;
            }
        }

        return new HtmlBuilder();
    }

    private string GetScriptReload(FormElementField field)
    {

        var reloadPanelScript = Scripts.GetReloadPanelScript(field.Name);
        
        //Workarround to trigger event on search component
        if (field.Component != FormComponent.Search) 
            return reloadPanelScript;
        
        var script = new StringBuilder();
        script.Append("setTimeout(function() { ");
        script.Append(reloadPanelScript);
        script.Append("}, 200);");
        return script.ToString();

    }

}
