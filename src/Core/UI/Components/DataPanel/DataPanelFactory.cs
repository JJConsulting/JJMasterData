﻿using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class DataPanelFactory(IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext httpContext,
        IEncryptionService encryptionService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        IComponentFactory componentFactory,
        UrlRedirectService urlRedirectService)
    : IFormElementComponentFactory<JJDataPanel>
{
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private IHttpContext HttpContext { get; } = httpContext;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private FieldsService FieldsService { get; } = fieldsService;
    private FormValuesService FormValuesService { get; } = formValuesService;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private IComponentFactory ComponentFactory { get; } = componentFactory;
    private UrlRedirectService UrlRedirectService { get; } = urlRedirectService;

    public JJDataPanel Create(FormElement formElement)
    {
        return new JJDataPanel(
            formElement, 
            EntityRepository, 
            HttpContext,
            EncryptionService, 
            FieldsService, 
            FormValuesService, 
            ExpressionsService,
            UrlRedirectService,
            ComponentFactory);
    }

    public async Task<JJDataPanel> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}