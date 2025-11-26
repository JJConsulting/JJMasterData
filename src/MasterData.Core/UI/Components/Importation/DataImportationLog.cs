using System;
using System.Globalization;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;

using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataImportationLog
{
    private readonly DataImportationReporter _reporter;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;
    private readonly IHttpContext _currentContext;
    internal DataImportationLog(JJDataImportation dataImportation)
    {
        _stringLocalizer = dataImportation.StringLocalizer;
        _currentContext = dataImportation.CurrentContext;
        _reporter = dataImportation.GetCurrentReporter();
    }

    public HtmlBuilder GetHtmlLog()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendComponent(GetAlertPanel())
            .Append(GetSummaryHtml())
            .Append(HtmlTag.Div, div =>
            {
                div.AppendText("\u00A0");
            })
            .Append(GetLogDetailsHtml());

        return html;
    }

    public HtmlBuilder GetSummaryHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithStyle( "text-align: center;")
            .WithCssClass("jjlabel-process");

        html.AppendIf(_reporter.EndDate != DateTime.MinValue, HtmlTag.Div, div =>
        {
            string elapsedTime = Format.FormatTimeSpan(_reporter.StartDate, _reporter.EndDate);
            div.AppendText(_stringLocalizer["Process performed on {0}", elapsedTime]);
        });

        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelSuccess)
             .WithAttribute("id", "lblInsert")
             .WithAttributeIf(_reporter.Insert == 0, "style", "display:none;")
             .AppendText(_stringLocalizer["Inserted:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblInsertCount")
                      .AppendText(_reporter.Insert.ToString("N0"));
             });
        });
        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelSuccess)
             .WithAttribute("id", "lblUpdate")
             .WithAttributeIf(_reporter.Update == 0, "style", "display:none;")
             .AppendText(_stringLocalizer["Updated:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblUpdateCount")
                      .AppendText(_reporter.Update.ToString("N0"));
             });
        });
        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelDefault)
             .WithAttribute("id", "lblDelete")
             .WithAttributeIf(_reporter.Delete == 0, "style", "display:none;")
             .AppendText(_stringLocalizer["Deleted:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblDeleteCount")
                      .AppendText(_reporter.Delete.ToString("N0"));
             });
        });

        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelWarning)
             .WithAttribute("id", "lblIgnore")
             .WithAttributeIf(_reporter.Ignore == 0, "style", "display:none;")
             .AppendText(_stringLocalizer["Ignored:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblIgnoreCount")
                      .AppendText(_reporter.Ignore.ToString("N0"));
             });
        });

        html.Append(HtmlTag.Span, s =>
        {
            s.WithCssClass(BootstrapHelper.LabelDanger)
             .WithAttribute("id", "lblError")
             .WithAttributeIf(_reporter.Error == 0, "style", "display:none;")
             .AppendText(_stringLocalizer["Errors:"])
             .Append(HtmlTag.Span, count =>
             {
                 count.WithAttribute("id", "lblErrorCount")
                      .AppendText(_reporter.Error.ToString("N0"));
             });
        });

        return html;
    }

    private HtmlBuilder GetLogDetailsHtml()
    {
        var panel = new JJCollapsePanel(_currentContext.Request.Form)
        {
            Title = _stringLocalizer["Importation Details"],
            TitleIcon = new JJIcon(FontAwesomeIcon.Film),
            ExpandedByDefault = true,
            HtmlBuilderContent = new HtmlBuilder(HtmlTag.Div)
                .Append(HtmlTag.B,b=>b.AppendText(_stringLocalizer["Start:"]))
                .AppendText(_reporter.StartDate.ToString(CultureInfo.CurrentCulture))
                .Append(HtmlTag.Br)
                .Append(HtmlTag.B,b=>b.AppendText(_stringLocalizer["End:"]))
                .AppendText(_reporter.EndDate.ToString(CultureInfo.CurrentCulture))
        };

        if (!string.IsNullOrEmpty(_reporter.UserId))
        {
            panel.HtmlBuilderContent.Append(HtmlTag.Br)
                .Append(HtmlTag.B, b => b.AppendText(_stringLocalizer["UserId:"]))
                .AppendText("\u00A0")
                .AppendText(_reporter.UserId.ToString(CultureInfo.CurrentCulture));
        }

        panel.HtmlBuilderContent
              .Append(HtmlTag.Br)
              .AppendText(_reporter.ErrorLog.ToString().Replace("\r\n", "<br>"));

        return panel.BuildHtml();
    }

    private JJAlert GetAlertPanel()
    {
        var alert = new JJAlert
        {
            CssClass = "text-center",
            ShowIcon = true
        };

        if (_reporter.HasError || _reporter.TotalProcessed == _reporter.Error)
        {
            alert.Icon = FontAwesomeIcon.ExclamationTriangle;
            alert.Color = BootstrapColor.Danger;
            alert.Title = _stringLocalizer["Error importing file!"];
            alert.Messages.Add(_stringLocalizer[_reporter.Message]);
        }
        else if (_reporter.Error > 0)
        {
            alert.Icon = FontAwesomeIcon.InfoCircle;
            alert.Color = BootstrapColor.Info;
            alert.Title =_stringLocalizer[_reporter.Message];
        }
        else
        {
            alert.Icon = FontAwesomeIcon.Check;
            alert.Color = BootstrapColor.Success;
            alert.Title = _stringLocalizer[_reporter.Message];
        }

        return alert;
    }

}
