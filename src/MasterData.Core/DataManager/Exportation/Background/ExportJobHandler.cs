using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation.Background;

internal sealed class ExportJobHandler(
    IDataDictionaryRepository dataDictionaryRepository,
    IEntityRepository entityRepository,
    ExpressionsService expressionsService,
    ExportFormatCatalog formats,
    IFileStorage fileStorage,
    IOptions<MasterDataCoreOptions> options,
    IStringLocalizer<MasterDataResources> localizer) : BackgroundJobHandler<ExportRequest>
{
    private const int RecordsPerPage = 100_000;

    public override async Task<object?> ExecuteAsync(
        ExportRequest request,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(request.ElementName) ??
                          throw new InvalidOperationException($"Element '{request.ElementName}' was not found.");
        var format = formats.GetRequired(request.FormatId);
        var columns = GetColumns(formElement, request.ExportAllFields);
        var source = await CreateSourceAsync(formElement, request, cancellationToken);
        long processed = 0;

        var exportProgress = new MasterDataProgress<ExportProgress>(current => progress.Report(
            new BackgroundJobProgress(current.Percentage, current.Message, current)));
        var context = new ExportContext
        {
            FormElement = formElement,
            Columns = columns,
            Rows = GetRowsAsync(
                formElement, source, value => processed = value, cancellationToken),
            UserValues = new Dictionary<string, object?>(request.UserValues),
            IncludeHeader = request.IncludeHeader,
            TotalRecords = source.Total,
            Progress = exportProgress
        };

        progress.Report(new BackgroundJobProgress(0, localizer["Retrieving records..."]));
        var tempFile = Path.GetTempFileName();
        try
        {
            await using (var output = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 81920, true))
                await format.WriteAsync(context, request.FormatOptions, output, cancellationToken);

            var fileName = GetFileName(formElement, format.Configuration.FileExtension);
            var folder = DataExportationHelper.GetExportationFolderPath(
                formElement, options.Value.ExportationFolderPath, request.UserId);
            var storagePath = FileStoragePath.Combine(folder, fileName);
            await using var input = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
            await fileStorage.SaveAsync(storagePath, input, true, cancellationToken);
            progress.Report(new BackgroundJobProgress(100, localizer["File generated successfully!"]));
            return new ExportJobResult
            {
                FileName = fileName,
                ContentType = format.Configuration.ContentType,
                StoragePath = storagePath,
                TotalRecords = source.Total ?? processed
            };
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private List<ExportColumn> GetColumns(FormElement formElement, bool exportAllFields)
    {
        var formState = new FormStateData(new Dictionary<string, object?>(), PageState.List);
        return formElement.Fields
            .Where(field => field.Export && (exportAllFields ||
                expressionsService.GetBoolValue(field.VisibleExpression, formState)))
            .Select(field => new ExportColumn(
                field.Name,
                string.IsNullOrEmpty(field.Label) ? field.Name : localizer[field.Label],
                field))
            .ToList();
    }

    private async Task<ExportSource> CreateSourceAsync(
        FormElement formElement,
        ExportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Rows is not null)
            return new ExportSource(request.Rows, request.Rows.Count, null);

        var parameters = new EntityParameters
        {
            Filters = new Dictionary<string, object?>(request.Filters),
            RecordsPerPage = RecordsPerPage,
            OrderBy = OrderByData.FromString(request.OrderBy),
            CurrentPage = 1
        };
        var firstPage = await entityRepository.GetDictionaryListResultAsync(formElement, parameters);
        cancellationToken.ThrowIfCancellationRequested();
        return new ExportSource(
            firstPage.Data,
            firstPage.TotalOfRecords,
            parameters);
    }

    private async IAsyncEnumerable<Dictionary<string, object?>> GetRowsAsync(
        FormElement formElement,
        ExportSource source,
        Action<long> setProcessed,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        long processed = 0;
        var totalPages = source.Parameters is null ? 1 :
            (int)Math.Ceiling((source.Total ?? 0) / (double)RecordsPerPage);

        for (var page = 1; page <= Math.Max(1, totalPages); page++)
        {
            List<Dictionary<string, object?>> rows;
            if (page == 1)
                rows = source.FirstPage;
            else
            {
                var pageParameters = new EntityParameters
                {
                    Filters = source.Parameters!.Filters,
                    RecordsPerPage = source.Parameters.RecordsPerPage,
                    OrderBy = source.Parameters.OrderBy,
                    CurrentPage = page
                };
                var result = await entityRepository.GetDictionaryListResultAsync(formElement, pageParameters);
                rows = result.Data;
            }

            foreach (var sourceRow in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = new Dictionary<string, object?>(sourceRow, StringComparer.OrdinalIgnoreCase);
                processed++;
                setProcessed(processed);
                yield return row;
            }
        }
    }

    private string GetFileName(FormElement formElement, string extension)
    {
        var configuredName = formElement.Options.GridToolbarActions.ExportAction.FileName;
        var name = !string.IsNullOrWhiteSpace(configuredName) ? configuredName :
            !string.IsNullOrWhiteSpace(formElement.Title) ?
                expressionsService.GetExpressionValue(formElement.Title, new FormStateData(PageState.List))?.ToString() :
                formElement.Name;
        name = StringManager.GetStringWithoutAccents(name ?? "file");
        foreach (var invalid in Path.GetInvalidFileNameChars().Concat([' ', '+', '=', '&', '%', '$', '#', '@']))
            name = name.Replace(invalid.ToString(), string.Empty);
        name = HttpUtility.UrlEncode(name, Encoding.UTF8);
        return $"{name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension.TrimStart('.').ToLowerInvariant()}";
    }

    private sealed class ExportSource(
        List<Dictionary<string, object?>> firstPage,
        long? total,
        EntityParameters? parameters)
    {
        public List<Dictionary<string, object?>> FirstPage { get; init; } = firstPage;
        public long? Total { get; init; } = total;
        public EntityParameters? Parameters { get; init; } = parameters;
    }
}
