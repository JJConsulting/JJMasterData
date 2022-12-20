using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.Html;
using System;
using System.Globalization;

namespace JJMasterData.Core.WebComponents;

internal class DataImpLog
{
    public DataImpReporter reporter { get; private set; }

    internal DataImpLog(JJDataImp dataImp)
    {
        reporter = dataImp.GetCurrentReporter();
    }

    public HtmlBuilder GetHtmlLog()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(GetAlertPanel())
            .AppendElement(GetHtmlResume())
            .AppendElement(HtmlTag.Div, div =>
            {
                div.AppendText("&nbsp;");
            })
            .AppendElement(GetHtmlLogDetails());

        return html;
    }

    public HtmlBuilder GetHtmlResume()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("style", "text-align: center;")
            .WithCssClass("jjlabel-process");

        html.AppendElementIf(reporter.EndDate != DateTime.MinValue, HtmlTag.Div, div =>
        {
            string elapsedTime = Format.FormatTimeSpan(reporter.StartDate, reporter.EndDate);
            div.AppendText(Translate.Key("Process performed on {0}", elapsedTime));
        });

        html.AppendElement(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelSucess)
             .WithAttribute("id", "lblInsert")
             .WithAttributeIf(reporter.Insert == 0, "style", "display:none;")
             .AppendText(Translate.Key("Inserted:"))
             .AppendElement(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblInsertCount")
                      .AppendText(reporter.Insert.ToString("N0"));
             });
        });
        html.AppendElement(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelSucess)
             .WithAttribute("id", "lblUpdate")
             .WithAttributeIf(reporter.Update == 0, "style", "display:none;")
             .AppendText(Translate.Key("Updated:"))
             .AppendElement(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblUpdateCount")
                      .AppendText(reporter.Update.ToString("N0"));
             });
        });
        html.AppendElement(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelDefault)
             .WithAttribute("id", "lblDelete")
             .WithAttributeIf(reporter.Delete == 0, "style", "display:none;")
             .AppendText(Translate.Key("Deleted:"))
             .AppendElement(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblDeleteCount")
                      .AppendText(reporter.Delete.ToString("N0"));
             });
        });

        html.AppendElement(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelWarning)
             .WithAttribute("id", "lblIgnore")
             .WithAttributeIf(reporter.Ignore == 0, "style", "display:none;")
             .AppendText(Translate.Key("Ignored:"))
             .AppendElement(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblIgnoreCount")
                      .AppendText(reporter.Ignore.ToString("N0"));
             });
        });

        html.AppendElement(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelDanger)
             .WithAttribute("id", "lblError")
             .WithAttributeIf(reporter.Error == 0, "style", "display:none;")
             .AppendText(Translate.Key("Errors:"))
             .AppendElement(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblErrorCount")
                      .AppendText(reporter.Error.ToString("N0"));
             });
        });

        return html;
    }

    private HtmlBuilder GetHtmlLogDetails()
    {
        var panel = new JJCollapsePanel();
        panel.Title = "(Click here for more details)";
        panel.TitleIcon = new JJIcon(IconType.Film);
        panel.ExpandedByDefault = false;
        panel.HtmlBuilderContent = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(HtmlTag.Label, label =>
            {
                label.AppendText(Translate.Key("Date:"));
            })
            .AppendText("&nbsp;")
            .AppendText(Translate.Key("start"))
            .AppendText("&nbsp;")
            .AppendText(reporter.StartDate.ToString(CultureInfo.CurrentCulture))
            .AppendText("&nbsp;")
            .AppendText(Translate.Key("end"))
            .AppendText("&nbsp;")
            .AppendText(reporter.EndDate.ToString(CultureInfo.CurrentCulture))
            .AppendElement(HtmlTag.Br);

        if (!string.IsNullOrEmpty(reporter.UserId))
        {
            panel.HtmlBuilderContent.AppendElement(HtmlTag.Label, label =>
            {
                label.AppendText(Translate.Key("User Id:"));
            })
            .AppendText("&nbsp;")
            .AppendText(reporter.UserId)
            .AppendElement(HtmlTag.Br);
        }

        panel.HtmlBuilderContent
                .AppendElement(HtmlTag.Br)
                .AppendText(reporter.ErrorLog.ToString().Replace("\r\n", "<br>"));

        return panel.RenderHtml();
    }

    private JJAlert GetAlertPanel()
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
            alert.Title = Translate.Key(reporter.Message);
        }

        return alert;
    }

}
