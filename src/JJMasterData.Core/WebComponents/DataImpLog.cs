using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

internal class DataImpLog
{
    public DataImpReporter reporter { get; private set; }

    internal DataImpLog(JJDataImp dataImp)
    {
        reporter = dataImp.GetCurrentReporter();
    }

    public HtmlElement GetHtmlLog()
    {
        var html = GetAlertPanel();
        html.AppendHiddenInput("current_uploadaction", "");
        html.AppendHiddenInput("filename", "");
        html.AppendElement(GetResumeInfo());
        html.AppendElement(HtmlTag.Div, div =>
        {
            div.AppendText("&nbsp;");
        });
        html.AppendElement(GetHtmlLogDetails());
        html.AppendElement(GetBackButton());
        html.AppendElement(GetHelpButton());

        return html;
    }

    private HtmlElement GetHtmlLogDetails()
    {
        var panel = new JJCollapsePanel();
        panel.Title = "(Click here for more details)";
        panel.TitleIcon = new JJIcon(IconType.Film);
        panel.ExpandedByDefault = false;
        panel.HtmlElementContent = new HtmlElement(HtmlTag.Div);
        panel.HtmlElementContent
            .AppendElement(HtmlTag.Label, label =>
            {
                label.AppendText(Translate.Key("Date:"));
            })
            .AppendText("&nbsp;")
            .AppendText(Translate.Key("start"))
            .AppendText(reporter.StartDate.ToString())
            .AppendText(" &nbsp;&nbsp;")
            .AppendText(Translate.Key("end"))
            .AppendText(reporter.EndDate.ToString())
            .AppendElement(HtmlTag.Br);

        if (!string.IsNullOrEmpty(reporter.UserId))
        {
            panel.HtmlElementContent
                .AppendElement(HtmlTag.Label)
                .AppendText(Translate.Key("User Id:"));

            panel.HtmlElementContent
                .AppendText("&nbsp;")
                .AppendText(reporter.UserId)
                .AppendElement(HtmlTag.Br);
        }

        panel.HtmlElementContent
                .AppendElement(HtmlTag.Br)
                .AppendText(reporter.ErrorLog.ToString().Replace("\r\n", "<br>"));


        return panel.GetHtmlElement();
    }

    private HtmlElement GetAlertPanel()
    {
        var alert = new JJAlert();
        alert.CssClass = "text-center";
        alert.ShowIcon = true;

        if (reporter.HasError || reporter.TotalProcessed == reporter.Error)
        {
            alert.Icon = IconType.ExclamationTriangle;
            alert.Color = PanelColor.Danger;
            alert.Title = Translate.Key("Error importing file!");
            alert.Messages.Add(Translate.Key(reporter.Message));
        }
        else if (reporter.Error > 0)
        {
            alert.Icon = IconType.InfoCircle;
            alert.Color = PanelColor.Info;
            alert.Title = Translate.Key("File imported with errors!");
            alert.Messages.Add(Translate.Key(reporter.Message));
        }
        else
        {
            alert.Icon = IconType.Check;
            alert.Color = PanelColor.Success;
            alert.Messages.Add(Translate.Key(reporter.Message));
        }

        return alert.GetHtmlElement();
    }

    private HtmlElement GetResumeInfo()
    {
        var html = new HtmlElement(HtmlTag.Div)
            .WithAttribute("style", "text-align: center;");

        html.AppendElement(HtmlTag.Div, div =>
        {
            string elapsedTime = Format.FormatTimeSpan(reporter.StartDate, reporter.EndDate);
            div.AppendText(Translate.Key("Process performed on {0}", elapsedTime));
        });

        if (reporter.Insert > 0)
        {
            html.AppendElement(HtmlTag.Span)
                .WithCssClass("label label-success")
                .AppendText(string.Format("{0} {1}", Translate.Key("Inserted:"), reporter.Insert));
        }

        if (reporter.Update > 0)
        {
            html.AppendElement(HtmlTag.Span)
                .WithCssClass("label label-success")
                .AppendText(string.Format("{0} {1}", Translate.Key("Updated:"), reporter.Update));
        }

        if (reporter.Delete > 0)
        {
            html.AppendElement(HtmlTag.Span)
                .WithCssClass("label label-default")
                .AppendText(string.Format("{0} {1}", Translate.Key("Deleted:"), reporter.Delete));
        }

        if (reporter.Ignore > 0)
        {
            html.AppendElement(HtmlTag.Span)
                .WithCssClass("label label-warning")
                .AppendText(string.Format("{0} {1}", Translate.Key("Ignored:"), reporter.Ignore));
        }

        if (reporter.Error > 0)
        {
            html.AppendElement(HtmlTag.Span)
                .WithCssClass("label label-danger")
                .AppendText(string.Format("{0} {1}", Translate.Key("Errors:"), reporter.Error));
        }

        return html;

    }

    private HtmlElement GetHelpButton()
    {
        var btnHelp = new JJLinkButton();
        btnHelp.IconClass = "fa fa-question-circle";
        btnHelp.Text = "Help";
        btnHelp.ShowAsButton = true;
        btnHelp.OnClientClick = "$('#current_uploadaction').val('process_help'); $('form:first').submit();";

        return btnHelp.GetHtmlElement();
    }

    private HtmlElement GetBackButton()
    {
        var btnBack = new JJLinkButton();
        btnBack.IconClass = "fa fa-arrow-left";
        btnBack.Text = "Back";
        btnBack.ShowAsButton = true;
        btnBack.OnClientClick = "uploadFile1Obj.remove();";

        return btnBack.GetHtmlElement();
    }

}
