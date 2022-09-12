using System;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJTextArea : JJBaseControl
{

    public int Rows
    {
        get
        {
            string val = GetAttr("rows");
            return string.IsNullOrEmpty(val) ? 5 : int.Parse(val);
        }
        set => SetAttr("rows", value);
    }

    public JJTextArea()
    {
        Rows = 5;
    }

    internal static JJTextArea GetInstance(FormElementField field,
                                 object value,
                                 bool enable = true,
                                 bool readOnly = false,
                                 string name = null)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = new JJTextArea();
        text.SetAttr(field.Attributes);
        text.ToolTip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = value != null ? value.ToString() : "";
        text.Enable = enable;
        text.ReadOnly = readOnly;
        text.Name = name ?? field.Name;

        return text;
    }


    protected override string RenderHtml()
    {
        StringBuilder html = new StringBuilder();
        html.AppendFormat("<textarea name=\"{0}\" ", Name);
        html.AppendFormat("id=\"{0}\" ", Name);

        if (MaxLength > 0)
        {
            html.AppendFormat("maxlength=\"{0}\" ", MaxLength);
        }

        html.Append("cols=\"20\" ");
        html.AppendFormat("strvalid=\"{0}\" ", Translate.Key("Maximum limit of {0} characters!"));
        html.AppendFormat("strchars=\"{0}\" ", Translate.Key("({0} characters remaining)"));

        if (ReadOnly)
            html.Append("readonly ");

        if (!Enable)
            html.Append("disabled ");

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.AppendFormat("title=\"{0}\" ", Translate.Key(ToolTip));
        }

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append('"');
            }
            html.Append(' ');
        }

        html.Append("class=\"form-control\">");
        html.Append(Text);
        html.Append("</textarea> ");

        return html.ToString();
    }
}
