using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager.Imports;

public class ImpTextWorker : IBackgroundTaskWorker
{

    #region "Events"

    /// <summary>
    /// Evento disparado antes de inserir pu atualizar o registro no banco de dados.
    /// </summary>
    public EventHandler<FormBeforeActionEventArgs> OnBeforeImport;

    /// <summary>
    /// Evento disparado após incluir ou atualizar o registro no banco de dados.
    /// </summary>
    public EventHandler<FormAfterActionEventArgs> OnAfterImport;

    /// <summary>
    /// Evento disparado após processar todo arquivo
    /// </summary>
    public EventHandler<FormAfterActionEventArgs> OnAfterProcess;

    public event EventHandler<IProgressReporter> OnProgressChanged;

    #endregion

    public string UserId { get; set; }

    public ProcessOptions ProcessOptions { get; set; }

    public bool EnableHistoryLog { get; set; }

    public CultureInfo Culture { get; set; }

    public FormElement FormElement
    {
        get
        {
            return FieldManager.FormElement;
        }
    }

    internal FieldManager FieldManager { get; private set; }

    internal FormService FormService { get; private set; }

    public string PostedText { get; private set; }

    public char SplitChar { get; private set; }


    public ImpTextWorker(FieldManager fieldManager,
                         FormService formservice,
                         string postedText,
                         char splitChar)
    {
        FieldManager = fieldManager;
        FormService = formservice;
        PostedText = postedText;
        SplitChar = splitChar;
        Culture = Thread.CurrentThread.CurrentUICulture;
    }

    public async Task RunWorkerAsync(CancellationToken token)
    {
        await Task.Run(() =>
        {
            var currentProcess = new DataImpReporter();
            try
            {
                currentProcess.StartDate = DateTime.Now;
                currentProcess.UserId = UserId;
                Reporter(currentProcess);
                RunWorker(token, currentProcess);

                if (currentProcess.Error > 0)
                    currentProcess.Message = Translate.Key("File imported with errors!");
                else
                    currentProcess.Message = Translate.Key("File imported successfully!");
            }
            catch (Exception ex)
            {
                currentProcess.HasError = true;
                if (ex is OperationCanceledException ||
                    ex is ThreadAbortException)
                {
                    currentProcess.Message = Translate.Key("Process aborted by user");
                    currentProcess.AddError(currentProcess.Message);
                }
                else if (ex is DataDictionaryException)
                {
                    currentProcess.Message = ex.Message;
                    currentProcess.AddError(currentProcess.Message);
                }
                else
                {
                    currentProcess.Message = Translate.Key("Unexpected error");
                    currentProcess.Message += " ";
                    currentProcess.Message += ExceptionManager.GetMessage(ex);
                    currentProcess.AddError(currentProcess.Message);
                    Log.AddError(ex.Message);
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
        if (OnProgressChanged != null)
            OnProgressChanged.Invoke(this, reporter);
    }

    private void RunWorker(CancellationToken token, DataImpReporter currentProcess)
    {
        Thread.CurrentThread.CurrentUICulture = Culture;
        Thread.CurrentThread.CurrentCulture = Culture;


        var formManager = FormService.FormManager;
        //recuperando campos a serem importados
        var listField = GetListImportedField();

        string[] stringSeparators = { "\r\n" };
        string[] rows = PostedText.Split(stringSeparators, StringSplitOptions.None);
        currentProcess.TotalRecords = rows.Length;
        currentProcess.Message = Translate.Key("Importing {0} records...", currentProcess.TotalRecords.ToString("N0"));
        //recupera default values
        var defaultValues = formManager.GetDefaultValues(null, PageState.Import);

        //executa script antes da execuçao
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandBeforeProcess))
        {
            string cmd;
            cmd = formManager.Expression.ParseExpression(ProcessOptions.CommandBeforeProcess, PageState.Import, false, defaultValues);
            formManager.DataAccess.SetCommand(cmd);
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
                currentProcess.AddError(Translate.Key("Empty line ignored"));
                currentProcess.Ignore++;
                continue;
            }

            //Verifica quantidade de colunas do arquivo
            string[] cols = line.Split(SplitChar);
            if (cols.Length != listField.Count)
            {
                currentProcess.Error++;

                string error = string.Empty;
                error += Translate.Key("Invalid number of fields.");
                error += " ";
                error += Translate.Key("Expected {0} Received {1}.", listField.Count, cols.Length);
                currentProcess.AddError(error);

                error += Translate.Key("Click on the [Help] link for more information regarding the file layout.");
                throw new DataDictionaryException(error);
            }

            //Analisa se ignora a primeira linha do arquivo
            if (isFirstRow)
            {
                isFirstRow = false;
                string colName1 = string.IsNullOrEmpty(listField[0].Label) ? listField[0].Name : listField[0].Label;
                if (colName1.Trim().ToLower().Equals(cols[0].Trim().ToLower()))
                {
                    currentProcess.AddError(Translate.Key("File header ignored"));
                    currentProcess.Ignore++;
                    continue;
                }
            }

            Hashtable values = GetHashWithNameAndValue(listField, cols);
            SetFormValues(values, currentProcess);
            Reporter(currentProcess);
            token.ThrowIfCancellationRequested();
        }

        //executa script após a execução
        if (currentProcess.TotalRecords > 0 &&
            !string.IsNullOrEmpty(ProcessOptions?.CommandAfterProcess))
        {
            string cmd;
            cmd = formManager.Expression.ParseExpression(ProcessOptions.CommandAfterProcess, PageState.Import, false, defaultValues);
            formManager.DataAccess.SetCommand(cmd);
        }

        if (OnAfterProcess != null)
            OnAfterProcess.Invoke(this, new FormAfterActionEventArgs());
    }

    /// <summary>
    /// Preenche um hashtable com o nome do campor e o valor
    /// </summary>
    private Hashtable GetHashWithNameAndValue(List<FormElementField> listField, string[] cols)
    {
        var values = new Hashtable();
        for (int i = 0; i < listField.Count; i++)
        {
            var field = listField[i];
            string value = cols[i];
            if (field.Component == FormComponent.CheckBox)
            {
                if (ExpressionManager.ParseBool(value))
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

        var list = new List<FormElementField>();
        foreach (var field in FormElement.Fields)
        {
            bool visible = FieldManager.IsVisible(field, PageState.Import, null);
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
    private void SetFormValues(Hashtable values, DataImpReporter currentProcess)
    {
        try
        {

            var ret = FormService.InsertOrReplace(values);

            if (ret.IsValid)
            {
                switch (ret.Result)
                {
                    case CommandType.Insert:
                        currentProcess.Insert++;
                        break;
                    case CommandType.Update:
                        currentProcess.Update++;
                        break;
                    case CommandType.Delete:
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
                foreach (DictionaryEntry err in ret.Errors)
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