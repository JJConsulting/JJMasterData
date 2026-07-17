#nullable disable warnings
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Tasks.Progress;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public abstract class DataExportationWriterBase(
    ExpressionsService expressionsService,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IOptionsSnapshot<MasterDataCoreOptions> options,
    ILogger<DataExportationWriterBase> logger)
    : IBackgroundTaskWorker, IExportationWriter
{
    public event EventHandler<IProgressReporter> OnProgressChanged;

    protected const int RecordsPerPage = 100000;
    
    private List<FormElementField> _fields;
    
    #region "Properties"

    private DataExportationReporter _processReporter;

    private ExpressionsService ExpressionsService { get; } = expressionsService;
    protected IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IOptionsSnapshot<MasterDataCoreOptions> Options { get; } = options;

    private ILogger<DataExportationWriterBase> Logger { get; } = logger;

    internal FileDownloaderFactory FileDownloaderFactory { get; set; }
    internal IFileStorage FileStorage { get; set; }
    internal string AbsoluteUri { get; set; }

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

                foreach (var @field in FormElement.Fields)
                {
                    if (@field.Export && ExpressionsService.GetBoolValue(@field.VisibleExpression, formData))
                    {
                        _fields.Add(@field);
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
            return DataExportationHelper.GetExportationFolderPath(FormElement, path, UserId);
        }
    }

    public string UserId { get; set; }

    #endregion

    public async Task RunWorkerAsync(CancellationToken token)
    {
        if (FormElement == null)
            throw new ArgumentNullException(nameof(FormElement));

        try
        {
            _processReporter = new DataExportationReporter();
            ProcessReporter.UserId = UserId;
            ProcessReporter.StartDate = DateTime.Now;
            ProcessReporter.Message = StringLocalizer["Retrieving records..."];

            Reporter(ProcessReporter);
            
            var fileName = GetFileName();
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 81920, true))
                {
                    await GenerateDocument(fs, token);
                }

                await using var readStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
                var fullPath = FileStoragePath.Combine(FolderPath, fileName);
                await FileStorage.SaveAsync(fullPath, readStream, true, token);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }

            ProcessReporter.FolderPath = FolderPath;
            ProcessReporter.FileName = fileName;

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
                case JJMasterDataException:
                    ProcessReporter.Message = ex.Message;
                    break;
                default:
                    ProcessReporter.Message = $"{StringLocalizer["Unexpected error"]}\n";
                    ProcessReporter.Message += StringLocalizer.GetExceptionMessage(ex);
                    Logger.LogError(ex, "Error at data exportation");
                    break;
            }

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

    protected string GetFileLink(FormElement formElement, FormElementField field, Dictionary<string, object> row,
        string value)
    {
        if (!field.DataFile!.ExportAsLink)
            return null;

        if (string.IsNullOrEmpty(value))
            return null;

        var files = value.Split(',');
        if (files.Length != 1)
            return null;

        var fileName = Path.GetFileName(files[0]);
        if (string.IsNullOrEmpty(fileName))
            return null;

        var downloader = FileDownloaderFactory.Create(formElement, field, row, fileName);
        
        return new Uri(new Uri(AbsoluteUri), downloader.GetDownloadUrl(AbsoluteUri)).AbsoluteUri;
    }
    
    private string GetFileName()
    {
        string fileName;
        var exportActionFileName = FormElement.Options.GridToolbarActions.ExportAction.FileName;

        if (!string.IsNullOrEmpty(exportActionFileName))
        {
            fileName = exportActionFileName;
        }
        else if (!string.IsNullOrEmpty(FormElement.Title))
        {
            fileName = ExpressionsService.GetExpressionValue(FormElement.Title, new FormStateData(PageState.List))?.ToString() ?? string.Empty;
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

    public async Task<Stream> OpenReadAsync()
    {
        var fullPath = FileStoragePath.Combine(ProcessReporter.FolderPath, ProcessReporter.FileName);
        return await FileStorage.OpenReadAsync(fullPath);
    }
}
