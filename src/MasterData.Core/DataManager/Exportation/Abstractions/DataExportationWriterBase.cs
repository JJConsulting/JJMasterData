using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public abstract class DataExportationWriterBase(
    IEncryptionService encryptionService,
    ExpressionsService expressionsService,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IOptionsSnapshot<MasterDataCoreOptions> options,
    ILogger<DataExportationWriterBase> logger)
    : IBackgroundTaskWorker, IExportationWriter
{
    public event EventHandler<IProgressReporter> OnProgressChanged;

    protected const int RecordsPerPage = 100000;

    #region "Properties"

    private DataExportationReporter _processReporter;
    private List<FormElementField> _fields;
    private FormFilePathBuilder _pathBuilder;

    private IEncryptionService EncryptionService { get; } = encryptionService;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    protected IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IOptionsSnapshot<MasterDataCoreOptions> Options { get; } = options;

    private ILogger<DataExportationWriterBase> Logger { get; } = logger;
    private FormFilePathBuilder PathBuilder => _pathBuilder ??= new FormFilePathBuilder(FormElement);

    public string AbsoluteUri { get; internal set; }

    private string GetFolderPath(FormElementField field, Dictionary<string, object> values)
    {
        return PathBuilder.GetFolderPath(field, values);
    }

    protected List<FormElementField> VisibleFields
    {
        get
        {
            if (_fields != null)
                return _fields;
            if (Configuration.ExportAllFields)
            {
                _fields = FormElement.Fields.FindAll(x => x.Export);
            }

            else
            {
                var defaultValues = new Dictionary<string, object>();
                var formData = new FormStateData(defaultValues, PageState.List);
                _fields = [];

                foreach (var field in FormElement.Fields)
                {
                    if (field.Export && ExpressionsService.GetBoolValue(field.VisibleExpression, formData))
                    {
                        _fields.Add(field);
                    }
                }
            }

            return _fields;
        }
    }

    public ProcessOptions ProcessOptions { get; set; }

    public DataExportationReporter ProcessReporter => _processReporter ??= new DataExportationReporter();

    public ExportOptions Configuration { get; set; }


    /// <summary>
    /// Get = Recupera o filtro atual<para/>
    /// </summary>
    public Dictionary<string, object> CurrentFilter { get; set; } = new();

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
            string folderPath = DataExportationHelper.GetFolderPath(FormElement, path, UserId);

            CreateFolderPathIfNotExits(folderPath);

            return folderPath;
        }
    }

    private static void CreateFolderPathIfNotExits(string folderPath)
    {
        try
        {
            if (folderPath != null && !Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
        catch (Exception ex)
        {
            const string message = "Error on create directory, set a valid ExportationFolderPath on JJMasterData Options.";
            throw new JJMasterDataException(message, ex);
        }
    }

    public string UserId { get; set; }

#if NETFRAMEWORK
    internal HttpContext HttpContext { get; set; }
#endif

    #endregion

    public async Task RunWorkerAsync(CancellationToken token)
    {
#if NETFRAMEWORK
            HttpContext.Current = HttpContext;
#endif
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

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
            ProcessReporter.HasError = false;
            ProcessReporter.Message = StringLocalizer["File generated successfully!"];
        }
        catch (Exception ex)
        {
            ProcessReporter.HasError = true;

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
                    ProcessReporter.Message += StringLocalizer[ExceptionManager.GetMessage(ex)];
                    Logger.LogError(ex, "Error at data exportation");
                    break;
            }

            if (File.Exists(FolderPath) && !FileIO.IsFileLocked(FolderPath))
                File.Delete(FolderPath);
        }
        finally
        {
            ProcessReporter.EndDate = DateTime.Now;
            Reporter(ProcessReporter);
        }
    }

    protected void Reporter(DataExportationReporter processReporter)
    {
        OnProgressChanged?.Invoke(this, processReporter);
    }

    public abstract Task GenerateDocument(Stream ms, CancellationToken token);

    protected string GetFileLink(FormElementField field, Dictionary<string, object> row, string value)
    {
        if (!field.DataFile!.ExportAsLink)
            return null;

        if (string.IsNullOrEmpty(value))
            return null;

        var files = value.Split(',');
        if (files.Length != 1)
            return null;

        var filePath = GetFolderPath(field, row) + value;
        return JJFileDownloader.GetExternalDownloadLink(EncryptionService, AbsoluteUri, filePath);
    }


    private string GetFilePath()
    {
        string fileName;
        var exportActionFileName = FormElement.Options.GridToolbarActions.ExportAction.FileName;

        if (!string.IsNullOrEmpty(exportActionFileName))
        {
            fileName = exportActionFileName;
        }
        else if (!string.IsNullOrEmpty(FormElement.Title))
        {
            fileName = ExpressionsService.GetExpressionValue(FormElement.Title, new FormStateData
            {
                Values = new Dictionary<string, object>(),
                UserValues = new Dictionary<string, object>(),
                PageState = PageState.List
            })?.ToString() ?? string.Empty;
        }
        else if (!string.IsNullOrEmpty(FormElement.Name))
        {
            fileName = FormElement.Name.Trim().ToLower();
        }
        else
        {
            fileName = "file";
        }

        fileName = StringManager.GetStringWithoutAccents(fileName);

        string[] escapeChars = ["/", "\\", "|", ":", "*", ">", "<", "+", "=", "&", "%", "$", "#", "@", " "];

        foreach (var @char in escapeChars)
        {
            fileName = fileName.Replace(@char, string.Empty);
        }

        fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);
        var extension = Configuration.FileExtension.ToString().ToLower();

        return $"{fileName}_{DateTime.Now:yyyMMdd_HHmmss}.{extension}";
    }
}