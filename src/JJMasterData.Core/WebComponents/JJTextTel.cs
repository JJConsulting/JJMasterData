using System;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJTextTel : JJBaseControl
{

    internal static JJTextTel GetInstance(FormElementField f,
                                object value,
                                bool enable = true,
                                bool readOnly = false,
                                string name = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        JJTextTel text = new JJTextTel();
        text.SetAttr(f.Attributes);
        text.ToolTip = f.HelpDescription;
        text.MaxLength = f.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Enable = enable;
        text.ReadOnly = readOnly;
        text.Name = name == null ? f.Name : name;

        return text;
    }
    protected override string RenderHtml()
    {
        string cssClass = "form-control ";
        cssClass += !string.IsNullOrEmpty(CssClass) ? CssClass : "";

        StringBuilder html = new StringBuilder();
        html.Append("<div class=\"input-group\"> ");

        html.Append($"<div class=\"{BootstrapHelper.InputGroupAddon}\"> ");
        if (BootstrapHelper.Version != 3)
            html.Append("<div class=\"input-group-text\">");
        
        html.Append("	<span class=\"fa fa-phone\"></span>");
        html.Append("	<span title=\"Brasil\">&nbsp;&nbsp;+55</span> ");
       

        if (BootstrapHelper.Version != 3)
            html.Append("</div> ");
        
        html.Append("</div>");
        html.Append("<input class=\"");
        html.Append(cssClass);
        html.Append("\" id=\"");
        html.Append(Name);
        html.Append("\" name=\"");
        html.Append(Name);
        html.Append("\" type=\"tel\" ");
        html.Append("data-inputmask=\"'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'\" ");
        html.Append("maxlength =\"19\" ");
    
        if (!string.IsNullOrEmpty(Text))
        {
            html.Append("value =\"");
            html.Append(Text);
            html.Append("\" ");
        }

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.AppendFormat("title=\"{0}\" ", Translate.Key(ToolTip));
        }

        if (ReadOnly)
            html.Append("readonly ");

        if (!Enable)
            html.Append("disabled ");

        foreach (DictionaryEntry attr in Attributes)
        {

            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
            html.Append(" ");
        }

        html.Append("/>");
        html.Append("</div>");

        return html.ToString();
    }

}
