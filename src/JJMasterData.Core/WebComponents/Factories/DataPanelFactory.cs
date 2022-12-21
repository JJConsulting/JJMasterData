using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;

namespace JJMasterData.Core.WebComponents.Factories;

public class DataPanelFactory
{
    public IDataDictionaryRepository DataDictionaryRepository { get; }
        
    public IEntityRepository EntityRepository { get; }

    public DataPanelFactory(IDataDictionaryRepository dataDictionaryRepository, IEntityRepository entityRepository)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
    }
        
    public JJDataPanel CreateDataPanel(string elementName)
    {
        var dataPanel = new JJDataPanel(DataDictionaryRepository, EntityRepository);
            
        SetDataPanelParams(dataPanel, elementName);
            
        return dataPanel;
    }



    internal void SetDataPanelParams(JJDataPanel dataPanel, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));
            
        var metadata = DataDictionaryRepository.GetMetadata(elementName);
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