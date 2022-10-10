using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.WebComponents
{
    internal class JJDataPanelControl : JJBaseView
    {




        //private string GetHtmlFormVertical(List<FormElementField> fields)
        //{
        //    string colClass = "";


        //    int cols = FormCols;
        //    if (cols > 12)
        //        cols = 12;

        //    if (cols >= 1)
        //        colClass = string.Format(" col-sm-{0}", (12 / cols));

        //    var html = new StringBuilder();

        //    int linegroup = int.MinValue;
        //    bool isfirst = true;
        //    foreach (var f in fields)
        //    {
        //        //visible expression
        //        bool visible = FieldManager.IsVisible(f, PageState, Values);
        //        if (!visible)
        //        {
        //            continue;
        //        }

        //        //value
        //        object value = null;
        //        if (Values != null && Values.Contains(f.Name))
        //            value = FieldManager.FormatVal(Values[f.Name], f);

        //        if (linegroup != f.LineGroup)
        //        {
        //            if (isfirst)
        //                isfirst = false;
        //            else
        //                html.AppendLine("\t</div>");

        //            html.AppendLine("\t<div class=\"row\">");
        //            linegroup = f.LineGroup;
        //        }

        //        string fieldClass = BootstrapHelper.FormGroup;

        //        if (!string.IsNullOrEmpty(f.CssClass))
        //        {
        //            fieldClass += string.Format(" {0}", f.CssClass);
        //        }
        //        else
        //        {
        //            if (cols > 1 && f.Component == FormComponent.TextArea)
        //                fieldClass += " col-sm-12";
        //            else
        //                fieldClass += colClass;
        //        }

        //        if (BootstrapHelper.Version == 3 && Erros != null && Erros.Contains(f.Name))
        //            fieldClass += " has-error";

        //        if (PageState == PageState.View && ShowViewModeAsStatic)
        //            fieldClass += " jjborder-static";

        //        html.AppendLine("\t\t<div class=\"" + fieldClass + "\">");

        //        if (f.Component != FormComponent.CheckBox)
        //        {
        //            html.Append("\t\t\t");
        //            html.AppendLine(new JJLabel(f).GetHtml());
        //        }

        //        html.Append("\t\t\t");

        //        if (PageState == PageState.View && ShowViewModeAsStatic)
        //        {
        //            html.Append("<p class=\"form-control-static\">");
        //            //TODO: recuperar valor do texto corretamente quando for combo, search, etc..
        //            html.Append(value);
        //            html.AppendLine("</p>");
        //        }
        //        else
        //        {
        //            html.AppendLine(GetHtmlField(f, value));
        //        }

        //        html.AppendLine("\t\t</div>");

        //    }

        //    html.AppendLine("\t</div>");

        //    return html.ToString();
        //}


        //private string GetHtmlField(FormElementField f, object value)
        //{
        //    var field = FieldManager.GetField(f, PageState, value, Values);

        //    if (BootstrapHelper.Version > 3 && Erros != null && Erros.Contains(f.Name))
        //    {
        //        if (field is JJTextGroup txtGroup)
        //            txtGroup.TextBox.CssClass = "is-invalid";
        //        else
        //            field.CssClass = "is-invalid";
        //    }

        //    if (f.Actions == null)
        //        return field.GetHtml();

        //    var actions = f.Actions.GetAll().FindAll(x => x.IsVisible);
        //    if (actions.Count == 0)
        //        return field.GetHtml();

        //    if (!(field is JJTextGroup))
        //        return field.GetHtml();

        //    //Actions
        //    var textBox = (JJTextGroup)field;
        //    foreach (BasicAction action in actions)
        //    {
        //        var link = ActionManager.GetLinkField(action, Values, PageState, field);
        //        var onRender = OnRenderAction;
        //        if (onRender != null)
        //        {
        //            var args = new ActionEventArgs(action, link, Values);
        //            onRender.Invoke(this, args);
        //        }
        //        if (link != null)
        //            textBox.Actions.Add(link);
        //    }

        //    return textBox.GetHtml();
        //}

    }
}
