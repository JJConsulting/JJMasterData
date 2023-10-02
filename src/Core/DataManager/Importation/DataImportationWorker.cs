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
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Importation;

public class DataImportationWorker : IBackgroundTaskWorker
{
    #region "Events"

    public event EventHandler<FormAfterActionEventArgs> OnAfterProcess;
    public event EventHandler<IProgressReporter> OnProgressChanged;

    #endregion

    public string UserId { get; set; }

    public ProcessOptions ProcessOptions { get; set; }

    public CultureInfo Culture { get; set; }

    public FormElement FormElement { get; }
    internal DataContext DataContext { get; }

    public string RawData { get; }
    public char Separator { get;}
    
    internal ExpressionsService ExpressionsService { get; }

    internal IEntityRepository EntityRepository { get; }
    
    internal FieldValuesService FieldValuesService { get; }

    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    internal ILogger<DataImportationWorker> Logger { get; }

    internal FormService FormService { get; }
    
    public DataImportationWorker(
        DataImportationContext context,
        FormService formService, 
        ExpressionsService expressionsService, 
        IEntityRepository entityRepository, 
        FieldValuesService fieldValuesService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILogger<DataImportationWorker> logger)
    {
        FormElement = context.FormElement;
        FormService = formService;
        ExpressionsService = expressionsService;
        EntityRepository = entityRepository;
        FieldValuesService = fieldValuesService;
        StringLocalizer = stringLocalizer;
        Logger = logger;
        DataContext = context.DataContext;
        RawData = context.RawData;
        Separator = context.Separator;
        Culture = Thread.CurrentThread.CurrentUICulture;
    }

    public async Task RunWorkerAsync(CancellationToken token)
    {
        await Task.Run(async () =>
        {
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
        var listField = await GetListImportedField();

        string[] stringSeparators = { "\r\n" };
        string[] rows = RawData.Split(stringSeparators, StringSplitOptions.None);
        currentProcess.TotalRecords = rows.Length;
        currentProcess.Message = StringLocalizer["Importing {0} records...", currentProcess.TotalRecords.ToString("N0")];
        
        var defaultValues = await FieldValuesService.GetDefaultValuesAsync(FormElement, null, PageState.Import);
        var formData = new FormStateData(defaultValues, PageState.Import);

        //executa script antes da execuçao
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandBeforeProcess))
        {
            var parsedSql = ExpressionsService.ParseExpression(ProcessOptions.CommandBeforeProcess, formData, false);
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
            if (cols.Length != listField.Count)
            {
                currentProcess.Error++;

                string error = string.Empty;
                error += StringLocalizer["Invalid number of fields."];
                error += " ";
                error += StringLocalizer["Expected {0} Received {1}.", listField.Count, cols.Length];
                currentProcess.AddError(error);

                error += StringLocalizer["Click on the [Help] link for more information regarding the file layout."];
                throw new JJMasterDataException(error);
            }

            //Analisa se ignora a primeira linha do arquivo
            if (isFirstRow)
            {
                isFirstRow = false;
                string colName1 = listField[0].LabelOrName;
                if (colName1.Trim().ToLower().Equals(cols[0].Trim().ToLower()))
                {
                    currentProcess.AddError(StringLocalizer["File header ignored"]);
                    currentProcess.Ignore++;
                    continue;
                }
            }

            var values = GetDictionaryWithNameAndValue(listField, cols);
            await SetFormValues(values, currentProcess);
            Reporter(currentProcess);
            token.ThrowIfCancellationRequested();
        }

        //executa script após a execução
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandAfterProcess))
        {
            string parsedSql = ExpressionsService.ParseExpression(ProcessOptions.CommandAfterProcess, formData, false);
            await EntityRepository.SetCommandAsync(new DataAccessCommand(parsedSql!));
        }

        if (OnAfterProcess != null)
            OnAfterProcess.Invoke(this, new FormAfterActionEventArgs());
    }

    /// <summary>
    /// Preenche um hashtable com o nome do campor e o valor
    /// </summary>
    private IDictionary<string, object> GetDictionaryWithNameAndValue(IReadOnlyList<FormElementField> listField, string[] cols)
    {
        var values = new Dictionary<string, object>();
        for (int i = 0; i < listField.Count; i++)
        {
            var field = listField[i];
            string value = cols[i];
            if (field.Component == FormComponent.CheckBox)
            {
                if (StringManager.ParseBool(value))
                    value = "1";
                else
                    value = "0";
            }

            values.Add(field.Name, value);
        }

        return values;
    }

    /// <summary>
    /// Lista de campos a serem importados
    /// </summary>
    private async Task<List<FormElementField>> GetListImportedField()
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var defaultValues = new Dictionary<string, object>();
        var formData = new FormStateData(defaultValues, PageState.Import);
        var list = new List<FormElementField>();
        foreach (var field in FormElement.Fields)
        {
            bool visible = await ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData);
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
            var values = await FieldValuesService.MergeWithExpressionValuesAsync(FormElement, fileValues, PageState.Import, true);
            var ret = await FormService.InsertOrReplaceAsync(FormElement, values, DataContext);

            if (ret.IsValid)
            {
                switch (ret.Result)
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
                foreach (var err in ret.Errors)
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