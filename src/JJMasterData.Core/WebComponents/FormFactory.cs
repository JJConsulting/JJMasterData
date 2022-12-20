using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using System;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DI;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents;

internal static class FormFactory
{
    public static JJFormView CreateFormView(string elementName)
    {
        var form = new JJFormView();
        SetFormViewParams(form, elementName);
        return form;
    }

    internal static void SetFormViewParams(JJFormView form, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary name cannot be empty"));

        var formEvent = JJServiceCore.FormEventResolver.GetFormEvent(elementName);
        if (formEvent != null)
        {
            AddFormEvent(form, formEvent);
        }
        
        form.Name = "jjview" + elementName.ToLower();
        
        var dictionaryRepository = JJServiceCore.DataDictionaryRepository;
        var metadata = dictionaryRepository.GetMetadata(elementName);
        
        var dataContext = new DataContext(DataContextSource.Form, DataHelper.GetCurrentUserId(null));
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));
        
        form.FormElement = metadata.GetFormElement();
        SetFormOptions(form, metadata.UIOptions);
    }

    internal static void SetFormOptions(JJFormView form, UIOptions options)
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
