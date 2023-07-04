using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DI;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

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
        formEvent?.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(metadata));
        
        form.FormElement = metadata;

        SetFormOptions(form, metadata.Options);
    }

    internal static void SetFormOptions(JJFormView form, FormElementOptions metadataOptions)
    {
        if (metadataOptions == null) 
            return;

        form.GridView.ToolBarActions = metadataOptions.GridToolbarActions.GetAllSorted();
        form.GridView.GridActions = metadataOptions.GridTableActions.GetAllSorted();
        form.ShowTitle = metadataOptions.Grid.ShowTitle;
        form.DataPanel.FormUI = metadataOptions.Form;
        GridViewFactory.SetGridOptions(form.GridView, metadataOptions.Grid);
    }

    private static void AddFormEvent(JJFormView form, IFormEvent formEvent)
    {
        form.OnBeforeDelete += formEvent.OnBeforeDelete;
        form.OnBeforeInsert += formEvent.OnBeforeInsert;
        form.OnBeforeUpdate += formEvent.OnBeforeUpdate;

        form.OnAfterDelete += formEvent.OnAfterDelete;
        form.OnAfterInsert += formEvent.OnAfterInsert;
        form.OnAfterUpdate += formEvent.OnAfterUpdate;
    }
}
