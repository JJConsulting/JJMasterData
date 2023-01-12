using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataImpFactory
{
    private IFormEventResolver FormEventResolver { get; }
    private IHttpContext HttpContext { get; }
    private RepositoryServicesFacade RepositoryServicesFacade { get; }
    private IBackgroundTask BackgroundTask { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private ILoggerFactory LoggerFactory { get; }
    private AuditLogService AuditLogService { get; }

    public DataImpFactory(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        IFormEventResolver formEventResolver,
        IBackgroundTask backgroundTask,
        JJMasterDataEncryptionService encryptionService,
        IOptions<JJMasterDataCoreOptions> options,
        ILoggerFactory loggerFactory,
        AuditLogService auditLogService)
    {
        FormEventResolver = formEventResolver;
        RepositoryServicesFacade = repositoryServicesFacade;
        BackgroundTask = backgroundTask;
        EncryptionService = encryptionService;
        Options = options;
        LoggerFactory = loggerFactory;
        AuditLogService = auditLogService;
        HttpContext = httpContext;
    }

    public JJDataImp CreateDataImp(string elementName)
    {
        var dataImp = new JJDataImp(HttpContext, RepositoryServicesFacade, BackgroundTask, EncryptionService, Options,
            LoggerFactory, AuditLogService);

        SetDataImpParams(dataImp, elementName);

        return dataImp;
    }

    internal void SetDataImpParams(JJDataImp dataImp, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var metadata = RepositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);

        var dataContext = new DataContext(HttpContext, DataContextSource.Upload,
            DataHelper.GetCurrentUserId(HttpContext, null));

        var formEvent = FormEventResolver?.GetFormEvent(elementName);
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));

        dataImp.FormElement = metadata.GetFormElement();
        dataImp.ProcessOptions = metadata.UIOptions.ToolBarActions.ImportAction.ProcessOptions;
    }
}