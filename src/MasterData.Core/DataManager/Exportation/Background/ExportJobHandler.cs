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
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation.Background;

internal sealed class ExportJobHandler(
    IEntityRepository entityRepository,
    ExpressionsService expressionsService,
    ExportFormatCatalog formats,
    IFileStorage fileStorage,
    IOptions<MasterDataCoreOptions> options,
    IStringLocalizer<MasterDataResources> localizer,
    IMasterDataUser masterDataUser) : BackgroundJobHandler<ExportRequest>
{
    private const int RecordsPerPage = 10_000;

    public override async Task<object?> ExecuteAsync(
        ExportRequest request,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken)
    {
        using var cultureScope = CultureScope.Create(request.CultureName, request.UICultureName);
        masterDataUser.Id = request.UserId;
        var formElement = request.FormElement;
        var format = formats.GetRequired(request.FormatId);
        var columns = GetColumns(request.FormElement, request.ExportAllFields);
        List<Dictionary<string, object?>> firstPage;
        long totalRecords;
        EntityParameters? parameters = null;
        if (request.Rows is not null)
        {
            firstPage = request.Rows;
            totalRecords = request.Rows.Count;
        }
        else
        {
            parameters = new EntityParameters
            {
                Filters = new Dictionary<string, object?>(request.Filters),
                RecordsPerPage = RecordsPerPage,
                OrderBy = OrderByData.FromString(request.OrderBy),
                CurrentPage = 1
            };
            var result = await entityRepository.GetDictionaryListResultAsync(formElement, parameters);
            cancellationToken.ThrowIfCancellationRequested();
            firstPage = result.Data;
            totalRecords = result.TotalOfRecords;
        }

        var context = new ExportContext
        {
            FormElement = formElement,
            Columns = columns,
            Rows = GetRowsAsync(formElement, firstPage, totalRecords, parameters, cancellationToken),
            UserValues = new Dictionary<string, object?>(request.UserValues),
            TotalRecords = totalRecords,
            Progress = new Progress<ExportProgress>(current => progress.Report(
                new BackgroundJobProgress(current.Percentage, current.Message, current)))
        };

        progress.Report(new BackgroundJobProgress(0, localizer["Retrieving records..."]));
        var tempFile = Path.GetTempFileName();
        try
        {
            await using (var output = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None,
                             81920, true))
            {
                var definitions = ExportFormatOptionsMetadataFactory.CreateOptions(format);
                var typedOptions = ExportFormatOptionsBinder.Bind(definitions, format.OptionsType, request.OptionsValues);                
                await format.WriteAsync(context, typedOptions, output, cancellationToken);     
            }
                

            var fileName = GetFileName(formElement, format.FileExtension);
            var folder = DataExportationHelper.GetExportationFolderPath(
                formElement, options.Value.ExportationFolderPath, request.UserId);
            var storagePath = FileStoragePath.Combine(folder, fileName);
            await using var input = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
            await fileStorage.SaveAsync(storagePath, input, true, cancellationToken);
            progress.Report(new BackgroundJobProgress(100, localizer["File generated successfully!"]));
            return fileName;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private List<FormElementField> GetColumns(FormElement formElement, bool exportAllFields)
    {
        var formState = new FormStateData(new Dictionary<string, object?>(), PageState.List);
        return formElement.Fields
            .Where(field => field.Export && (exportAllFields || expressionsService.GetBoolValue(field.VisibleExpression, formState)))
            .Select(field =>
            {
                var exportField = field.DeepCopy();
                exportField.Label = string.IsNullOrEmpty(field.Label) ? field.Name : localizer[field.Label];
                return exportField;
            })
            .ToList();
    }

    private async IAsyncEnumerable<Dictionary<string, object?>> GetRowsAsync(
        FormElement formElement,
        List<Dictionary<string, object?>> firstPage,
        long totalOfRecords,
        EntityParameters? parameters,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var totalPages = parameters is null ? 1 :
            (int)Math.Ceiling(totalOfRecords / (double)RecordsPerPage);

        for (var page = 1; page <= Math.Max(1, totalPages); page++)
        {
            List<Dictionary<string, object?>> rows;
            if (page == 1)
                rows = firstPage;
            else
            {
                var pageParameters = new EntityParameters
                {
                    Filters = parameters!.Filters,
                    RecordsPerPage = parameters.RecordsPerPage,
                    OrderBy = parameters.OrderBy,
                    CurrentPage = page
                };
                var result = await entityRepository.GetDictionaryListResultAsync(
                    formElement, pageParameters, recoverTotalOfRecords: false);
                rows = result.Data;
            }

            foreach (var sourceRow in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return sourceRow;
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
}
