using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents.Factories;

public class FormViewFactory
{
    public IEntityRepository EntityRepository { get; }

    public IDataDictionaryRepository DataDictionaryRepository { get; }
    public IFormEventResolver FormEventResolver { get; }
    public GridViewFactory GridViewFactory { get; }

    public FormViewFactory(
        IDataDictionaryRepository dataDictionaryRepository, 
        IEntityRepository entityRepository,
        IFormEventResolver formEventResolver,
        GridViewFactory gridViewFactory)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        FormEventResolver = formEventResolver;
        GridViewFactory = gridViewFactory;
        EntityRepository = entityRepository;
    }

    public JJFormView CreateFormView(string elementName)
    {
        var form = new JJFormView(DataDictionaryRepository, EntityRepository, FormEventResolver, this);
        SetFormViewParams(form, elementName);
        return form;
    }
    
    public JJFormView CreateFormView(FormElement formElement)
    {
        return new JJFormView(formElement, DataDictionaryRepository, EntityRepository, FormEventResolver, this);
    }


    internal void SetFormViewParams(JJFormView form, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary name cannot be empty"));

        var formEvent = FormEventResolver?.GetFormEvent(elementName);
        if (formEvent != null)
        {
            AddFormEvent(form, formEvent);
        }

        form.Name = "jjview" + elementName.ToLower();

        var metadata = DataDictionaryRepository.GetMetadata(elementName);

        var dataContext = new DataContext(DataContextSource.Form, DataHelper.GetCurrentUserId(null));
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