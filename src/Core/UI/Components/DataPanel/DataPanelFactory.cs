using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class DataPanelFactory : IFormElementComponentFactory<JJDataPanel>
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IHttpContext HttpContext { get; }
    private IEncryptionService EncryptionService { get; }
    private FieldsService FieldsService { get; }
    private FormValuesService FormValuesService { get; }
    private ExpressionsService ExpressionsService { get; }
    private IComponentFactory ComponentFactory { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    public DataPanelFactory(IEntityRepository entityRepository, 
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext httpContext, 
        IEncryptionService encryptionService, 
        FieldsService fieldsService,
        FormValuesService formValuesService, 
        ExpressionsService expressionsService,
        IComponentFactory componentFactory,
        JJMasterDataUrlHelper urlHelper)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        HttpContext = httpContext;
        EncryptionService = encryptionService;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        ComponentFactory = componentFactory;
        UrlHelper = urlHelper;
    }

    public JJDataPanel Create(FormElement formElement)
    {
        return new JJDataPanel(
            formElement, 
            EntityRepository, 
            HttpContext,
            EncryptionService, 
            UrlHelper, 
            FieldsService, 
            FormValuesService, 
            ExpressionsService,
            ComponentFactory);
    }

    public async Task<JJDataPanel> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}