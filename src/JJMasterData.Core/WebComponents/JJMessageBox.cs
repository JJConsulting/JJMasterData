using System.Text;
using JJMasterData.Commons.Language;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Exibe uma caixa de mensagem (dialog)
/// </summary>
public class JJMessageBox
{
    private string _Text;
    public string Text
    {
        get => _Text;
        
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _Text = value.Replace("'","`");
            }
            else
            {
                _Text = value;
            }
            
        }
    }
    
    public string Title { get; set; }
    public bool AddScriptTags { get; set; }
    public MessageIcon Icon { get; set; }
    public MessageSize Size { get; set; }

    public JJMessageBox(string text, MessageIcon icon, bool addScriptTags)
    {
        Text = text;
        Icon = icon;
        Size = MessageSize.Default;
        AddScriptTags = addScriptTags;
        Title = Icon switch
        {
            MessageIcon.Error => "Erro",
            MessageIcon.Warning => "Aviso",
            MessageIcon.Info => "Info",
            _ => "Mensagem",
        };
    }

    public JJMessageBox(string text, string title, MessageIcon icon, MessageSize size, bool addScriptTags)
    {
        Title = title;
        Text = text;
        Icon = icon;
        Size = size;
        AddScriptTags = addScriptTags;
    }
    
    public string GetHtml()
    {
        StringBuilder javaScript = new();
        if (AddScriptTags)
        {
            javaScript.AppendLine("<script type=\"text/javascript\" lang=\"javascript\">");
            javaScript.Append("\t");
        }

        string msg = Translate.Key(Text);
        javaScript.Append("\t\t");
        javaScript.AppendLine("$(document).ready(function() {");
        javaScript.Append("\t\t\t");
        javaScript.Append("messageBox.show('");
        javaScript.Append(Translate.Key(Title));
        javaScript.Append("','");
        javaScript.Append(msg.Replace("<br>","\\r\\n").Replace("\r\n",""));
        javaScript.Append("', ");
        javaScript.Append((int)Icon);
        javaScript.Append(", ");
        javaScript.Append((int)Size);
        javaScript.AppendLine(");");
        javaScript.Append("\t\t");
        javaScript.AppendLine("});");
    
        if (AddScriptTags)
        {
            javaScript.Append("\t");
            javaScript.AppendLine("</script>");
        }
        
        return javaScript.ToString();
    }

}
