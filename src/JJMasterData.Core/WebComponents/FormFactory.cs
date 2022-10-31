using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using System;

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

        form.Name = "jjview" + elementName.ToLower();
        var dicDao = new DictionaryDao();
        var dicParser = dicDao.GetDictionary(elementName);

        form.FormElement = dicParser.GetFormElement();
        SetFormptions(form, dicParser.UIOptions);

        var assemblyFormEvent = FormEventManager.GetFormEvent(elementName);
        if (assemblyFormEvent != null)
        {
            AddFormEvent(form, assemblyFormEvent);
        }

        form.InvokeOnInstanceCreated(form);
    }

    internal static void SetFormptions(JJFormView form, UIOptions options)
    {
        if (options == null) 
            return;

        form.ToolBarActions = options.ToolBarActions.GetAll();
        form.GridActions = options.GridActions.GetAll();
        form.ShowTitle = options.Grid.ShowTitle;
        form.DataPanel.UISettings = options.Form;
        GridViewFactory.SetGridOptions(form, options.Grid);
    }

    private static void AddFormEvent(JJFormView form, IFormEvent assemblyFormEvent)
    {
        foreach (var method in FormEventManager.GetFormEventMethods(assemblyFormEvent))
        {
            switch (method)
            {
                case "OnBeforeImport":
                    form.DataImp.OnBeforeImport += assemblyFormEvent.OnBeforeImport;
                    break;
                case "OnAfterImport":
                    form.DataImp.OnAfterImport += assemblyFormEvent.OnAfterImport;
                    break;
                case "OnInstanceCreated":
                    form.OnInstanceCreated += assemblyFormEvent.OnInstanceCreated;
                    break;
            }
        }
    }


}
