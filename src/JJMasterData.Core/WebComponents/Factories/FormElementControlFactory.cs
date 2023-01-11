using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.WebComponents.Factories;

internal class FormElementControlFactory
{
    public readonly EventHandler<ActionEventArgs> OnRenderAction;

    internal ActionManager ActionManager { get; }

    public string PanelName { get; private set; }

    public ExpressionOptions ExpressionOptions { get; private set; }

    public FormElement FormElement { get; set; }
    public IHttpContext HttpContext { get; }
    internal RepositoryServicesFacade RepositoryServicesFacade { get; }
    internal IDataDictionaryRepository DataDictionaryRepository { get; }
    internal IEnumerable<IExportationWriter> ExportationWriters { get; }
    internal IEntityRepository EntityRepository { get; }
    public TextGroupFactory TextGroupFactory { get; }
    
    internal IOptions<JJMasterDataCoreOptions> Options { get; }
 
    internal ILoggerFactory LoggerFactory { get; }
    public JJMasterDataEncryptionService EncryptionService { get; }
    
    public FormElementControlFactory(
        JJDataPanel dataPanel,
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        IEnumerable<IExportationWriter> exportationWriters,
        JJMasterDataEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IOptions<JJMasterDataCoreOptions> options)
    {
        RepositoryServicesFacade = repositoryServicesFacade;
        EntityRepository = repositoryServicesFacade.EntityRepository;
        DataDictionaryRepository = repositoryServicesFacade.DataDictionaryRepository;
        ExportationWriters = exportationWriters;
        LoggerFactory = loggerFactory;

        ActionManager = new ActionManager(dataPanel.FormElement,
            new ExpressionManager(new Hashtable(), dataPanel.EntityRepository, httpContext, LoggerFactory),
            repositoryServicesFacade.DataDictionaryRepository,
            encryptionService,
            options,
            dataPanel.Name);
        OnRenderAction += dataPanel.OnRenderAction;
        FormElement = dataPanel.FormElement;
        PanelName = dataPanel.Name;
        HttpContext = httpContext;
        TextGroupFactory = new TextGroupFactory(HttpContext);
        ExpressionOptions = new ExpressionOptions(dataPanel.UserValues, dataPanel.Values, dataPanel.PageState,
            dataPanel.EntityRepository);
    }

    public FormElementControlFactory(
        FormElement formElement,
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        JJMasterDataEncryptionService encryptionService,
        IOptions<JJMasterDataCoreOptions> options,
        ILoggerFactory loggerFactory,
        ExpressionManager expressionManager,
        ExpressionOptions expressionOptions,
        string panelName)
    {
        EntityRepository = repositoryServicesFacade.EntityRepository;
        DataDictionaryRepository = repositoryServicesFacade.DataDictionaryRepository;
        FormElement = formElement;
        HttpContext = httpContext;
        RepositoryServicesFacade = repositoryServicesFacade;
        EncryptionService = encryptionService;
        ExpressionOptions = expressionOptions;
        LoggerFactory = loggerFactory;
        Options = options;
        PanelName = panelName;
        TextGroupFactory = new TextGroupFactory(HttpContext);
        ActionManager = new ActionManager(FormElement, expressionManager,
            repositoryServicesFacade.DataDictionaryRepository, encryptionService,
            options, panelName);
    }



    public JJBaseControl CreateControl(FormElementField field, object value)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField can not be null");

        JJBaseControl baseView;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                baseView = JJComboBox.GetInstance(field, HttpContext, EntityRepository, ExpressionOptions,
                    LoggerFactory, value);
                break;
            case FormComponent.Search:
                baseView = JJSearchBox.GetInstance(field, HttpContext, ExpressionOptions, LoggerFactory, value,
                    PanelName);
                break;
            case FormComponent.Lookup:
                baseView = JJLookup.GetInstance(field, HttpContext, DataDictionaryRepository,EncryptionService,Options,LoggerFactory,
                    ExpressionOptions, value, PanelName);
                break;
            case FormComponent.CheckBox:
                baseView = JJCheckBox.GetInstance(field, HttpContext, value);

                if (ExpressionOptions.PageState != PageState.List)
                    baseView.Text = string.IsNullOrEmpty(field.Label) ? field.Name : field.Label;

                break;
            case FormComponent.TextArea:
                baseView = JJTextArea.GetInstance(field, value, HttpContext);
                break;
            case FormComponent.Slider:
                baseView = JJSlider.GetInstance(field, value, HttpContext);
                break;
            case FormComponent.File:
                if (ExpressionOptions.PageState == PageState.Filter)
                {
                    baseView = TextGroupFactory.CreateTextGroup(field, value);
                }
                else
                {
                    var textFile = JJTextFile.GetInstance(
                        FormElement,
                        field,
                        HttpContext,
                        RepositoryServicesFacade,
                        ExportationWriters,
                        EncryptionService,
                        new GridViewFactory(HttpContext,RepositoryServicesFacade,EncryptionService,LoggerFactory,ExportationWriters,null,Options,null),
                        LoggerFactory,
                        ExpressionOptions, value, PanelName);
                    baseView = textFile;
                }

                break;
            default:
                var textGroup = TextGroupFactory.CreateTextGroup(field, value);


                if (ExpressionOptions.PageState == PageState.Filter)
                {
                    textGroup.Actions = textGroup.Actions.Where(a => a.ShowInFilter).ToList();
                }
                else
                {
                    AddUserActions(textGroup, field);
                }

                baseView = textGroup;

                break;
        }

        baseView.ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly &&
                            ExpressionOptions.PageState != PageState.Filter;


        return baseView;
    }



    private void AddUserActions(JJTextGroup textGroup, FormElementField f)
    {
        var actions = f.Actions.GetAll().FindAll(x => x.IsVisible);
        foreach (var action in actions)
        {
            var link = ActionManager.GetLinkField(action, ExpressionOptions.FormValues, ExpressionOptions.PageState,
                PanelName);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, ExpressionOptions.FormValues);
                onRender.Invoke(this, args);
            }

            if (link != null)
                textGroup.Actions.Add(link);
        }
    }
}