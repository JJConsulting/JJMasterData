using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

internal class JJDataPanelControl : JJBaseView
{
    //public FormElement FormElement { get; private set; }

    public UIForm UISettings { get; private set; }

    public FieldManager FieldManager { get; private set; }

    public PageState PageState { get; private set; }

    public Hashtable Erros { get; private set; }

    public Hashtable Values { get; private set; }

    public string PanelName { get; private set; }


    public JJDataPanelControl(JJDataPanel dataPanel)
    {
      
    }

    private string GetHtmlFormVertical(List<FormElementField> fields)
    {
        string colClass = "";
        int cols = UISettings.FormCols;
        if (cols > 12)
            cols = 12;

        if (cols >= 1)
            colClass = string.Format(" col-sm-{0}", (12 / cols));

        var html = new StringBuilder();

        int linegroup = int.MinValue;
        bool isfirst = true;
        foreach (var f in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
            {
                continue;
            }

            //value
            object value = null;
            if (Values != null && Values.Contains(f.Name))
                value = FieldManager.FormatVal(Values[f.Name], f);

            if (linegroup != f.LineGroup)
            {
                if (isfirst)
                    isfirst = false;
                else
                    html.AppendLine("\t</div>");

                html.AppendLine("\t<div class=\"row\">");
                linegroup = f.LineGroup;
            }

            string fieldClass = BootstrapHelper.FormGroup;

            if (!string.IsNullOrEmpty(f.CssClass))
            {
                fieldClass += string.Format(" {0}", f.CssClass);
            }
            else
            {
                if (cols > 1 && f.Component == FormComponent.TextArea)
                    fieldClass += " col-sm-12";
                else
                    fieldClass += colClass;
            }

            if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(f.Name))
                fieldClass += " has-error";

            if (PageState == PageState.View && UISettings.ShowViewModeAsStatic)
                fieldClass += " jjborder-static";

            html.AppendLine("\t\t<div class=\"" + fieldClass + "\">");

            if (f.Component != FormComponent.CheckBox)
            {
                html.Append("\t\t\t");
                html.AppendLine(new JJLabel(f).GetHtml());
            }

            html.Append("\t\t\t");

            if (PageState == PageState.View && UISettings.ShowViewModeAsStatic)
            {
                html.Append("<p class=\"form-control-static\">");
                //TODO: recuperar valor do texto corretamente quando for combo, search, etc..
                html.Append(value);
                html.AppendLine("</p>");
            }
            else
            {

                var field = FieldManager.GetField(f, PageState, Values, value);
                bool enable = FieldManager.IsEnable(f, PageState, Values);
                field.Enabled = enable;
                if (BootstrapHelper.Version > 3 && Erros != null && Erros.Contains(f.Name))
                {
                    field.CssClass = "is-invalid";
                }

                html.AppendLine(field.GetHtml());
            }

            html.AppendLine("\t\t</div>");

        }

        html.AppendLine("\t</div>");

        return html.ToString();
    }

    private string GetHtmlFormHorizontal(List<FormElementField> fields)
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
            fullClass = "col-sm-10";
        }
        else if (cols == 3)
        {
            labelClass = "col-sm-2";
            fieldClass = "col-sm-2";
            fullClass = "col-sm-10";
        }
        else if (cols >= 4)
        {
            cols = 4;
            labelClass = "col-sm-1";
            fieldClass = "col-sm-2";
            fullClass = "col-sm-8";
        }

        var html = new StringBuilder();
        html.Append($"<div class=\"{BootstrapHelper.FormHorizontal}\">");

        int colCount = 1;
        foreach (var f in fields)
        {
            //visible expression
            bool visible = FieldManager.IsVisible(f, PageState, Values);
            if (!visible)
            {
                continue;
            }

            //value
            object fieldValue = null;
            if (Values != null && Values.Contains(f.Name))
                fieldValue = FieldManager.FormatVal(Values[f.Name], f);

            var label = new JJLabel(f);
            label.CssClass = labelClass;

            fldClass += string.IsNullOrEmpty(f.CssClass) ? "" : string.Format(" {0}", f.CssClass);
            if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(f.Name))
                fldClass += " has-error";

            string bs4Row = BootstrapHelper.Version > 3 ? "row" : string.Empty;


            var field = FieldManager.GetField(f, PageState, Values, fieldValue);
            bool enable = FieldManager.IsEnable(f, PageState, Values);
            field.Enabled = enable;
            if (BootstrapHelper.Version > 3 && Erros != null && Erros.Contains(f.Name))
            {
                field.CssClass = "is-invalid";
            }

            if (f.Component == FormComponent.TextArea)
            {
                if (colCount > 1)
                    html.AppendLine("\t</div>");

                html.AppendLine($"\t<div class=\"{BootstrapHelper.FormGroup} {bs4Row}\">");
                html.Append("\t\t");
                html.Append("<div class=\"");
                html.Append(fldClass);
                html.AppendLine("\">");
                html.Append("\t\t\t");
                html.AppendLine(label.GetHtml());
                html.AppendLine("\t\t\t<div class=\"" + fullClass + "\">");
                html.Append("\t\t\t\t");
                if (PageState == PageState.View && UISettings.ShowViewModeAsStatic)
                {
                    html.Append("<p class=\"form-control-static\">");
                    html.Append(fieldValue);
                    html.AppendLine("</p>");
                }
                else
                {
                    html.AppendLine(field.GetHtml());
                }
                html.AppendLine("\t\t\t</div>");
                html.AppendLine("\t\t</div>");
                html.AppendLine("\t</div>");

                colCount = 1;
            }
            else
            {
                if (colCount == 1)
                    html.AppendLine($"\t<div class=\"{BootstrapHelper.FormGroup}\">");

                html.Append("\t\t");
                html.Append("<div class=\"");
                html.Append($"{fldClass} {bs4Row}");
                html.AppendLine("\">");
                html.Append("\t\t\t");
                html.AppendLine(label.GetHtml());
                html.AppendLine("\t\t\t<div class=\"" + fieldClass + "\">");
                html.Append("\t\t\t\t");
                if (PageState == PageState.View && UISettings.ShowViewModeAsStatic)
                {
                    html.Append("<p class=\"form-control-static\">");
                    html.Append(fieldValue);
                    html.AppendLine("</p>");
                }
                else
                {
                    html.AppendLine(field.GetHtml());
                }
                html.AppendLine("\t\t\t</div>");
                html.AppendLine("\t\t</div>");

                if (colCount >= cols)
                {
                    html.AppendLine("\t</div>");
                    colCount = 1;
                }
                else
                {
                    colCount++;
                }
            }

        }
        if (colCount > 1)
            html.AppendLine("\t</div>");

        html.AppendLine("</div>");
        return html.ToString();
    }

}
