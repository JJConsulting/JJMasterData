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

    public ProcessOptions ProcessOptions { get; set; }

    private CultureInfo Culture { get; set; } = Thread.CurrentThread.CurrentUICulture;

    public FormElement FormElement { get; } = context.FormElement;
    private DataContext DataContext { get; } = context.DataContext;

    private string RawData { get; } = context.RawData;
    private char Separator { get; } = context.Separator;
    
    private IDictionary<string, object> RelationValues { get; } = context.RelationValues;

    private ExpressionsService ExpressionsService { get; } = expressionsService;

    private IEntityRepository EntityRepository { get; } = entityRepository;

    private FieldValuesService FieldValuesService { get; } = fieldValuesService;

    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    private ILogger<DataImportationWorker> Logger { get; } = logger;

    internal FormService FormService { get; } = formService;

#if NETFRAMEWORK
    private System.Web.HttpContext HttpContext { get; } = System.Web.HttpContext.Current;
#endif
    public Task RunWorkerAsync(CancellationToken token)
    {
        return Task.Run(async () =>
        {
#if NETFRAMEWORK
            System.Web.HttpContext.Current = HttpContext;
#endif
            var currentProcess = new DataImportationReporter();
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
                    currentProcess.Message += ExceptionManager.GetMessage(ex);
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
        }, token);
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
        var fieldList = GetListImportedField();

        string[] stringSeparators = ["\r\n"];
        string[] rows = RawData.Split(stringSeparators, StringSplitOptions.None);
        currentProcess.TotalRecords = rows.Length;
        currentProcess.Message =
            StringLocalizer["Importing {0} records...", currentProcess.TotalRecords.ToString("N0")];

        var defaultValues = await FieldValuesService.GetDefaultValuesAsync(FormElement, new FormStateData()
        {
            Values = new Dictionary<string, object>(),
            PageState = PageState.Import,
            UserValues = UserValues
        });
        var formStateData = new FormStateData(defaultValues, UserValues, PageState.Import);

        //executa script antes da execuçao
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandBeforeProcess))
        {
            var parsedSql =
                ExpressionsService.ReplaceExpressionWithParsedValues(ProcessOptions.CommandBeforeProcess,
                    formStateData);
            await EntityRepository.SetCommandAsync(new DataAccessCommand(parsedSql));
        }

        token.ThrowIfCancellationRequested();
        bool isFirstRow = true;
        foreach (string line in rows)
        {
            currentProcess.TotalProcessed++;

            //Ignora linhas em branco
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            //Ignora conteudo vazio
            if (string.IsNullOrWhiteSpace(line.Replace(Separator.ToString(), string.Empty)))
            {
                currentProcess.AddError(StringLocalizer["Empty line ignored"]);
                currentProcess.Ignore++;
                continue;
            }

            //Verifica quantidade de colunas do arquivo
            string[] cols = line.Split(Separator);
            if (cols.Length != fieldList.Count)
            {
                currentProcess.Error++;

                string error = string.Empty;
                error += StringLocalizer["Invalid number of fields."];
                error += " ";
                error += StringLocalizer["Expected {0} Received {1}.", fieldList.Count, cols.Length];
                currentProcess.AddError(error);

                error += StringLocalizer["Click on the Help button for more information regarding the file layout."];
                throw new JJMasterDataException(error);
            }

            //Analisa se ignora a primeira linha do arquivo
            if (isFirstRow)
            {
                isFirstRow = false;
                string colName1 = fieldList[0].LabelOrName;
                if (colName1.Trim().ToLower().Equals(cols[0].Trim().ToLower()))
                {
                    currentProcess.AddError(StringLocalizer["File header ignored"]);
                    currentProcess.Ignore++;
                    continue;
                }
            }

            var values = GetDictionaryWithNameAndValue(fieldList, cols);
            await SetFormValues(values, currentProcess);
            Reporter(currentProcess);
            token.ThrowIfCancellationRequested();
        }

        //executa script após a execução
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandAfterProcess))
        {
            string parsedSql =
                ExpressionsService.ReplaceExpressionWithParsedValues(ProcessOptions.CommandAfterProcess, formStateData);
            await EntityRepository.SetCommandAsync(new DataAccessCommand(parsedSql!));
        }

        if (OnAfterProcessAsync != null)
            await OnAfterProcessAsync(this, new FormAfterActionEventArgs());
    }

    internal IDictionary<string, object> UserValues { get; set; } = new Dictionary<string, object>();
    
    
    private static Dictionary<string, object> GetDictionaryWithNameAndValue(IReadOnlyList<FormElementField> listField,
        string[] cols)
    {
        var values = new Dictionary<string, object>();
        for (int i = 0; i < listField.Count; i++)
        {
            var field = listField[i];
            var value = cols[i];
            FormValuesService.HandleFieldValue(field, values, value);
        }

        return values;
    }

    /// <summary>
    /// Lista de campos a serem importados
    /// </summary>
    private List<FormElementField> GetListImportedField()
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var defaultValues = new Dictionary<string, object>();
        var formData = new FormStateData(defaultValues, PageState.Import);
        var list = new List<FormElementField>();
        foreach (var field in FormElement.Fields)
        {
            bool visible = ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
            if (visible && field.DataBehavior == FieldBehavior.Real)
                list.Add(field);
        }

        return list;
    }

    /// <summary>
    /// Atualiza os valores no banco de dados. 
    /// Retorna lista de erros
    /// </summary>
    /// <returns>Retorna lista de erros</returns>
    private async Task SetFormValues(IDictionary<string, object> fileValues, DataImportationReporter currentProcess)
    {
        try
        {
            DataHelper.CopyIntoDictionary(fileValues, RelationValues);
            var values = await FieldValuesService.MergeWithExpressionValuesAsync(FormElement,
                new FormStateData(fileValues, UserValues, PageState.Import));
            
            var formLetter = await FormService.InsertOrReplaceAsync(FormElement, values, DataContext);

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
                    default:
                        currentProcess.Ignore++;
                        break;
                }
            }
            else
            {
                currentProcess.Error++;
                var sErr = new StringBuilder();
                foreach (var err in formLetter.Errors)
                {
                    if (sErr.Length > 0)
                        sErr.AppendLine("");

                    sErr.Append(err.Value);
                }

                currentProcess.AddError(sErr.ToString());
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ThreadAbortException)
        {
            throw;
        }
        catch (Exception ex)
        {
            currentProcess.Error++;
            currentProcess.AddError(ExceptionManager.GetMessage(ex));
        }
    }
}