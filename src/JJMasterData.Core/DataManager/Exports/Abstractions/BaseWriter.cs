using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public abstract class BaseWriter : IBackgroundTaskWorker, IWriter
{

    public event EventHandler<IProgressReporter> OnProgressChanged;

    public const int RegPerPag = 100000;

    #region "Properties"

    private FieldManager _fieldManager;
    private DataExpReporter _processReporter;
    private List<FormElementField> _fields;

    public List<FormElementField> Fields
    {
        get
        {
            if (_fields == null)
            {
                if (Configuration.ExportAllFields)
                    _fields = FormElement.Fields.ToList().FindAll(x => x.Export);
                else
                    _fields = FormElement.Fields.ToList().FindAll(x => x.Export && FieldManager.IsVisible(x, PageState.List, null));
            }

            return _fields;
        }
    }

    public ProcessOptions ProcessOptions { get; set; }

    public DataExpReporter ProcessReporter => _processReporter ??= new DataExpReporter();

    public ExportOptions Configuration { get; set; }

    public FieldManager FieldManager
    {
        get
        {
            if (_fieldManager != null) return _fieldManager;
            _fieldManager = new FieldManager(FormElement);

            return _fieldManager;
        }
        set => _fieldManager = value;
    }

    /// <summary>
    /// Get = Recupera o filtro atual<para/>
    /// </summary>
    public Hashtable CurrentFilter { get; set; }

    /// <summary>
    /// Recupera a ordenação da tabela, 
    /// por padrão utiliza o primeiro campo da chave primária
    /// </summary>
    /// <returns>Ordem atual da tabela</returns>
    /// <remarks>
    /// Para mais de um campo utilize virgula ex:
    /// "Campo1 ASC, Campo2 DESC, Campo3 ASC"
    /// </remarks>
    public string CurrentOrder { get; set; }

    /// <summary>
    /// Tabela com os dados
    /// </summary>
    /// <remarks>
    /// Datasource é propriedade responsável por controlar a origem de dados.
    /// O componente utiliza seguinte regra para recuperar os dados da grid:
    /// <para/>1) Utiliza a propriedade DataSource;
    /// <para/>2) Se a propriedade DataSource for nula, tenta executar a ação OnDataLoad;
    /// <para/>3) Se a ação OnDataLoad não for implementada, tenta recuperar 
    /// utilizando a proc informada no FormElement;
    /// </remarks>
    public DataTable DataSource { get; set; }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Path where the files are generated.
    /// </summary>
    public string FolderPath
    {
        get
        {
            string folderPath = JJService.Settings.ExportationFolderPath;
            folderPath += FormElement.Name;
            folderPath += "\\";

            if (ProcessOptions.Scope == ProcessScope.User)
            {
                folderPath += UserId;
                folderPath += "\\";
            }

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }
    }

    public string UserId { get; set; }
    public HttpContext CurrentContext { get; internal set; }

    #endregion


    public BaseWriter()
    {
        CurrentFilter = new Hashtable();
    }

    public async Task RunWorkerAsync(CancellationToken token)
    {
        if (FormElement == null) throw new ArgumentNullException(nameof(FormElement));
        if (FieldManager == null) throw new ArgumentNullException(nameof(FieldManager));

        await Task.Run(() =>
            {
#if NETFRAMEWORK
                HttpContext.Current = CurrentContext;
#endif
                try
                {
                    _processReporter = new DataExpReporter();
                    ProcessReporter.UserId = UserId;
                    ProcessReporter.StartDate = DateTime.Now;
                    ProcessReporter.Message = Translate.Key("Retrieving records...");

                    Reporter(ProcessReporter);

                    var filePath = Path.Combine(FolderPath, GetFilePath());

                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        GenerateDocument(fs, token);
                    }

                    ProcessReporter.FilePath = filePath;

                    ProcessReporter.EndDate = DateTime.Now;
                    ProcessReporter.Message = Translate.Key("File generated successfully!");

                }
                catch (Exception ex)
                {
                    ProcessReporter.HasError = true;

                    if (File.Exists(FolderPath) && !FileIO.IsFileLocked(FolderPath))
                        File.Delete(FolderPath);

                    switch (ex)
                    {
                        case OperationCanceledException:
                        case ThreadAbortException:
                            ProcessReporter.Message = Translate.Key("Process aborted by the user.");
                            break;
                        case IOException:
                            if (FileIO.IsFileLocked(FolderPath))
                                ProcessReporter.Message = Translate.Key("File is already being used by another process. Try downloading it from \"Recently generated files\".");
                            else
                                goto default;
                            break;
                        case DataDictionaryException:
                            ProcessReporter.Message = ex.Message;
                            break;
                        default:
                            ProcessReporter.Message = Translate.Key("Unexpected error") + "\n";
                            ProcessReporter.Message += ExceptionManager.GetMessage(ex);
                            Log.AddError(ex.Message);
                            break;
                    }
                }
                finally
                {
                    ProcessReporter.EndDate = DateTime.Now;
                    Reporter(ProcessReporter);
                }
            });
    }

    public void Reporter(DataExpReporter processReporter)
    {
        if (OnProgressChanged != null)
            OnProgressChanged.Invoke(this, processReporter);
    }

    public abstract void GenerateDocument(Stream ms, CancellationToken token);

    public string GetLinkFile(FormElementField field, DataRow row, string value)
    {
        if (!field.DataFile.ExportAsLink)
            return null;

        if (string.IsNullOrEmpty(value))
            return null;

        var files = value.Split(',');
        if (files.Length != 1)
            return null;

        var values = new Hashtable();

        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            values.Add(row.Table.Columns[i].ColumnName, row[i]);
        }

        string fileName = value;
        var textFile = new JJTextFile();
        textFile.FormElement = FormElement;
        textFile.ElementField = field;
        textFile.PageState = PageState.List;
        textFile.Text = value;
        textFile.FormValues = values;
        textFile.Name = field.Name;
        return textFile.GetDownloadLink(fileName, true);
    }


    private string GetFilePath()
    {
        string title;
        if (!string.IsNullOrEmpty(FormElement.Title))
            title = Translate.Key(FormElement.Title).Trim().ToLower();
        else if (!string.IsNullOrEmpty(FormElement.Name))
            title = FormElement.Name.Trim().ToLower();
        else
            title = "file";

        title = StringManager.NoAccents(title);
        string[] espChars = { "/", "\\", "|", ":", "*", ">", "<", "+", "=", "&", "%", "$", "#", "@", " " };
        for (int i = 0; i < espChars.Length; i++)
        {
            title = title.Replace(espChars[i], "");
        }

        title = HttpUtility.UrlEncode(title, Encoding.UTF8);
        string ext = Configuration.FileExtension.ToString().ToLower();

        return string.Format("{0}_{1}.{2}", title, DateTime.Now.ToString("yyyMMdd_HHmmss"), ext);
    }




}
