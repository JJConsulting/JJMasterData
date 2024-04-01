using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataPanelFactory(IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext httpContext,
        IEncryptionService encryptionService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
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
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
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
            StringLocalizer,
            ComponentFactory);
    }

    public async Task<JJDataPanel> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}