using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public abstract class BaseWriter : IBackgroundTaskWorker, IWriter
{

    public event EventHandler<IProgressReporter> OnProgressChanged;

    public const int RegPerPag = 100000;

    #region "Properties"
    
    private DataExportationReporter _processReporter;
    private List<FormElementField> _fields;

    protected IFieldVisibilityService FieldVisibilityService { get; } =
        JJService.Provider.GetScopedDependentService<IFieldVisibilityService>();
    protected IStringLocalizer<JJMasterDataResources> StringLocalizer { get; } =
        JJService.Provider.GetScopedDependentService<IStringLocalizer<JJMasterDataResources>>();

    protected IControlFactory<JJTextFile> TextFileFactory { get; } =
        JJService.Provider.GetScopedDependentService<IControlFactory<JJTextFile>>();
    
    protected ILogger<BaseWriter> Logger { get; } =
        JJService.Provider.GetScopedDependentService<ILogger<BaseWriter>>();
    
    public List<FormElementField> Fields
    {
        get
        {
            if (_fields == null)
            {
                if (Configuration.ExportAllFields)
                    _fields = FormElement.Fields.ToList().FindAll(x => x.Export);
                else
                    _fields = FormElement.Fields.ToList().FindAll(x => x.Export && FieldVisibilityService.IsVisible(x, PageState.List, null));
            }

            return _fields;
        }
    }

    public ProcessOptions ProcessOptions { get; set; }

    public DataExportationReporter ProcessReporter => _processReporter ??= new DataExportationReporter();

    public ExportOptions Configuration { get; set; }

    public ControlFactory ControlFactory { get; } = JJService.Provider.GetScopedDependentService<ControlFactory>();

    /// <summary>
    /// Get = Recupera o filtro atual<para/>
    /// </summary>
    public IDictionary<string,dynamic>CurrentFilter { get; set; }

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
            var path = JJService.Provider.GetScopedDependentService<IOptions<JJMasterDataCoreOptions>>().Value.ExportationFolderPath;
            string folderPath = Path.Combine(path, FormElement.Name);

            if (ProcessOptions.Scope == ProcessScope.User)
            {
                folderPath = Path.Combine(folderPath, UserId);
            }

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }
    }

    public string UserId { get; set; }
    public HttpContext CurrentContext { get; internal set; }

    //We need this property because Current.Context.Request is disposed inside a thread.
    public string AbsoluteUri { get; set; }

    #endregion


    public BaseWriter()
    {
        CurrentFilter = new Dictionary<string,dynamic>();
    }

    public async Task RunWorkerAsync(CancellationToken token)
    {
        if (FormElement == null) throw new ArgumentNullException(nameof(FormElement));
        await Task.Run(() =>
            {
#if NETFRAMEWORK
                HttpContext.Current = CurrentContext;
#endif
                try
                {
                    _processReporter = new DataExportationReporter();
                    ProcessReporter.UserId = UserId;
                    ProcessReporter.StartDate = DateTime.Now;
                    ProcessReporter.Message = StringLocalizer["Retrieving records..."];

                    Reporter(ProcessReporter);

                    var filePath = Path.Combine(FolderPath, GetFilePath());

                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        GenerateDocument(fs, token);
                    }

                    ProcessReporter.FilePath = filePath;

                    ProcessReporter.EndDate = DateTime.Now;
                    ProcessReporter.Message = StringLocalizer["File generated successfully!"];

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
                            ProcessReporter.Message = StringLocalizer["Process aborted by the user."];
                            break;
                        case IOException:
                            if (FileIO.IsFileLocked(FolderPath))
                                ProcessReporter.Message = StringLocalizer["File is already being used by another process. Try downloading it from \"Recently generated files\"."];
                            else
                                goto default;
                            break;
                        case JJMasterDataException:
                            ProcessReporter.Message = ex.Message;
                            break;
                        default:
                            ProcessReporter.Message = StringLocalizer["Unexpected error"] + "\n";
                            ProcessReporter.Message += ExceptionManager.GetMessage(ex);
                            Logger.LogError(ex, "Error at data exportation");
                            break;
                    }
                }
                finally
                {
                    ProcessReporter.EndDate = DateTime.Now;
                    Reporter(ProcessReporter);
                }
            }, token);
    }

    public void Reporter(DataExportationReporter processReporter)
    {
        OnProgressChanged?.Invoke(this, processReporter);
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

        var values = new Dictionary<string, dynamic>();

        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            values.Add(row.Table.Columns[i].ColumnName, row[i]);
        }

        string fileName = value;
        var textFile = TextFileFactory.Create();
        textFile.FormElement = FormElement;
        textFile.ElementField = field;
        textFile.PageState = PageState.List;
        textFile.Text = value;
        textFile.FormValues = values;
        textFile.Name = field.Name;

        return textFile.GetDownloadLink(fileName, true, AbsoluteUri);
    }


    private string GetFilePath()
    {
        string title;
        if (!string.IsNullOrEmpty(FormElement.Title))
            title = StringLocalizer[FormElement.Title].Value.Trim().ToLower();
        else if (!string.IsNullOrEmpty(FormElement.Name))
            title = FormElement.Name.Trim().ToLower();
        else
            title = "file";

        title = StringManager.GetStringWithoutAccents(title);
        
        string[] escapeChars = { "/", "\\", "|", ":", "*", ">", "<", "+", "=", "&", "%", "$", "#", "@", " " };
        
        foreach (var @char in escapeChars)
        {
            title = title.Replace(@char, string.Empty);
        }

        title = HttpUtility.UrlEncode(title, Encoding.UTF8);
        string ext = Configuration.FileExtension.ToString().ToLower();

        return $"{title}_{DateTime.Now:yyyMMdd_HHmmss}.{ext}";
    }




}
