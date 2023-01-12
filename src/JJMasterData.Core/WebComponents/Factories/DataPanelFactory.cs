using System;
using System.Collections;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataPanelFactory
{
    public IHttpContext HttpContext { get; }
    private readonly RepositoryServicesFacade _repositoryServicesFacade;
    private readonly JJMasterDataEncryptionService _encryptionService;
    private readonly IOptions<JJMasterDataCoreOptions> _options;
    private readonly ILoggerFactory _loggerFactory;


    public DataPanelFactory(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        JJMasterDataEncryptionService encryptionService,
        IOptions<JJMasterDataCoreOptions> options,
        ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        _repositoryServicesFacade = repositoryServicesFacade;
        _encryptionService = encryptionService;
        _options = options;
        _loggerFactory = loggerFactory;
    }
        
    public JJDataPanel CreateDataPanel(string elementName)
    {
        var dataPanel = new JJDataPanel(HttpContext, _repositoryServicesFacade,_encryptionService,_options,_loggerFactory);
            
        SetDataPanelParams(dataPanel, elementName);
            
        return dataPanel;
    }

    public JJDataPanel CreateDataPanel(FormElement formElement)
    {
        var dataPanel = new JJDataPanel(HttpContext, _repositoryServicesFacade,_encryptionService,_options,_loggerFactory);
        
        SetDataPanelParams(dataPanel, formElement);

        return dataPanel;
    }

    public JJDataPanel CreateDataPanel(
        FormElement formElement,
        Hashtable values,
        Hashtable errors,
        PageState pageState)
    {
        var dataPanel = new JJDataPanel(HttpContext, _repositoryServicesFacade,_encryptionService,_options,_loggerFactory)
            {
                Values = values,
                Errors = errors,
                PageState = pageState
            };
        SetDataPanelParams(dataPanel, formElement);

        return dataPanel;
    }

    internal static void SetDataPanelParams(JJDataPanel dataPanel)
    {
        dataPanel.Values = new Hashtable();
        dataPanel.Errors = new Hashtable();
        dataPanel.AutoReloadFormFields = true;
        dataPanel.PageState = PageState.View;
    }

    internal void SetDataPanelParams(JJDataPanel dataPanel, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));
            
        var metadata = _repositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);
        var formElement = metadata.GetFormElement();

        SetDataPanelParams(dataPanel, formElement);
        dataPanel.UISettings = metadata.UIOptions.Form;
    }

    internal void SetDataPanelParams(JJDataPanel dataPanel, FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        dataPanel.FormElement = formElement;
        dataPanel.Name = "pnl_" + formElement.Name.ToLower();
        dataPanel.RenderPanelGroup = formElement.Panels.Count > 0;
    }

}