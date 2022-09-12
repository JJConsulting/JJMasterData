using System;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJTextDateTime : JJBaseControl
{

    internal static JJTextDateTime GetInstance(FormElementField f,
                                 object value,
                                 bool enable = true,
                                 bool readOnly = false,
                                 string name = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        JJTextDateTime text = new JJTextDateTime();
        text.SetAttr(f.Attributes);
        text.ToolTip = f.HelpDescription;
        text.MaxLength = f.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Enable = enable;
        text.ReadOnly = readOnly;
        text.Name = name ?? f.Name;

        return text;
    }

    protected override string RenderHtml()
    {
        string cssClass = "form-control ";
        cssClass += !string.IsNullOrEmpty(CssClass) ? CssClass : "";
        StringBuilder html = new();
        html.Append("<div class=\"input-group date jjform-datetime");
        html.Append(!string.IsNullOrEmpty(CssClass) ? CssClass : "");
        html.Append("\"> ");

        html.Append("<input class=\"");
        html.Append(cssClass);
        html.Append("\" id=\"");
        html.Append(Name);
        html.Append("\" name=\"");
        html.Append(Name);
        html.Append("\" type=\"text\" ");
        html.Append("maxlength =\"19\" ");
        
        if (!string.IsNullOrEmpty(Text))
        {
            html.Append("value =\"");
            html.Append($"{DateTime.Parse(Text):d} {DateTime.Parse(Text):HH:mm:ss}");
            html.Append("\" ");
        }
        
        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key(ToolTip));
            html.Append("\" ");
        }

        if (ReadOnly)
            html.Append("readonly ");

        if (!Enable)
            html.Append("disabled ");

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(" ");
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
        }

        html.Append("/ data-input>");
        html.Append($"<div class=\"{BootstrapHelper.InputGroupAddon}\" {BootstrapHelper.DataToggle}> ");
        html.Append($"	<span class=\"fa fa-{BootstrapHelper.DateIcon} {(BootstrapHelper.Version == 4 ? "input-group-text" : string.Empty)}\"></span> ");
        html.Append("</div> ");
        html.Append("</div>");

        return html.ToString();
    }

}
