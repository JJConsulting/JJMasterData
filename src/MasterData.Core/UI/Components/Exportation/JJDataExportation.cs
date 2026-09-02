#nullable disable warnings

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Security;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Background;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Exporta dados para um arquivo
/// </summary>
public class JJDataExportation : ProcessComponent
{
    #region "Events"

    /// <summary>
    /// Event fired when the cell is rendered.
    /// </summary>
    #endregion

    #region "Properties"

    private ExportOptions _exportOptions;

    public ExportOptions ExportOptions
    {
        get => _exportOptions ??= new ExportOptions();
        internal set => _exportOptions = value;
    }
    
    internal MasterDataCoreOptions MasterDataOptions { get; }

    internal DataExportationScripts Scripts => field ??= new DataExportationScripts(this);
    internal IComponentFactory ComponentFactory { get; }
    internal IFileStorage FileStorage { get; }
    public IUrlHelper UrlHelper { get; }
    internal ExportJobService ExportJobService { get; }
    internal ExportFormatCatalog ExportFormatCatalog { get; }

    #endregion

    #region "Constructors"
    internal JJDataExportation(
        FormElement formElement,
        IMasterDataUser masterDataUser,
        IUrlHelper urlHelper,
        ExpressionsService expressionsService,
        IOptionsSnapshot<MasterDataCoreOptions> masterDataOptions,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor, 
        DataProtectionService dataProtectionService,
        IFileStorage fileStorage,
        ExportJobService exportJobService,
        ExportFormatCatalog exportFormatCatalog) :
        base(httpContextAccessor, masterDataUser, expressionsService, loggerFactory.CreateLogger<ProcessComponent>(),dataProtectionService,stringLocalizer)
    {
        FileStorage = fileStorage;
        UrlHelper = urlHelper;
        ExportJobService = exportJobService;
        ExportFormatCatalog = exportFormatCatalog;
        ComponentFactory = componentFactory;
        HttpContextAccessor = httpContextAccessor;
        MasterDataOptions = masterDataOptions.Value;
        FormElement = formElement;
    }
    #endregion
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var currentJob = ExportJobService.GetCurrentStatus(FormElement.Name, UserId);
        if (currentJob is null)
            return new ContentComponentResult(await new DataExportationSettings(this).GetHtmlBuilderAsync());

        var html = new DataExportationLog(this).GetLoadingHtml();
        html.AppendHiddenInput($"{Name}-export-job-id", currentJob.Id.ToString());
        return new ContentComponentResult(html);
    }

    internal bool IsRunning() => ExportJobService.GetCurrentStatus(FormElement.Name, UserId) is not null;

    internal static JJIcon GetFileIcon(string ext)
    {
        if (ext.EndsWith("xls"))
            return new JJIcon(FontAwesomeIcon.FileExcelO);
        if (ext.EndsWith("pdf"))
            return new JJIcon(FontAwesomeIcon.FilePdfO);
        return new JJIcon(FontAwesomeIcon.FileTextO);
    }

    internal string GetDownloadUrl(string fileName)
    {
        return UrlHelper.ActionLink("Exportation", "File", new { Area = "MasterData", elementName = FormElement.Name, fileName });
    }

    private string GetFinishedMessageHtml(ExportJobStatus status)
    {
        if (status.State == BackgroundJobState.Succeeded && status.Result is not null)
        {
            string url = GetDownloadUrl(status.Result.FileName);
            var html = new HtmlBuilder(HtmlTag.Div);

            {
                var icon = GetFileIcon(Path.GetExtension(status.Result.FileName));
                icon.CssClass = "fa-3x ";

                html.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("text-center");
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Span, span =>
                    {
                        span.WithCssClass("text-success");
                        span.Append(HtmlTag.Span, span =>
                        {
                            span.WithCssClass("fa fa-check fa-lg");
                            span.WithAttribute("aria-hidden", "true");
                        });

                        span.AppendText(StringLocalizer["File generated successfully!"]);
                    });
                    div.Append(HtmlTag.Br);

                    string elapsedTime = status.StartedAt.HasValue && status.CompletedAt.HasValue
                        ? Format.FormatTimeSpan(status.StartedAt.Value.DateTime, status.CompletedAt.Value.DateTime)
                        : string.Empty;

                    div.AppendText(StringLocalizer["Process performed on {0}", elapsedTime]);

                    div.Append(HtmlTag.Br);

                    div.Append(HtmlTag.I, i =>
                    {
                        i.AppendText(
                            StringLocalizer["If the download does not start automatically, click on the icon below."]);
                    });

                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Br);

                    div.Append(HtmlTag.A, a =>
                    {
                        a.WithAttribute("id", $"export_link_{Name}");
                        a.WithAttribute("href", url);
                        a.AppendComponent(icon);
                        a.Append(HtmlTag.Br);
                        a.AppendText(status.Result.FileName);
                    });
                    div.Append(HtmlTag.Br);
                    div.Append(HtmlTag.Br);
                });
            }

            var btnCancel = new JJLinkButton();
            btnCancel.Text = StringLocalizer["Close"];
            btnCancel.IconClass = "fa fa-times";
            btnCancel.ShowAsButton = true;
            btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

            html.Append(HtmlTag.Hr);

            html.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("row");
                div.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass($"col-sm-12 {BootstrapHelper.TextRight}");
                    div.AppendComponent(btnCancel);
                });
            });

            return html.ToString();
        }

        var alert = new JJAlert
        {
            Title = status.State == BackgroundJobState.Cancelled
                ? StringLocalizer["Process aborted by user"]
                : status.Error ?? StringLocalizer["Unexpected error"],
            Icon = FontAwesomeIcon.Warning,
            Color = status.State == BackgroundJobState.Cancelled ? BootstrapColor.Warning : BootstrapColor.Danger
        };

        return alert.GetHtml();
    }

    internal ValueTask<Guid> ExportFileInBackground(
        Dictionary<string, object?> filter,
        OrderByData orderByData,
        List<Dictionary<string, object?>>? rows = null)
    {
        var request = new ExportRequest
        {
            ElementName = FormElement.Name,
            UserId = UserId,
            FormatId = ExportOptions.FormatId,
            IncludeHeader = ExportOptions.ExportFirstLine,
            ExportAllFields = ExportOptions.ExportAllFields,
            FormatOptions = ExportOptions.FormatOptions,
            Filters = filter,
            OrderBy = orderByData.ToQueryParameter(),
            UserValues = UserValues,
            Rows = rows,
            BaseUri = HttpContextAccessor.HttpContext?.Request.GetAbsoluteUri()
        };
        return ExportJobService.EnqueueAsync(request, HttpContextAccessor.HttpContext?.RequestAborted ?? default);
    }

    internal DataExportationProgressDto GetCurrentProgress(Guid jobId)
    {
        var status = ExportJobService.GetStatus(jobId, UserId);
        var dto = new DataExportationProgressDto();
        if (status != null)
        {
            dto.Message = status.Progress?.Message ?? StringLocalizer["Waiting..."];
            dto.HasError = status.State == BackgroundJobState.Failed;
            dto.StartDate = status.CreatedAt.LocalDateTime.ToDateTimeString();
            dto.PercentProcess = status.Progress?.Percentage ?? 0;
            dto.IsProcessing = status.State is BackgroundJobState.Queued or BackgroundJobState.Running;
            if (!dto.IsProcessing)
                dto.FinishedMessage = GetFinishedMessageHtml(status);
        }
        else
        {
            dto.Message = StringLocalizer["Background job was not found."];
            dto.StartDate = DateTime.Now.ToShortDateString();
            dto.HasError = true;
            dto.FinishedMessage = new JJAlert
            {
                Title = dto.Message,
                Icon = FontAwesomeIcon.Warning,
                Color = BootstrapColor.Danger
            }.GetHtml();
        }

        return dto;
    }

    internal bool Cancel(Guid jobId) => ExportJobService.Cancel(jobId, UserId);
}
