using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataImpFactory
{
    public IEntityRepository EntityRepository { get; }
    public IDataDictionaryRepository DataDictionaryRepository { get; }
    public IFormEventResolver FormEventResolver { get; }

    public DataImpFactory(IEntityRepository entityRepository,IDataDictionaryRepository dataDictionaryRepository,IFormEventResolver formEventResolver)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        FormEventResolver = formEventResolver;
    }
    
    public JJDataImp CreateDataImp(string elementName)
    {
        var dataImp = new JJDataImp(DataDictionaryRepository, EntityRepository);
        
        SetDataImpParams(dataImp, elementName);

        return dataImp;
    }
    
    internal void SetDataImpParams(JJDataImp dataImp, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));
        
        var metadata = DataDictionaryRepository.GetMetadata(elementName);
            
        var dataContext = new DataContext(DataContextSource.Upload, DataHelper.GetCurrentUserId(null));
            
        var formEvent = FormEventResolver?.GetFormEvent(elementName);
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));
            
        dataImp.FormElement = metadata.GetFormElement();
        dataImp.ProcessOptions = metadata.UIOptions.ToolBarActions.ImportAction.ProcessOptions;
    }


}