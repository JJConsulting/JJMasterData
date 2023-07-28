using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Factories;

internal class FormViewFactory : IFormElementComponentFactory<JJFormView>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly IHttpContext _currentContext;
    private readonly IFormEventResolver _formEventResolver;

    public FormViewFactory(IServiceProvider serviceProvider, 
                           IDataDictionaryRepository dataDictionaryRepository,
                           IHttpContext currentContext,
                           IFormEventResolver formEventResolver)
    {
        _serviceProvider = serviceProvider;
        _dataDictionaryRepository = dataDictionaryRepository;
        _currentContext = currentContext;
        _formEventResolver = formEventResolver;
    }

    public JJFormView Create(FormElement formElement)
    {
        return new JJFormView(formElement, _serviceProvider);
    }
    
    public async Task<JJFormView> CreateAsync(string elementName)
    {
        var formElement = await _dataDictionaryRepository.GetMetadataAsync(elementName);
        var form = Create(formElement);
        SetFormViewParams(form, formElement);
        return form;
    }

    internal void SetFormViewParams(JJFormView form, FormElement formElement)
    {

        var formEvent = _formEventResolver.GetFormEvent(formElement.Name);
        if (formEvent != null)
        {
            AddFormEvent(form, formEvent);
        }

        form.Name = "jjview" + formElement.Name.ToLower();

        var userId = DataHelper.GetCurrentUserId(_currentContext, null);
        var dataContext = new DataContext(_currentContext, DataContextSource.Form, userId);
        formEvent?.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(formElement));

        SetFormOptions(form, formElement.Options);
    }

    internal static void SetFormOptions(JJFormView formView, FormElementOptions metadataOptions)
    {
        if (metadataOptions == null)
            return;

        formView.GridView.ToolBarActions = metadataOptions.GridToolbarActions.GetAllSorted();
        formView.GridView.GridActions = metadataOptions.GridTableActions.GetAllSorted();
        formView.ShowTitle = metadataOptions.Grid.ShowTitle;
        formView.DataPanel.FormUI = metadataOptions.Form;
    }

    private static void AddFormEvent(JJFormView formView, IFormEvent formEvent)
    {
        formView.FormService.OnBeforeDelete += formEvent.OnBeforeDelete;
        formView.FormService.OnBeforeInsert += formEvent.OnBeforeInsert;
        formView.FormService.OnBeforeUpdate += formEvent.OnBeforeUpdate;

        formView.FormService.OnAfterDelete += formEvent.OnAfterDelete;
        formView.FormService.OnAfterInsert += formEvent.OnAfterInsert;
        formView.FormService.OnAfterUpdate += formEvent.OnAfterUpdate;
    }
}