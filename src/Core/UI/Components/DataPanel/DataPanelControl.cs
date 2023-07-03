using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render components fields in a div
/// </summary>
internal class DataPanelControl
{
    public string Name { get; set; }

    public FormUI FormUI { get; private set; }

    public FieldManager FieldManager { get; private set; }

    public PageState PageState { get; private set; }

    public IDictionary<string,dynamic>Errors { get; private set; }

    public IDictionary<string,dynamic> UserValues { get; set; }
    public IDictionary<string,dynamic>Values { get; set; }

    public string FieldNamePrefix { get; set; }

    public bool IsExternalRoute { get; }

    private bool IsViewModeAsStatic => PageState == PageState.View && FormUI.ShowViewModeAsStatic;

    private IFieldVisibilityService FieldVisibilityService { get; }
    internal IExpressionsService ExpressionsService { get; }
    private IFieldFormattingService FieldFormattingService { get; }
    
    public DataPanelControl(JJDataPanel dataPanel)
    {
        FormUI = dataPanel.FormUI;
        FieldManager = dataPanel.FieldManager;
        PageState = dataPanel.PageState;
        Errors = dataPanel.Errors;
        Values = dataPanel.Values;
        FieldVisibilityService = dataPanel.FieldVisibilityService;
        UserValues = dataPanel.UserValues;
        Name = dataPanel.Name;
        ExpressionsService = dataPanel.ExpressionsService;
        FieldFormattingService = dataPanel.FieldFormattingService;
        IsExternalRoute = dataPanel.IsExternalRoute;
    }

    public DataPanelControl(JJGridView gridView)
    {
        FormUI = new FormUI
        {
            IsVerticalLayout = false
        };
        FieldManager = gridView.FieldManager;
        PageState = PageState.Filter;
        Errors = new Dictionary<string, dynamic>();
        UserValues = gridView.UserValues;
        Name = gridView.Name;
        ExpressionsService = gridView.ExpressionsService;
        FieldFormattingService = gridView.FieldFormattingService;
        IsExternalRoute = gridView.IsExternalRoute;
        FieldVisibilityService = gridView.FieldVisibilityService;
    }

    public HtmlBuilder GetHtmlForm(List<FormElementField> fields)
    {
        if (FormUI.IsVerticalLayout)
            return GetHtmlFormVertical(fields);

        return GetHtmlFormHorizontal(fields);
    }

    private HtmlBuilder GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
        int cols = FormUI.FormCols;
        if (cols > 12)
            cols = 12;

        if (cols >= 1)
            colClass = $" col-sm-{12 / cols}";

        var html = new HtmlBuilder(HtmlTag.Div);
        int lineGroup = int.MinValue;
        HtmlBuilder row = null;
        foreach (var field in fields)
        {
            bool visible = FieldVisibilityService.IsVisible(field, PageState, Values);
            if (!visible)
                continue;
            
            object value = null;
            if (Values != null && Values.ContainsKey(field.Name))
            {
                if (field.Component != FormComponent.Currency)
                {
                    value = FieldFormattingService.FormatValue(field, Values[field.Name]);
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
                html.AppendElement(row);
            }

            var htmlField = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.FormGroup);

            row?.AppendElement(htmlField);

            string fieldClass;
            if (FieldManager.IsRange(field, PageState))
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
                htmlField.AppendElement(new JJLabel(field));

            if (IsViewModeAsStatic)
                htmlField.AppendElement(GetStaticField(field));
            else
                htmlField.AppendElement(GetControlField(field, value));
        }

        return html;
    }

    private HtmlBuilder GetHtmlFormHorizontal(List<FormElementField> fields)
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
        HtmlBuilder row = null;
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
            bool visible = FieldVisibilityService.IsVisible(f, PageState, Values);
            if (!visible)
                continue;

            //Value
            object value = null;
            if (Values != null && Values.TryGetValue(f.Name, out var nonFormattedValue))
                value = FieldFormattingService.FormatValue(f, nonFormattedValue);

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

                html.AppendElement(row);
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
             .AppendElement(label);

            if (FieldManager.IsRange(f, PageState))
            {
                row?.AppendElement(GetControlField(f, value));
            }
            else
            {
                row.AppendElement(HtmlTag.Div, col =>
                {
                    col.WithCssClass(colClass);
                    col.AppendElement(IsViewModeAsStatic ? GetStaticField(f) : GetControlField(f, value));
                });
            }

        }

        return html;
    }

    [Obsolete("Must be async")]
    private HtmlBuilder GetStaticField(FormElementField f)
    {
        var tag = BootstrapHelper.Version == 3 ? HtmlTag.P : HtmlTag.Span;
        var html = new HtmlBuilder(tag)
            .WithCssClass("form-control-static")
            .AppendText(FieldFormattingService.FormatGridValue(f, Values,UserValues).GetAwaiter().GetResult());

        return html;
    }

    private HtmlBuilder GetControlField(FormElementField f, object value)
    {
        var field = FieldManager.GetField(f, PageState, Values,UserValues, value);
        field.IsExternalRoute = IsExternalRoute;

        if (!string.IsNullOrEmpty(FieldNamePrefix))
            field.Name = FieldNamePrefix + f.Name;

        field.Enabled = FieldVisibilityService.IsEnabled(f, PageState, Values);
        if (BootstrapHelper.Version > 3 && Errors != null && Errors.ContainsKey(f.Name))
        {
            field.CssClass = "is-invalid";
        }

        if (f.AutoPostBack && PageState is PageState.Insert or PageState.Update)
        {
            field.SetAttr("onchange", GetScriptReload(f));
        }

        if (PageState == PageState.Filter)
        {
            if (field is JJTextGroup textGroup)
            {
                if (f.Filter.Type is FilterMode.MultValuesContain or FilterMode.MultValuesEqual)
                {
                    textGroup.Attributes.Add("data-role", "tagsinput");
                    textGroup.MaxLength = 0;
                }
            }
            else if (field is JJComboBox comboBox)
            {
                if (f.Filter.IsRequired || f.Filter.Type is FilterMode.MultValuesEqual or FilterMode.MultValuesContain)
                    comboBox.DataItem.FirstOption = FirstOptionMode.None;
                else
                    comboBox.DataItem.FirstOption = FirstOptionMode.All;

                if (f.Filter.Type == FilterMode.MultValuesEqual)
                    comboBox.MultiSelect = true;
            }
        }

        return field.RenderHtml();
    }

    private string GetScriptReload(FormElementField f)
    {
        //WorkArroud to trigger event on search component
        if (f.Component == FormComponent.Search)
        {
            var script = new StringBuilder();
            script.Append("setTimeout(function() { ");
            script.Append($"JJDataPanel.doReload('{Name}','{f.Name}'); ");
            script.Append("}, 200);");
            return script.ToString();
        }

        return $"JJDataPanel.doReload('{Name}','{f.Name}');";
    }

}
