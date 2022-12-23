using System;
using System.Collections.Generic;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class FormViewFactory
{
    public GridViewFactory GridViewFactory { get; }
    public IHttpContext HttpContext { get; }
    public RepositoryServicesFacade RepositoryServicesFacade { get; }
    public CoreServicesFacade CoreServicesFacade { get; }

    public IEnumerable<IExportationWriter> ExportationWriters { get; }

    public FormViewFactory(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade,
        IEnumerable<IExportationWriter> exportationWriters,
        GridViewFactory gridViewFactory)
    {
        HttpContext = httpContext;
        RepositoryServicesFacade = repositoryServicesFacade;
        CoreServicesFacade = coreServicesFacade;
        ExportationWriters = exportationWriters;
        GridViewFactory = gridViewFactory;
    }


    public JJFormView CreateFormView(string elementName)
    {
        var form = new JJFormView(HttpContext, RepositoryServicesFacade, CoreServicesFacade, ExportationWriters, this);
        SetFormViewParams(form, elementName);
        return form;
    }

    public JJFormView CreateFormView(FormElement formElement)
    {
        return new JJFormView(formElement, HttpContext, RepositoryServicesFacade, CoreServicesFacade,
            ExportationWriters, this);
    }


    internal void SetFormViewParams(JJFormView form, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary name cannot be empty"));

        var formEvent = CoreServicesFacade.FormEventResolver?.GetFormEvent(elementName);
        if (formEvent != null)
        {
            AddFormEvent(form, formEvent);
        }

        form.Name = "jjview" + elementName.ToLower();

        var metadata = RepositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);

        var dataContext = new DataContext(HttpContext, DataContextSource.Form,
            DataHelper.GetCurrentUserId(HttpContext, null));
        formEvent?.OnMetadataLoad(dataContext, new MetadataLoadEventArgs(metadata));

        form.FormElement = metadata.GetFormElement();
        SetFormOptions(form, metadata.UIOptions);
    }

    internal void SetFormOptions(JJFormView form, UIOptions options)
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