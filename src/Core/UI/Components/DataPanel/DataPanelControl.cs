using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render components fields in a div
/// </summary>
internal class DataPanelControl
{

    private DataPanelScripts _panelScripts;


    public string Name { get; }
    public FormElement FormElement { get; }
    public FormUI FormUI { get; private set; }

    public ControlsFactory ControlFactory { get; }

    public PageState PageState { get; private set; }

    public IDictionary<string, dynamic> Errors { get; private set; }

    public IDictionary<string, dynamic> UserValues { get; set; }
    public IDictionary<string, dynamic> Values { get; set; }

    public string FieldNamePrefix { get; set; }

    public bool IsExternalRoute { get; }

    private bool IsViewModeAsStatic => PageState == PageState.View && FormUI.ShowViewModeAsStatic;
    internal IExpressionsService ExpressionsService { get; }
    private IFieldsService FieldsService { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal DataPanelScripts Scripts => _panelScripts ??= new DataPanelScripts(EncryptionService, UrlHelper);
    public DataPanelControl(JJDataPanel dataPanel)
    {
        FormElement = dataPanel.FormElement;
        FormUI = dataPanel.FormUI;
        ControlFactory = dataPanel.ControlsFactory;
        PageState = dataPanel.PageState;
        Errors = dataPanel.Errors;
        Values = dataPanel.Values;
        EncryptionService = dataPanel.EncryptionService;
        UrlHelper = dataPanel.UrlHelper;
        UserValues = dataPanel.UserValues;
        FieldsService = dataPanel.FieldsService;
        Name = dataPanel.Name;
        ExpressionsService = dataPanel.ExpressionsService;
        IsExternalRoute = dataPanel.IsExternalRoute;
    }

    public DataPanelControl(JJGridView gridView)
    {
        FormElement = gridView.FormElement;
        FormUI = new FormUI
        {
            IsVerticalLayout = false
        };
        PageState = PageState.Filter;
        EncryptionService = gridView.EncryptionService;
        UrlHelper = gridView.UrlHelper;
        Errors = new Dictionary<string, dynamic>();
        UserValues = gridView.UserValues;
        Name = gridView.Name;
        ControlFactory = gridView.ControlsFactory;
        ExpressionsService = gridView.ExpressionsService;
        FieldsService = gridView.FieldsService;
        IsExternalRoute = gridView.IsExternalRoute;
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
            bool visible = FieldsService.IsVisible(field, PageState, Values);
            if (!visible)
                continue;

            object value = null;
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
                html.AppendElement(row);
            }

            var htmlField = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.FormGroup);

            row?.AppendElement(htmlField);

            string fieldClass;
            if (ControlsFactory.IsRange(field, PageState))
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
                htmlField.AppendElement(GetStaticField(field).GetAwaiter().GetResult());
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
            bool visible = FieldsService.IsVisible(f, PageState, Values);
            if (!visible)
                continue;

            //Value
            object value = null;
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

            if (ControlsFactory.IsRange(f, PageState))
            {
                row?.AppendElement(GetControlField(f, value));
            }
            else
            {
                row.AppendElement(HtmlTag.Div, col =>
                {
                    col.WithCssClass(colClass);
                    col.AppendElement(IsViewModeAsStatic ? GetStaticField(f).GetAwaiter().GetResult() : GetControlField(f, value));
                });
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

    private HtmlBuilder GetControlField(FormElementField field, object value)
    {
        var control = ControlFactory.CreateControl(FormElement, Name, field, PageState, Values, UserValues, value);
        control.IsExternalRoute = IsExternalRoute;

        if (!string.IsNullOrEmpty(FieldNamePrefix))
            control.Name = FieldNamePrefix + field.Name;

        control.Enabled = FieldsService.IsEnabled(field, PageState, Values);
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

        return control.RenderHtml();
    }

    private string GetScriptReload(FormElementField f)
    {
        //WorkArroud to trigger event on search component
        if (f.Component == FormComponent.Search)
        {
            var script = new StringBuilder();
            script.Append("setTimeout(function() { ");
            script.Append(Scripts.GetReloadPanelScript(FormElement.Name, f.Name, Name, IsExternalRoute));
            script.Append("}, 200);");
            return script.ToString();
        }

        return Scripts.GetReloadPanelScript(FormElement.Name, f.Name, Name, IsExternalRoute);
    }

}
