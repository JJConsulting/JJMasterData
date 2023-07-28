using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Factories;

internal class DataPanelFactory : IFormElementComponentFactory<JJDataPanel>
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IHttpContext HttpContext { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IFieldsService FieldsService { get; }
    private IFormValuesService FormValuesService { get; }
    private IExpressionsService ExpressionsService { get; }
    private ControlFactory ControlFactory { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    public DataPanelFactory(IEntityRepository entityRepository, IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext httpContext, JJMasterDataEncryptionService encryptionService, IFieldsService fieldsService,
        IFormValuesService formValuesService, IExpressionsService expressionsService,
        ControlFactory controlFactory, JJMasterDataUrlHelper urlHelper)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        HttpContext = httpContext;
        EncryptionService = encryptionService;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        ControlFactory = controlFactory;
        UrlHelper = urlHelper;
    }

    public JJDataPanel Create(FormElement formElement)
    {
        var dataPanel = new JJDataPanel(formElement, EntityRepository, DataDictionaryRepository, HttpContext,
            EncryptionService, UrlHelper, FieldsService, FormValuesService, ExpressionsService, ControlFactory);
        return dataPanel;
    }

    public async Task<JJDataPanel> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        var dataPanel = Create(formElement);

        return dataPanel;
    }
}