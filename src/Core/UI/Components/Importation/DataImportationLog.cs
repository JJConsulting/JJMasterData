using System;
using System.Globalization;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class DataImportationLog
{
    internal DataImportationReporter Reporter { get; private set; }
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    internal IHttpContext CurrentContext { get; }
    internal DataImportationLog(JJDataImportation dataImportation)
    {
        StringLocalizer = dataImportation.StringLocalizer;
        CurrentContext = dataImportation.CurrentContext;
        Reporter = dataImportation.GetCurrentReporter();
    }

    public HtmlBuilder GetHtmlLog()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendComponent(GetAlertPanel())
            .Append(GetHtmlResume())
            .Append(HtmlTag.Div, div =>
            {
                div.AppendText("&nbsp;");
            })
            .Append(GetHtmlLogDetails());

        return html;
    }

    public HtmlBuilder GetHtmlResume()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("style", "text-align: center;")
            .WithCssClass("jjlabel-process");

        html.AppendIf(Reporter.EndDate != DateTime.MinValue, HtmlTag.Div, div =>
        {
            string elapsedTime = Format.FormatTimeSpan(Reporter.StartDate, Reporter.EndDate);
            div.AppendText(StringLocalizer["Process performed on {0}", elapsedTime]);
        });

        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelSucess)
             .WithAttribute("id", "lblInsert")
             .WithAttributeIf(Reporter.Insert == 0, "style", "display:none;")
             .AppendText(StringLocalizer["Inserted:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblInsertCount")
                      .AppendText(Reporter.Insert.ToString("N0"));
             });
        });
        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelSucess)
             .WithAttribute("id", "lblUpdate")
             .WithAttributeIf(Reporter.Update == 0, "style", "display:none;")
             .AppendText(StringLocalizer["Updated:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblUpdateCount")
                      .AppendText(Reporter.Update.ToString("N0"));
             });
        });
        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelDefault)
             .WithAttribute("id", "lblDelete")
             .WithAttributeIf(Reporter.Delete == 0, "style", "display:none;")
             .AppendText(StringLocalizer["Deleted:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblDeleteCount")
                      .AppendText(Reporter.Delete.ToString("N0"));
             });
        });

        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelWarning)
             .WithAttribute("id", "lblIgnore")
             .WithAttributeIf(Reporter.Ignore == 0, "style", "display:none;")
             .AppendText(StringLocalizer["Ignored:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblIgnoreCount")
                      .AppendText(Reporter.Ignore.ToString("N0"));
             });
        });

        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelDanger)
             .WithAttribute("id", "lblError")
             .WithAttributeIf(Reporter.Error == 0, "style", "display:none;")
             .AppendText(StringLocalizer["Errors:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblErrorCount")
                      .AppendText(Reporter.Error.ToString("N0"));
             });
        });

        return html;
    }

    private HtmlBuilder GetHtmlLogDetails()
    {
        var panel = new JJCollapsePanel(CurrentContext.Request.Form)
        {
            Title = StringLocalizer["Importation Details"],
            TitleIcon = new JJIcon(IconType.Film),
            ExpandedByDefault = true,
            HtmlBuilderContent = new HtmlBuilder(HtmlTag.Div)
                .Append(HtmlTag.B,b=>b.AppendText(StringLocalizer["Start:"]))
                .AppendText(Reporter.StartDate.ToString(CultureInfo.CurrentCulture))
                .Append(HtmlTag.Br)
                .Append(HtmlTag.B,b=>b.AppendText(StringLocalizer["End:"]))
                .AppendText(Reporter.EndDate.ToString(CultureInfo.CurrentCulture))
        };

        if (!string.IsNullOrEmpty(Reporter.UserId))
        {
            panel.HtmlBuilderContent.Append(HtmlTag.Br)
                .Append(HtmlTag.B, b => b.AppendText(StringLocalizer["UserId:"]))
                .AppendText("&nbsp;")
                .AppendText(Reporter.UserId.ToString(CultureInfo.CurrentCulture));
        }

        panel.HtmlBuilderContent
              .Append(HtmlTag.Br)
              .AppendText(Reporter.ErrorLog.ToString().Replace("\r\n", "<br>"));

        return panel.BuildHtml();
    }

    private JJAlert GetAlertPanel()
    {
        var alert = new JJAlert
        {
            CssClass = "text-center",
            ShowIcon = true
        };

        if (Reporter.HasError || Reporter.TotalProcessed == Reporter.Error)
        {
            alert.Icon = IconType.ExclamationTriangle;
            alert.Color = PanelColor.Danger;
            alert.Title = StringLocalizer["Error importing file!"];
            alert.Messages.Add(StringLocalizer[Reporter.Message]);
        }
        else if (Reporter.Error > 0)
        {
            alert.Icon = IconType.InfoCircle;
            alert.Color = PanelColor.Info;
            alert.Title = StringLocalizer["File imported with errors!"];
            alert.Messages.Add(StringLocalizer[
            Reporter.Message]);
        }
        else
        {
            alert.Icon = IconType.Check;
            alert.Color = PanelColor.Success;
            alert.Title = StringLocalizer[Reporter.Message];
        }

        return alert;
    }

}
