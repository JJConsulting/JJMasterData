using System;
using System.Collections.Generic;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.WebComponents.Factories;

public class FormViewFactory
{
    private IHttpContext HttpContext { get; }
    private RepositoryServicesFacade RepositoryServicesFacade { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private ILoggerFactory LoggerFactory { get; }
    private IBackgroundTask BackgroundTask { get; }

    private IEnumerable<IExportationWriter> ExportationWriters { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private AuditLogService AuditLogService { get; }
    private IFormEventResolver FormEventResolver { get; }
    private DataPanelFactory DataPanelFactory { get; }
    private GridViewFactory GridViewFactory { get; }

    public FormViewFactory(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        JJMasterDataEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IBackgroundTask backgroundTask,
        IEnumerable<IExportationWriter> exportationWriters,
        IOptions<JJMasterDataCoreOptions> options,
        AuditLogService auditLogService,
        IFormEventResolver formEventResolver,
        DataPanelFactory dataPanelFactory,
        GridViewFactory gridViewFactory)
    {
        HttpContext = httpContext;
        RepositoryServicesFacade = repositoryServicesFacade;
        EncryptionService = encryptionService;
        LoggerFactory = loggerFactory;
        BackgroundTask = backgroundTask;
        ExportationWriters = exportationWriters;
        Options = options;
        AuditLogService = auditLogService;
        FormEventResolver = formEventResolver;
        DataPanelFactory = dataPanelFactory;
        GridViewFactory = gridViewFactory;
    }


    public JJFormView CreateFormView()
    {
        var form = new JJFormView(
            HttpContext,
            RepositoryServicesFacade, 
            EncryptionService, 
            LoggerFactory,
            BackgroundTask, 
            ExportationWriters, 
            Options, 
            AuditLogService, 
            FormEventResolver, 
            DataPanelFactory,
            GridViewFactory, 
            this);

        return form;
    }
    
    public JJFormView CreateFormView(string elementName)
    {
        var form = CreateFormView();
        SetFormViewParams(form, elementName);
        return form;
    }

    public JJFormView CreateFormView(FormElement formElement)
    {
        var form = CreateFormView();
        
        form.Name = "jjview" + formElement.Name.ToLower();
        form.FormElement = formElement;
        
        return form;
    }

    internal static void SetFormViewParams(JJFormView formView)
    {
        formView.ShowTitle = true;
        formView.ToolBarActions.Add(new InsertAction());
        formView.ToolBarActions.Add(new DeleteSelectedRowsAction());
        formView.ToolBarActions.Add(new LogAction());
        formView.GridActions.Add(new ViewAction());
        formView.GridActions.Add(new EditAction());
        formView.GridActions.Add(new DeleteAction());
    }

    internal void SetFormViewParams(JJFormView form, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary name cannot be empty"));

        var formEvent = FormEventResolver?.GetFormEvent(elementName);
        if (formEvent != null)
        {
            AddFormEvent(form, formEvent);
        }

        form.Name = "jjview" + elementName.ToLower();

        var metadata = RepositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);

        var dataContext = new DataContext(HttpContext, DataContextSource.Form,
            DataHelper.GetCurrentUserId(HttpContext, null));
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));

        form.FormElement = metadata.GetFormElement();
        SetFormOptions(form, metadata.UIOptions);
    }

    internal void SetFormOptions(JJFormView form, UIOptions options)
    {
        if (options == null)
            return;

        form.ToolBarActions = options.ToolBarActions.GetAll();
        form.GridActions = options.GridActions.GetAll();
        form.ShowTitle = options.Grid.ShowTitle;
        form.DataPanel.UISettings = options.Form;
        GridViewFactory.SetGridOptions(form, options.Grid);
    }

    private static void AddFormEvent(JJFormView form, IFormEvent formEvent)
    {
        form.DataImp.OnBeforeImport += formEvent.OnBeforeImport;
    }
}