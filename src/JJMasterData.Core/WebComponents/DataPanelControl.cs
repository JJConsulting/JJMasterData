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

    public Hashtable Values { get; private set; }

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

    public HtmlElement GetHtmlForm(List<FormElementField> fields)
    {
        if (UISettings.IsVerticalLayout)
            return GetHtmlFormVertical(fields);

        return GetHtmlFormHorizontal(fields);
    }

    private HtmlElement GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
        int cols = UISettings.FormCols;
        if (cols > 12)
            cols = 12;

        if (cols >= 1)
            colClass = $" col-sm-{(12 / cols)}";

        var html = new HtmlElement(HtmlTag.Div);
        int linegroup = int.MinValue;
        HtmlElement row = null;
        foreach (var f in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
                continue;

            //value
            object value = null;
            if (Values != null && Values.Contains(f.Name))
                value = FieldManager.FormatVal(Values[f.Name], f);

            if (linegroup != f.LineGroup)
            {
                linegroup = f.LineGroup;
                row = new HtmlElement(HtmlTag.Div)
                    .WithCssClass("row");
                html.AppendElement(row);
            }

            var htmlField = new HtmlElement(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.FormGroup);

            row.AppendElement(htmlField);

            string fieldClass;
            if (IsRange(f))
            {
                fieldClass = string.Empty;
            }
            else if (!string.IsNullOrEmpty(f.CssClass))
            {
                fieldClass = f.CssClass;
            }
            else
            {
                if (f.Component == FormComponent.TextArea | f.Component == FormComponent.CheckBox)
                    fieldClass = "col-sm-12";
                else
                    fieldClass = colClass;
            }
            htmlField.WithCssClass(fieldClass);

            if (IsRange(f))


            if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(f.Name))
                htmlField.WithCssClass("has-error");

            if (PageState == PageState.View && UISettings.ShowViewModeAsStatic)
                htmlField.WithCssClass("jjborder-static");

            if (f.Component != FormComponent.CheckBox)
                htmlField.AppendElement(new JJLabel(f));

            if (IsViewModeAsStatic)
                htmlField.AppendElement(GetStaticField(f));
            else
                htmlField.AppendElement(GetControlField(f, value));
        }

        return html;
    }

    private HtmlElement GetHtmlFormHorizontal(List<FormElementField> fields)
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

        var html = new HtmlElement(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.FormHorizontal);

        int colCount = 1;
        HtmlElement row = null;
        foreach (var f in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
                continue;

            //value
            object value = null;
            if (Values != null && Values.Contains(f.Name))
                value = FieldManager.FormatVal(Values[f.Name], f);

            var label = new JJLabel(f);
            label.CssClass = labelClass;

            fldClass += string.IsNullOrEmpty(f.CssClass) ? "" : $" {f.CssClass}";
            if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(f.Name))
                fldClass += " has-error";

            if (colCount == 1 || colCount >= cols)
            {
                colCount = 1;
                row = new HtmlElement(HtmlTag.Div)
                    .WithCssClass("form-group")
                    .WithCssClassIf(BootstrapHelper.Version > 3, "row mb-3");

                html.AppendElement(row);
            }

            string colClass = fieldClass;
            if (IsRange(f))
            {
                colCount = 1;
                colClass = string.Empty;
            }
            else if (f.Component == FormComponent.TextArea)
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
             .AppendElement(label)
             .AppendElement(HtmlTag.Div, col =>
             {
                 col.WithCssClass(colClass);
                 if (IsViewModeAsStatic)
                     col.AppendElement(GetStaticField(f));
                 else
                     col.AppendElement(GetControlField(f, value));
             });
        }

        return html;
    }

    private HtmlElement GetStaticField(FormElementField f)
    {
        var tag = BootstrapHelper.Version == 3 ? HtmlTag.P : HtmlTag.Span;
        var html = new HtmlElement(tag)
            .WithCssClass("form-control-static")
            .AppendText(FieldManager.ParseVal(Values, f));

        return html;
    }

    private HtmlElement GetControlField(FormElementField f, object value)
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

        return field.RenderHtmlElement();
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

    private bool IsRange(FormElementField f)
    {
        return PageState == PageState.Filter & f.Filter.Type == FilterMode.Range;
    }

}
