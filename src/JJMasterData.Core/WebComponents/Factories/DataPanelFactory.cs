using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataPanelFactory
{
    public IHttpContext HttpContext { get; }
    private readonly RepositoryServicesFacade _repositoryServicesFacade;
    private readonly CoreServicesFacade _coreServicesFacade;

    public DataPanelFactory(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade)
    {
        HttpContext = httpContext;
        _repositoryServicesFacade = repositoryServicesFacade;
        _coreServicesFacade = coreServicesFacade;
    }
        
    public JJDataPanel CreateDataPanel(string elementName)
    {
        var dataPanel = new JJDataPanel(HttpContext, _repositoryServicesFacade, _coreServicesFacade);
            
        SetDataPanelParams(dataPanel, elementName);
            
        return dataPanel;
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