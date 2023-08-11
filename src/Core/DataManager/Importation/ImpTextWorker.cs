using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.DataManager.Imports;

public class ImpTextWorker : IBackgroundTaskWorker
{
    #region "Events"

    public EventHandler<FormAfterActionEventArgs> OnAfterProcess;
    public event EventHandler<IProgressReporter> OnProgressChanged;

    #endregion

    public string UserId { get; set; }

    public ProcessOptions ProcessOptions { get; set; }

    public bool EnableHistoryLog { get; set; }

    public CultureInfo Culture { get; set; }

    public FormElement FormElement { get; set; }
    internal DataContext DataContext { get; private set; }

    internal IExpressionsService ExpressionsService { get; }

    internal IEntityRepository EntityRepository { get; }
    
    internal IFieldValuesService FieldValuesService { get; }

    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    internal ILogger<ImpTextWorker> Logger { get; }

    internal IFormService FormService { get; }
    public string PostedText { get; private set; }

    public char SplitChar { get; private set; }


    public ImpTextWorker(FormElement formElement,
                         IFormService formService,
                         DataContext dataContext,
                         string postedText,
                         char splitChar)
    {
        FormElement = formElement;
        FormService = formService;
        DataContext = dataContext;
        PostedText = postedText;
        SplitChar = splitChar;
        Culture = Thread.CurrentThread.CurrentUICulture;
    }

    public async Task RunWorkerAsync(CancellationToken token)
    {
        await Task.Run(async () =>
        {
            var currentProcess = new DataImpReporter();
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

    private void Reporter(DataImpReporter reporter)
    {
        OnProgressChanged?.Invoke(this, reporter);
    }

    private async Task RunWorker(DataImpReporter currentProcess, CancellationToken token)
    {
        Thread.CurrentThread.CurrentUICulture = Culture;
        Thread.CurrentThread.CurrentCulture = Culture;

        //recuperando campos a serem importados
        var listField = GetListImportedField();

        string[] stringSeparators = { "\r\n" };
        string[] rows = PostedText.Split(stringSeparators, StringSplitOptions.None);
        currentProcess.TotalRecords = rows.Length;
        currentProcess.Message = StringLocalizer["Importing {0} records...", currentProcess.TotalRecords.ToString("N0")];
        
        var defaultValues = await FieldValuesService.GetDefaultValuesAsync(FormElement, null, PageState.Import);
        var formData = new FormStateData(defaultValues, PageState.Import);

        //executa script antes da execuçao
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandBeforeProcess))
        {
            string cmd = ExpressionsService.ParseExpression(ProcessOptions.CommandBeforeProcess, formData, false);
            await EntityRepository.SetCommandAsync(cmd);
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
            if (string.IsNullOrWhiteSpace(line.Replace(SplitChar.ToString(), string.Empty)))
            {
                currentProcess.AddError(StringLocalizer["Empty line ignored"]);
                currentProcess.Ignore++;
                continue;
            }

            //Verifica quantidade de colunas do arquivo
            string[] cols = line.Split(SplitChar);
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
                string colName1 = string.IsNullOrEmpty(listField[0].Label) ? listField[0].Name : listField[0].Label;
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
            string cmd;
            cmd = ExpressionsService.ParseExpression(ProcessOptions.CommandAfterProcess, formData, false);
            EntityRepository.SetCommand(cmd);
        }

        if (OnAfterProcess != null)
            OnAfterProcess.Invoke(this, new FormAfterActionEventArgs());
    }

    /// <summary>
    /// Preenche um hashtable com o nome do campor e o valor
    /// </summary>
    private IDictionary<string, dynamic> GetDictionaryWithNameAndValue(IReadOnlyList<FormElementField> listField, string[] cols)
    {
        var values = new Dictionary<string, dynamic>();
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
    private List<FormElementField> GetListImportedField()
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var defaultValues = new Dictionary<string, dynamic>();
        var formData = new FormStateData(defaultValues, PageState.Import);
        var list = new List<FormElementField>();
        foreach (var field in FormElement.Fields)
        {
            bool visible = ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData).GetAwaiter().GetResult();
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
    private async Task SetFormValues(IDictionary<string, dynamic> fileValues, DataImpReporter currentProcess)
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