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
            .Append(GetLogDetailsHtml());
    }

    public HtmlBuilder GetSummaryHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("jj-import-summary");

        if (_status?.StartedAt is { } startedAt && _status.CompletedAt is { } completedAt)
        {
            var elapsedTime = Format.FormatTimeSpan(startedAt.LocalDateTime, completedAt.LocalDateTime);
            html.Append(HtmlTag.Div,
                div => div
                    .WithCssClass("jj-import-summary__duration")
                    .AppendText(_stringLocalizer["Process performed on {0}", elapsedTime]));
        }

        html.AppendDiv(div =>
        {
            div.WithCssClass("text-center");
            AppendCount(div, "lblInsert", BootstrapHelper.LabelSuccess, "Inserted:", _result?.Inserted ?? 0);
            AppendCount(div, "lblUpdate", BootstrapHelper.LabelSuccess, "Updated:", _result?.Updated ?? 0);
            AppendCount(div, "lblDelete", BootstrapHelper.LabelDefault, "Deleted:", _result?.Deleted ?? 0);
            AppendCount(div, "lblIgnore", BootstrapHelper.LabelWarning, "Ignored:", _result?.Ignored ?? 0);
            AppendCount(div, "lblError", BootstrapHelper.LabelDanger, "Errors:", _result?.Errors ?? 0);
        });

        return html;
    }

    private void AppendCount(HtmlBuilder html, string id, string cssClass, string label, int value)
    {
        html.Append(HtmlTag.Span, span =>
        {
            span.WithCssClass(cssClass)
                .WithCssClass("me-1")
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
            .WithCssClass("jj-import-details")
            .Append(HtmlTag.Div, metadata =>
            {
                metadata.WithCssClass("jj-import-details__metadata");
                AppendDetail(metadata, "Start:", startDate.ToString(CultureInfo.CurrentCulture));
                AppendDetail(metadata, "End:", endDate.ToString(CultureInfo.CurrentCulture));
            });

        var errors = _result?.ErrorMessages ?? [];
        if (errors.Count > 0)
        {
            content.Append(HtmlTag.Div, errorSection =>
            {
                errorSection.WithCssClass("jj-import-details__errors")
                    .Append(HtmlTag.Div, header => header
                        .WithCssClass("jj-import-details__errors-header")
                        .AppendComponent(new JJIcon(FontAwesomeIcon.ExclamationTriangle))
                        .Append(HtmlTag.Strong, title => title
                            .AppendText(_stringLocalizer["Errors:"])
                            .AppendText(" ")
                            .AppendText(errors.Count.ToString("N0"))));

                errorSection.Append(HtmlTag.Ul, list =>
                {
                    list.WithCssClass("jj-import-details__error-list");
                    foreach (var error in errors)
                        list.Append(HtmlTag.Li, item => item.AppendText(error));
                });
            });
        }

        return new JJCollapsePanel
        {
            Title = _stringLocalizer["Importation Details"],
            TitleIcon = new JJIcon(FontAwesomeIcon.Film),
            ExpandedByDefault = true,
            Content = content
        }.GetHtmlBuilder();
    }

    private void AppendDetail(HtmlBuilder html, string label, string value, bool monospace = false)
    {
        html.Append(HtmlTag.Div, item => item
            .WithCssClass("jj-import-details__item")
            .Append(HtmlTag.Span, text => text
                .WithCssClass("jj-import-details__label")
                .AppendText(_stringLocalizer[label]))
            .Append(HtmlTag.Strong, text => text
                .WithCssClass(monospace ? "jj-import-details__value jj-import-details__value--monospace" :
                    "jj-import-details__value")
                .AppendText(value)));
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
