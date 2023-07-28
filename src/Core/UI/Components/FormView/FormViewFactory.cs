using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Factories;

internal class FormViewFactory : IFormElementComponentFactory<JJFormView>
{
    private IHttpContext CurrentContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IFormService FormService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IFieldValuesService FieldValuesService { get; }
    private IExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IComponentFactory<JJFileDownloader> FileDownloaderFactory { get; }
    private Lazy<IFormElementComponentFactory<JJGridView>> GridViewFactory { get; }
    private Lazy<IFormElementComponentFactory<JJAuditLogView>> AuditLogViewFactory { get; }
    private Lazy<IFormElementComponentFactory<JJDataPanel>> DataPanelFactory { get; }
    private IFormEventResolver FormEventResolver { get; }

    public FormViewFactory(
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IFormService formService,
        JJMasterDataEncryptionService encryptionService,
        IFieldValuesService fieldValuesService,
        IExpressionsService expressionsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IComponentFactory<JJFileDownloader> fileDownloaderFactory,
        Lazy<IFormElementComponentFactory<JJGridView>> gridViewFactory,
        Lazy<IFormElementComponentFactory<JJAuditLogView>> auditLogViewFactory,
        Lazy<IFormElementComponentFactory<JJDataPanel>> dataPanelFactory, 
        IFormEventResolver formEventResolver
        )
    {
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        FormService = formService;
        EncryptionService = encryptionService;
        FieldValuesService = fieldValuesService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        FileDownloaderFactory = fileDownloaderFactory;
        GridViewFactory = gridViewFactory;
        AuditLogViewFactory = auditLogViewFactory;
        DataPanelFactory = dataPanelFactory;
        FormEventResolver = formEventResolver;
    }

    public JJFormView Create(FormElement formElement)
    {
        return new JJFormView(
            formElement,
            CurrentContext, 
            EntityRepository, DataDictionaryRepository, FormService,
            EncryptionService, FieldValuesService, ExpressionsService, 
            StringLocalizer, 
            FileDownloaderFactory,
            GridViewFactory,
            AuditLogViewFactory, DataPanelFactory, 
            this); // This need to be a reference to itself to prevent a recursive dependency.
    }
    
    public async Task<JJFormView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        var form = Create(formElement);
        SetFormViewParams(form, formElement);
        return form;
    }

    internal void SetFormViewParams(JJFormView form, FormElement formElement)
    {

        var formEvent = FormEventResolver.GetFormEvent(formElement.Name);
        if (formEvent != null)
        {
            AddFormEvent(form, formEvent);
        }

        form.Name = "jjview" + formElement.Name.ToLower();

        var dataContext = new DataContext(CurrentContext,DataContextSource.Form, DataHelper.GetCurrentUserId(CurrentContext,null));
        formEvent?.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(formElement));

        SetFormOptions(form, formElement.Options);
    }

    internal static void SetFormOptions(JJFormView formView, FormElementOptions metadataOptions)
    {
        if (metadataOptions == null)
            return;

        formView.GridView.ToolBarActions = metadataOptions.GridToolbarActions.GetAllSorted();
        formView.GridView.GridActions = metadataOptions.GridTableActions.GetAllSorted();
        formView.ShowTitle = metadataOptions.Grid.ShowTitle;
        formView.DataPanel.FormUI = metadataOptions.Form;
    }

    private static void AddFormEvent(JJFormView formView, IFormEvent formEvent)
    {
        formView.FormService.OnBeforeDelete += formEvent.OnBeforeDelete;
        formView.FormService.OnBeforeInsert += formEvent.OnBeforeInsert;
        formView.FormService.OnBeforeUpdate += formEvent.OnBeforeUpdate;

        formView.FormService.OnAfterDelete += formEvent.OnAfterDelete;
        formView.FormService.OnAfterInsert += formEvent.OnAfterInsert;
        formView.FormService.OnAfterUpdate += formEvent.OnAfterUpdate;
    }
}