using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Render components fields in a div
/// </summary>
internal class DataPanelControl
{
    public string Name { get; set; }

    public UIForm UISettings { get; private set; }

    public FieldManager FieldManager { get; private set; }

    public PageState PageState { get; private set; }

    public Hashtable Erros { get; private set; }

    public Hashtable Values { get; set; }

    public string FieldNamePrefix { get; set; }

    private bool IsViewModeAsStatic => PageState == PageState.View && UISettings.ShowViewModeAsStatic;


    public DataPanelControl(JJDataPanel dataPanel)
    {
        UISettings = dataPanel.UISettings;
        FieldManager = dataPanel.FieldManager;
        PageState = dataPanel.PageState;
        Erros = dataPanel.Erros;
        Values = dataPanel.Values;
        Name = dataPanel.Name;
    }

    public DataPanelControl(JJGridView gridView)
    {
        UISettings = new UIForm
        {
            IsVerticalLayout = false
        };
        FieldManager = gridView.FieldManager;
        PageState = PageState.Filter;
        Erros = new Hashtable();
        Name = gridView.Name;
    }

    public HtmlBuilder GetHtmlForm(List<FormElementField> fields)
    {
        if (UISettings.IsVerticalLayout)
            return GetHtmlFormVertical(fields);

        return GetHtmlFormHorizontal(fields);
    }

    private HtmlBuilder GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
        int cols = UISettings.FormCols;
        if (cols > 12)
            cols = 12;

        if (cols >= 1)
            colClass = $" col-sm-{(12 / cols)}";

        var html = new HtmlBuilder(HtmlTag.Div);
        int linegroup = int.MinValue;
        HtmlBuilder row = null;
        foreach (var field in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(field, PageState, Values);
            if (!visible)
                continue;

            //value
            object value = null;
            if (Values != null && Values.Contains(field.Name))
                value = FieldManager.FormatVal(field, Values[field.Name]);

            if (linegroup != field.LineGroup)
            {
                linegroup = field.LineGroup;
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

            if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(field.Name))
                htmlField.WithCssClass("has-error");

            if (PageState == PageState.View && UISettings.ShowViewModeAsStatic)
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
        string fldClass = "";
        string labelClass = "";
        string fieldClass = "";
        string fullClass = "";

        int cols = UISettings.FormCols;
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

        if (BootstrapHelper.Version > 3)
        {
            labelClass += " d-flex justify-content-end align-items-center";
            fieldClass += " d-flex justify-content-start align-items-center";
        }

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.FormHorizontal);

        int colCount = 1;
        HtmlBuilder row = null;
        foreach (var f in fields)
        {
            //Visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
                continue;

            //Value
            object value = null;
            if (Values != null && Values.Contains(f.Name))
                value = FieldManager.FormatVal(f, Values[f.Name]);

            var label = new JJLabel(f)
            {
                CssClass = labelClass
            };

            fldClass += string.IsNullOrEmpty(f.CssClass) ? "" : $" {f.CssClass}";
            if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(f.Name))
                fldClass += " has-error";

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

            row.WithCssClass(fldClass)
             .AppendElement(label);

            if (FieldManager.IsRange(f, PageState))
            {
                row.AppendElement(GetControlField(f, value));
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

    private HtmlBuilder GetStaticField(FormElementField f)
    {
        var tag = BootstrapHelper.Version == 3 ? HtmlTag.P : HtmlTag.Span;
        var html = new HtmlBuilder(tag)
            .WithCssClass("form-control-static")
            .AppendText(FieldManager.ParseVal(f, Values));

        return html;
    }

    private HtmlBuilder GetControlField(FormElementField f, object value)
    {
        var field = FieldManager.GetField(f, PageState, Values, value);

        if (!string.IsNullOrEmpty(FieldNamePrefix))
            field.Name = FieldNamePrefix + f.Name;

        field.Enabled = FieldManager.IsEnable(f, PageState, Values);
        if (BootstrapHelper.Version > 3 && Erros != null && Erros.Contains(f.Name))
        {
            field.CssClass = "is-invalid";
        }

        if (f.AutoPostBack & (PageState == PageState.Insert | PageState == PageState.Update))
        {
            field.SetAttr("onchange", GetScriptReload(f));
        }

        if (PageState == PageState.Filter)
        {
            if (field is JJTextGroup textGroup)
            {
                if (f.Filter.Type == FilterMode.MultValuesContain ||
                    f.Filter.Type == FilterMode.MultValuesEqual)
                {
                    textGroup.Attributes.Add("data-role", "tagsinput");
                    textGroup.MaxLength = 0;
                }
            }
            else if (field is JJComboBox comboBox)
            {
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
