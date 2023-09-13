using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public abstract class DataExportationWriterBase : IBackgroundTaskWorker, IExportationWriter
{
    public event EventHandler<IProgressReporter> OnProgressChanged;

    protected const int RecordsPerPage = 100000;

    #region "Properties"

    private DataExportationReporter _processReporter;
    private List<FormElementField> _fields;

    private IExpressionsService ExpressionsService { get; }
    protected IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private IControlFactory<JJTextFile> TextFileFactory { get; }

    private ILogger<DataExportationWriterBase> Logger { get; }


    protected async Task<List<FormElementField>> GetVisibleFieldsAsync()
    {
        if (_fields != null)
            return _fields;
        if (Configuration.ExportAllFields)
        {
            _fields = FormElement.Fields.ToList().FindAll(x => x.Export);
        }

        else
        {
            var defaultValues = new Dictionary<string, object>();
            var formData = new FormStateData(defaultValues, PageState.Import);
            _fields = new List<FormElementField>();

            foreach (var field in FormElement.Fields)
            {
                if (field.Export && await ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData))
                {
                    _fields.Add(field);
                }
            }
        }

        return _fields;
    }

    public ProcessOptions ProcessOptions { get; set; }

    public DataExportationReporter ProcessReporter => _processReporter ??= new DataExportationReporter();

    public ExportOptions Configuration { get; set; }


    /// <summary>
    /// Get = Recupera o filtro atual<para/>
    /// </summary>
    public IDictionary<string, object> CurrentFilter { get; set; }

    /// <summary>
    /// Recupera a ordenação da tabela, 
    /// por padrão utiliza o primeiro campo da chave primária
    /// </summary>
    /// <returns>Ordem atual da tabela</returns>
    /// <remarks>
    /// Para mais de um campo utilize virgula ex:
    /// "Campo1 ASC, Campo2 DESC, Campo3 ASC"
    /// </remarks>
    public OrderByData CurrentOrder { get; set; }

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
    public IList<Dictionary<string, object>> DataSource { get; set; }

    public int TotalOfRecords { get; set; }

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
            var path = Options.Value.ExportationFolderPath;
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

    #endregion


    protected DataExportationWriterBase(IExpressionsService expressionsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer, IOptions<JJMasterDataCoreOptions> options,
        IControlFactory<JJTextFile> textFileFactory, ILogger<DataExportationWriterBase> logger)
    {
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        Options = options;
        TextFileFactory = textFileFactory;
        Logger = logger;
        CurrentFilter = new Dictionary<string, object>();
    }

    public async Task RunWorkerAsync(CancellationToken token)
    {
        if (FormElement == null) throw new ArgumentNullException(nameof(FormElement));
        await Task.Run(async () =>
        {
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
                    await GenerateDocument(fs, token);
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
                            ProcessReporter.Message =
                                StringLocalizer[
                                    "File is already being used by another process. Try downloading it from \"Recently generated files\"."];
                        else
                            goto default;
                        break;
                    case JJMasterDataException:
                        ProcessReporter.Message = ex.Message;
                        break;
                    default:
                        ProcessReporter.Message = $"{StringLocalizer["Unexpected error"]}\n";
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

    public abstract Task GenerateDocument(Stream ms, CancellationToken token);

    public string GetLinkFile(FormElementField field, Dictionary<string, object> row, string value)
    {
        if (!field.DataFile.ExportAsLink)
            return null;

        if (string.IsNullOrEmpty(value))
            return null;

        var files = value.Split(',');
        if (files.Length != 1)
            return null;

        string fileName = value;
        var textFile = TextFileFactory.Create();
        textFile.FormElement = FormElement;
        textFile.FormElementField = field;
        textFile.PageState = PageState.List;
        textFile.Text = value;
        textFile.FormValues = row;
        textFile.Name = field.Name;

        return textFile.GetDownloadLink(fileName, true);
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