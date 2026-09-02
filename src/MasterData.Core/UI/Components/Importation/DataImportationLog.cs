using System;
using System.Globalization;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Importation.Background;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataImportationLog
{
    private readonly BackgroundJobSnapshot? _status;
    private readonly ImportJobResult? _result;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;

    internal DataImportationLog(JJDataImportation dataImportation)
    {
        _stringLocalizer = dataImportation.StringLocalizer;
        _status = dataImportation.GetCurrentStatus();
        _result = _status?.Result as ImportJobResult ?? _status?.Progress?.Details as ImportJobResult;
    }

    public HtmlBuilder GetHtmlLog()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .AppendComponent(GetAlertPanel())
            .Append(GetSummaryHtml())
            .Append(HtmlTag.Div, div => div.AppendText("\u00A0"))
            .Append(GetLogDetailsHtml());
    }

    public HtmlBuilder GetSummaryHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithStyle("text-align: center;")
            .WithCssClass("jjlabel-process");

        if (_status?.StartedAt is { } startedAt && _status.CompletedAt is { } completedAt)
        {
            var elapsedTime = Format.FormatTimeSpan(startedAt.LocalDateTime, completedAt.LocalDateTime);
            html.Append(HtmlTag.Div,
                div => div.AppendText(_stringLocalizer["Process performed on {0}", elapsedTime]));
        }

        AppendCount(html, "lblInsert", BootstrapHelper.LabelSuccess, "Inserted:", _result?.Inserted ?? 0);
        AppendCount(html, "lblUpdate", BootstrapHelper.LabelSuccess, "Updated:", _result?.Updated ?? 0);
        AppendCount(html, "lblDelete", BootstrapHelper.LabelDefault, "Deleted:", _result?.Deleted ?? 0);
        AppendCount(html, "lblIgnore", BootstrapHelper.LabelWarning, "Ignored:", _result?.Ignored ?? 0);
        AppendCount(html, "lblError", BootstrapHelper.LabelDanger, "Errors:", _result?.Errors ?? 0);

        return html;
    }

    private void AppendCount(HtmlBuilder html, string id, string cssClass, string label, int value)
    {
        html.Append(HtmlTag.Span, span =>
        {
            span.WithCssClass(cssClass)
                .WithAttribute("id", id)
                .WithAttributeIf(value == 0, "style", "display:none;")
                .AppendText(_stringLocalizer[label])
                .Append(HtmlTag.Span, count => count
                    .WithAttribute("id", $"{id}Count")
                    .AppendText(value.ToString("N0")));
        });
    }

    private HtmlBuilder GetLogDetailsHtml()
    {
        var startDate = _status?.StartedAt?.LocalDateTime ?? _status?.CreatedAt.LocalDateTime ?? DateTime.MinValue;
        var endDate = _status?.CompletedAt?.LocalDateTime ?? DateTime.MinValue;
        var content = new HtmlBuilder(HtmlTag.Div)
            .Append(HtmlTag.B, b => b.AppendText(_stringLocalizer["Start:"]))
            .AppendText(startDate.ToString(CultureInfo.CurrentCulture))
            .AppendBr()
            .Append(HtmlTag.B, b => b.AppendText(_stringLocalizer["End:"]))
            .AppendText(endDate.ToString(CultureInfo.CurrentCulture));

        if (!string.IsNullOrEmpty(_status?.UserId))
        {
            content.AppendBr()
                .Append(HtmlTag.B, b => b.AppendText(_stringLocalizer["UserId:"]))
                .AppendText("\u00A0")
                .AppendText(_status.UserId);
        }

        foreach (var error in _result?.ErrorMessages ?? [])
            content.AppendBr().AppendText(error);

        return new JJCollapsePanel
        {
            Title = _stringLocalizer["Importation Details"],
            TitleIcon = new JJIcon(FontAwesomeIcon.Film),
            ExpandedByDefault = true,
            Content = content
        }.GetHtmlBuilder();
    }

    private JJAlert GetAlertPanel()
    {
        var message = _status?.State == BackgroundJobState.Cancelled
            ? _stringLocalizer["Process aborted by user"]
            : _status?.Progress?.Message ?? _status?.Error ?? _stringLocalizer["Waiting..."];
        var alert = new JJAlert
        {
            CssClass = "text-center",
            ShowIcon = true
        };

        if (_status?.State == BackgroundJobState.Failed ||
            _result is { TotalProcessed: > 0 } && _result.TotalProcessed == _result.Errors)
        {
            alert.Icon = FontAwesomeIcon.ExclamationTriangle;
            alert.Color = BootstrapColor.Danger;
            alert.Title = _stringLocalizer["Error importing file!"];
            alert.Messages.Add(message);
        }
        else if (_result?.Errors > 0)
        {
            alert.Icon = FontAwesomeIcon.InfoCircle;
            alert.Color = BootstrapColor.Info;
            alert.Title = message;
        }
        else
        {
            alert.Icon = FontAwesomeIcon.Check;
            alert.Color = BootstrapColor.Success;
            alert.Title = message;
        }

        return alert;
    }
}
