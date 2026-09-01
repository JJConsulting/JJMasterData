using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exceptions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Importation.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Importation.Background;

internal sealed class ImportJobHandler(
    IDataDictionaryRepository dataDictionaryRepository,
    ImportFormatCatalog formats,
    FormService formService,
    ExpressionsService expressionsService,
    IEntityRepository entityRepository,
    FieldValuesService fieldValuesService,
    IFileStorage fileStorage,
    IFormEventHandlerResolver formEventHandlerResolver,
    IStringLocalizer<MasterDataResources> localizer,
    ILogger<ImportJobHandler> logger) : BackgroundJobHandler<ImportRequest>
{
    public override async Task<object?> ExecuteAsync(
        ImportRequest request,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(request.ElementName) ??
                          throw new InvalidOperationException($"Element '{request.ElementName}' was not found.");
        var reader = formats.Resolve(request.FormatId, request.FileName, request.ContentType);
        var dataContext = new DataContext(
            DataContextSource.Upload, request.UserId, request.IpAddress, request.BrowserInfo);
        await ConfigureFormEventsAsync(formElement, dataContext);

        var errors = new List<string>();
        var result = new ImportCounters();
        try
        {
            var fields = GetImportFields(formElement);
            var defaultValues = await fieldValuesService.GetDefaultValuesAsync(
                formElement, new FormStateData(new(), new Dictionary<string, object?>(request.UserValues), PageState.Import));
            var formState = new FormStateData(defaultValues, new Dictionary<string, object?>(request.UserValues), PageState.Import);
            await ExecuteCommandAsync(request.CommandBeforeProcess, formElement, formState);

            await using var stream = await fileStorage.OpenReadAsync(request.FilePath, cancellationToken);
            var importContext = new ImportContext(formElement, request.FileName, request.ContentType);
            await foreach (var record in reader.ReadAsync(importContext, request.FormatOptions, stream, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.TotalProcessed++;
                if (record.Values.Count == 0 || record.Values.All(string.IsNullOrWhiteSpace))
                {
                    result.Ignored++;
                    continue;
                }

                if (record.Values.Count != fields.Count)
                {
                    result.Errors++;
                    errors.Add(localizer["Row {0}: Invalid number of fields. Expected {1} Received {2}.",
                        record.RowNumber, fields.Count, record.Values.Count]);
                    continue;
                }

                if (record.RowNumber == 1 && IsHeader(fields, record))
                {
                    result.Ignored++;
                    continue;
                }

                try
                {
                    var values = GetValues(fields, record);
                    foreach (var relation in request.RelationValues)
                        values[relation.Key] = relation.Value;
                    var merged = await fieldValuesService.MergeWithExpressionValuesAsync(
                        formElement,
                        new FormStateData(values, new Dictionary<string, object?>(request.UserValues), PageState.Import));
                    var formLetter = await formService.InsertOrReplaceAsync(formElement, merged, dataContext);
                    ProcessResult(formLetter, result, errors, record.RowNumber);
                }
                catch (FormValuesException exception)
                {
                    result.Errors++;
                    errors.Add(localizer["Row {0}: Error parsing value [{1}] for field [{2}].",
                        record.RowNumber, exception.Value, exception.Field.Name]);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogError(exception, "Error processing row {Row} of {Element}", record.RowNumber, formElement.Name);
                    result.Errors++;
                    errors.Add($"Row {record.RowNumber}: {localizer.GetExceptionMessage(exception)}");
                }

                progress.Report(new BackgroundJobProgress(0,
                    localizer["Importing {0} records...", result.TotalProcessed.ToString("N0")],
                    result.ToResult(errors)));
            }

            await ExecuteCommandAsync(request.CommandAfterProcess, formElement, formState);
            var finalResult = result.ToResult(errors);
            progress.Report(new BackgroundJobProgress(100,
                result.Errors > 0 ? localizer["File imported with errors!"] : localizer["File imported successfully!"],
                finalResult));
            return finalResult;
        }
        finally
        {
            await fileStorage.DeleteAsync(request.FilePath, CancellationToken.None);
        }
    }

    private async Task ConfigureFormEventsAsync(FormElement formElement, DataContext dataContext)
    {
        var handler = formEventHandlerResolver.GetFormEventHandler(formElement.Name);
        if (handler is null)
            return;
        await handler.OnFormElementLoadAsync(dataContext, new FormElementLoadEventArgs(formElement));
        formService.OnBeforeImportAsync += handler.OnBeforeImportAsync;
        formService.OnAfterInsertAsync += handler.OnAfterInsertAsync;
        formService.OnAfterUpdateAsync += handler.OnAfterUpdateAsync;
        formService.OnAfterDeleteAsync += handler.OnAfterDeleteAsync;
    }

    private List<FormElementField> GetImportFields(FormElement formElement)
    {
        var state = new FormStateData(PageState.Import);
        return formElement.Fields.Where(field =>
            expressionsService.GetBoolValue(field.VisibleExpression, state) &&
            field.DataBehavior is FieldBehavior.Real or FieldBehavior.WriteOnly).ToList();
    }

    private bool IsHeader(List<FormElementField> fields, ImportRecord record)
    {
        var expected = localizer[fields[0].LabelOrName].Value;
        return string.Equals(expected.Trim(), record.Values[0]?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static Dictionary<string, object?> GetValues(List<FormElementField> fields, ImportRecord record)
    {
        var values = new Dictionary<string, object?>(fields.Count, StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < fields.Count; index++)
            FormValuesService.HandleFieldValue(fields[index], values, record.Values[index]);
        return values;
    }

    private static void ProcessResult(
        FormLetter<CommandOperation> formLetter,
        ImportCounters counters,
        List<string> errors,
        long rowNumber)
    {
        if (!formLetter.IsValid)
        {
            counters.Errors++;
            errors.Add($"Row {rowNumber}: {string.Join(Environment.NewLine, formLetter.Errors.Values)}");
            return;
        }
        switch (formLetter.Result)
        {
            case CommandOperation.Insert: counters.Inserted++; break;
            case CommandOperation.Update: counters.Updated++; break;
            case CommandOperation.Delete: counters.Deleted++; break;
            default: counters.Ignored++; break;
        }
    }

    private async Task ExecuteCommandAsync(string? command, FormElement formElement, FormStateData formState)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;
        var parsed = expressionsService.ReplaceExpressionWithParsedValues(command, formState);
        await entityRepository.SetCommandAsync(new DataAccessCommand(parsed!), formElement.ConnectionId);
    }

    private sealed class ImportCounters
    {
        public long TotalProcessed { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Deleted { get; set; }
        public int Ignored { get; set; }
        public int Errors { get; set; }

        public ImportJobResult ToResult(List<string> errors) => new()
        {
            TotalProcessed = TotalProcessed,
            Inserted = Inserted,
            Updated = Updated,
            Deleted = Deleted,
            Ignored = Ignored,
            Errors = Errors,
            ErrorMessages = [.. errors]
        };
    }
}
