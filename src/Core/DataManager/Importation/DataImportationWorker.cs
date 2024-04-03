using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exceptions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Importation;

public class DataImportationWorker(
    DataImportationContext context,
    FormService formService,
    ExpressionsService expressionsService,
    IEntityRepository entityRepository,
    FieldValuesService fieldValuesService,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<DataImportationWorker> logger)
    : IBackgroundTaskWorker
{
    #region "Events"

    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterProcessAsync;
    public event EventHandler<IProgressReporter> OnProgressChanged;

    #endregion
    
    public string UserId { get; set; }

    internal Dictionary<string, object> UserValues { get; set; } = new();
    
    public ProcessOptions ProcessOptions { get; set; }

    private CultureInfo Culture { get; set; } = Thread.CurrentThread.CurrentUICulture;

    public FormElement FormElement { get; } = context.FormElement;
    private DataContext DataContext { get; } = context.DataContext;

    private string RawData { get; } = context.RawData;
    private char Separator { get; } = context.Separator;

    private Dictionary<string, object> RelationValues { get; } = context.RelationValues;

    private ExpressionsService ExpressionsService { get; } = expressionsService;

    private IEntityRepository EntityRepository { get; } = entityRepository;

    private FieldValuesService FieldValuesService { get; } = fieldValuesService;

    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    private ILogger<DataImportationWorker> Logger { get; } = logger;

    internal FormService FormService { get; } = formService;

#if NETFRAMEWORK
    private System.Web.HttpContext HttpContext { get; } = System.Web.HttpContext.Current;
#endif
    public async Task RunWorkerAsync(CancellationToken token)
    {
#if NETFRAMEWORK
            System.Web.HttpContext.Current = HttpContext;
#endif
        var currentProcess = new DataImportationReporter(StringLocalizer);
        try
        {
            currentProcess.StartDate = DateTime.Now;
            currentProcess.UserId = UserId;
            Reporter(currentProcess);
            await RunWorker(currentProcess, token);

            if (currentProcess.Error > 0)
                currentProcess.Message = StringLocalizer["File imported with errors!"];
            else
                currentProcess.Message = StringLocalizer["File imported successfully!"];
        }
        catch (Exception ex)
        {
            currentProcess.HasError = true;
            if (ex is OperationCanceledException or ThreadAbortException)
            {
                currentProcess.Message = StringLocalizer["Process aborted by user"];
                currentProcess.AddError(currentProcess.Message);
            }
            else if (ex is JJMasterDataException)
            {
                currentProcess.Message = ex.Message;
                currentProcess.AddError(currentProcess.Message);
            }
            else
            {
                currentProcess.Message = StringLocalizer["Unexpected error"];
                currentProcess.Message += " ";
                currentProcess.Message += StringLocalizer[ExceptionManager.GetMessage(ex)];
                currentProcess.AddError(currentProcess.Message);
                Logger.LogError(ex, "Error while importing file");
                throw;
            }
        }
        finally
        {
            currentProcess.EndDate = DateTime.Now;
            Reporter(currentProcess);
        }
    }

    private void Reporter(DataImportationReporter reporter)
    {
        OnProgressChanged?.Invoke(this, reporter);
    }

    private async Task RunWorker(DataImportationReporter currentProcess, CancellationToken token)
    {
        Thread.CurrentThread.CurrentUICulture = Culture;
        Thread.CurrentThread.CurrentCulture = Culture;

        //recuperando campos a serem importados
        var fieldList = GetImportationFields();
        
        string[] stringSeparators = ["\r\n"];
        string[] rows = RawData.Split(stringSeparators, StringSplitOptions.None);
        currentProcess.TotalRecords = rows.Length;
        currentProcess.Message =
            StringLocalizer["Importing {0} records...", currentProcess.TotalRecords.ToString("N0")];

        var defaultValues = await FieldValuesService.GetDefaultValuesAsync(FormElement, new FormStateData
        {
            Values = new Dictionary<string, object>(),
            PageState = PageState.Import,
            UserValues = UserValues
        });
        var formStateData = new FormStateData(defaultValues, UserValues, PageState.Import);
        
        //Execute before process events
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandBeforeProcess))
        {
            var parsedSql =
                ExpressionsService.ReplaceExpressionWithParsedValues(ProcessOptions.CommandBeforeProcess,
                    formStateData);
            await EntityRepository.SetCommandAsync(new DataAccessCommand(parsedSql!));
        }

        token.ThrowIfCancellationRequested();
        for (var index = 0; index < rows.Length; index++)
        {
            var line = rows[index];
            currentProcess.TotalProcessed++;
            
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            if (string.IsNullOrWhiteSpace(line.Replace(Separator.ToString(), string.Empty)))
            {
                currentProcess.AddError(StringLocalizer["Empty line ignored"]);
                currentProcess.Ignore++;
                continue;
            }

            var cols = line.Split(Separator);

            //Check if the row count is valid.
            if (cols.Length != fieldList.Count)
            {
                currentProcess.Error++;

                var errorBuilder = new StringBuilder();
                errorBuilder.Append(StringLocalizer["Invalid number of fields."]);
                errorBuilder.Append(' ');
                errorBuilder.Append(StringLocalizer["Expected {0} Received {1}.", fieldList.Count, cols.Length]);
                currentProcess.AddError(errorBuilder.ToString());

                errorBuilder.Append(StringLocalizer["Click on the Help button for more information regarding the file layout."]);
                throw new JJMasterDataException(errorBuilder.ToString());
            }

            //Verify if it's first row.
            if (index == 0)
            {
                string firstColumnName = StringLocalizer[fieldList[0].LabelOrName];

                //Validate if the first column is the same label of the FormElement. Is there a better way to do this?
                if (string.Equals(firstColumnName.Trim(), cols[0].Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    currentProcess.AddError(StringLocalizer["File header ignored"]);
                    currentProcess.Ignore++;
                    continue;
                }
            }

            try
            {
                var values = GetDictionaryWithNameAndValue(fieldList, cols);
                var formLetter = await SaveRowValues(values);
                ProcessFormLetter(currentProcess, formLetter);
            }
            catch (FormValuesException fvEx)
            {
                currentProcess.Error++;
                currentProcess.AddError(StringLocalizer["Error parsing value [{0}] for field [{1}].",fvEx.Value, fvEx.Field.Name]);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Error while processing line {Line} of {FormElement} at Data Importation.",index, FormElement.Name);
                currentProcess.Error++;
                currentProcess.AddError(StringLocalizer[ExceptionManager.GetMessage(exception)]);
            }

            Reporter(currentProcess);
            token.ThrowIfCancellationRequested();
        }

        //Execute success events
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandAfterProcess))
        {
            var parsedSql =
                ExpressionsService.ReplaceExpressionWithParsedValues(ProcessOptions.CommandAfterProcess, formStateData);
            await EntityRepository.SetCommandAsync(new DataAccessCommand(parsedSql!));
        }

        if (OnAfterProcessAsync != null)
            await OnAfterProcessAsync(this, new FormAfterActionEventArgs());
    }

    private static void ProcessFormLetter(DataImportationReporter currentProcess, FormLetter<CommandOperation> formLetter)
    {
        if (formLetter.IsValid)
        {
            switch (formLetter.Result)
            {
                case CommandOperation.Insert:
                    currentProcess.Insert++;
                    break;
                case CommandOperation.Update:
                    currentProcess.Update++;
                    break;
                case CommandOperation.Delete:
                    currentProcess.Delete++;
                    break;
                case CommandOperation.None:
                default:
                    currentProcess.Ignore++;
                    break;
            }
        }
        else
        {
            currentProcess.Error++;
            var errorBuilder = new StringBuilder();
            foreach (var error in formLetter.Errors)
            {
                if (errorBuilder.Length > 0)
                    errorBuilder.AppendLine();

                errorBuilder.Append(error.Value);
            }

            currentProcess.AddError(errorBuilder.ToString());
        }
    }

    private static Dictionary<string, object> GetDictionaryWithNameAndValue(
        List<FormElementField> fieldList,
        string[] cols)
    {
        var values = new Dictionary<string, object>(fieldList.Count);
        for (var i = 0; i < fieldList.Count; i++)
        {
            FormValuesService.HandleFieldValue(fieldList[i], values, cols[i]);
        }

        return values;
    }
    
    private List<FormElementField> GetImportationFields()
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var defaultValues = new Dictionary<string, object>();
        var formData = new FormStateData(defaultValues, PageState.Import);
        var list = new List<FormElementField>();
        foreach (var field in FormElement.Fields)
        {
            var visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
            if (visible && field.DataBehavior is FieldBehavior.Real or FieldBehavior.WriteOnly)
                list.Add(field);
        }

        return list;
    }
    
    private async Task<FormLetter<CommandOperation>> SaveRowValues(Dictionary<string, object> fileValues)
    {
        DataHelper.CopyIntoDictionary(fileValues, RelationValues);
        
        var values = await FieldValuesService.MergeWithExpressionValuesAsync(FormElement,
            new FormStateData(fileValues, UserValues, PageState.Import));

        return await FormService.InsertOrReplaceAsync(FormElement, values, DataContext);
    }
}