using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

internal abstract class JJBaseData : JJBaseView
{
    public JJDataPanel DataPanel { get; set; }

    public bool ShowTitle { get; set; }

    public HeadingSize TitleSize { get; set; }

    internal abstract HtmlElement GetHtmlForm();

    internal abstract HtmlElement GetHtmlBottomBar();

    internal override HtmlElement RenderHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div);

        if (ShowTitle)
            html.AppendElement(GetTitle());

        html.AppendElement(GetHtmlForm());

        if (DataPanel.Erros?.Count > 0)
            html.AppendElement(new JJValidationSummary(DataPanel.Erros));

        html.AppendElement(GetHtmlBottomBar());

        return html;
    }

    internal JJTitle GetTitle()
    {
        var formElement = DataPanel.FormElement;
        var title = new JJTitle(formElement.Title, formElement.SubTitle)
        {
            Size = TitleSize
        };
        return title;
    }


}





