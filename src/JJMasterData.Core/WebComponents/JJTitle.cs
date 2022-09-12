using System;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJTitle : JJBaseView
{
    /// <summary>
    /// Descrição do título
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Descrição do sub-título
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// Tamanho da fonte do titulo (default(H1)
    /// </summary>
    public HeadingSize Size { get; set; }

    public JJTitle(string title, string subtitle)
    {
        Title = title;
        SubTitle = subtitle;
        Size = HeadingSize.H1;
    }

    public JJTitle(FormElement form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        Title = form.Title;
        SubTitle = form.SubTitle;
        Size = HeadingSize.H1;
    }


    protected override string RenderHtml()
    {
        StringBuilder html = new StringBuilder();
        html.Append($"<div class=\"{(BootstrapHelper.Version == 3 ? "page-header" : "pb-2 mt-4 mb-2 border-bottom")} ");
        html.Append(CssClass);
        html.Append("\"");
        if (!string.IsNullOrEmpty(Name))
        {
            html.Append(" id=\"");
            html.Append(Name);
            html.Append("\"");
        }
        html.AppendLine(">");
        html.Append("<h");
        html.Append((int)Size);
        html.Append(">");
        html.Append(Translate.Key(Title));
        html.Append(" <small>");
        html.Append(Translate.Key(SubTitle));
        html.Append("</small>");
        html.Append("</h");
        html.Append((int)Size);
        html.AppendLine(">");
        html.AppendLine("</div>");

        return html.ToString();
    }

}
