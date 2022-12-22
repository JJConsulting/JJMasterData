using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataImpFactory
{
    public RepositoryServicesFacade RepositoryServicesFacade { get; }
    public CoreServicesFacade CoreServicesFacade { get; }

    public DataImpFactory(RepositoryServicesFacade repositoryServicesFacade, CoreServicesFacade coreServicesFacade)
    {
        RepositoryServicesFacade = repositoryServicesFacade;
        CoreServicesFacade = coreServicesFacade;
    }
    
    public JJDataImp CreateDataImp(string elementName)
    {
        var dataImp = new JJDataImp(RepositoryServicesFacade, CoreServicesFacade);
        
        SetDataImpParams(dataImp, elementName);

        return dataImp;
    }
    
    internal void SetDataImpParams(JJDataImp dataImp, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));
        
        var metadata = RepositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);
            
        var dataContext = new DataContext(DataContextSource.Upload, DataHelper.GetCurrentUserId(null));
            
        var formEvent = CoreServicesFacade.FormEventResolver?.GetFormEvent(elementName);
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));
            
        dataImp.FormElement = metadata.GetFormElement();
        dataImp.ProcessOptions = metadata.UIOptions.ToolBarActions.ImportAction.ProcessOptions;
    }


}