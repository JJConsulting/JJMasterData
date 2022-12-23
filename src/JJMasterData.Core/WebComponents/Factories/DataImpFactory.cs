using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataImpFactory
{
    public IHttpContext HttpContext { get; }
    public RepositoryServicesFacade RepositoryServicesFacade { get; }
    public CoreServicesFacade CoreServicesFacade { get; }

    public DataImpFactory(IHttpContext httpContext, RepositoryServicesFacade repositoryServicesFacade, CoreServicesFacade coreServicesFacade)
    {
        RepositoryServicesFacade = repositoryServicesFacade;
        CoreServicesFacade = coreServicesFacade;
        HttpContext = httpContext;
    }
    
    public JJDataImp CreateDataImp(string elementName)
    {
        var dataImp = new JJDataImp(HttpContext, RepositoryServicesFacade, CoreServicesFacade);
        
        SetDataImpParams(dataImp, elementName);

        return dataImp;
    }
    
    internal void SetDataImpParams(JJDataImp dataImp, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName)); 
        
        var metadata = RepositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);
            
        var dataContext = new DataContext(HttpContext, DataContextSource.Upload, DataHelper.GetCurrentUserId(HttpContext, null));
            
        var formEvent = CoreServicesFacade.FormEventResolver?.GetFormEvent(elementName);
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));
            
        dataImp.FormElement = metadata.GetFormElement();
        dataImp.ProcessOptions = metadata.UIOptions.ToolBarActions.ImportAction.ProcessOptions;
    }


}