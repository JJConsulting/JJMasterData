using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class DataPanelFactory
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IHttpContext HttpContext { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IFieldsService FieldsService { get; }
    private IFormValuesService FormValuesService { get; }
    private IExpressionsService ExpressionsService { get; }
    private FieldControlFactory FieldControlFactory { get; }
    private ScriptsHelper ScriptsHelper { get; }

    public DataPanelFactory(IEntityRepository entityRepository, IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext httpContext, JJMasterDataEncryptionService encryptionService, IFieldsService fieldsService,
        IFormValuesService formValuesService, IExpressionsService expressionsService,
        FieldControlFactory fieldControlFactory, ScriptsHelper scriptsHelper)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        HttpContext = httpContext;
        EncryptionService = encryptionService;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        FieldControlFactory = fieldControlFactory;
        ScriptsHelper = scriptsHelper;
    }

    public JJDataPanel CreateDataPanel(FormElement formElement)
    {
        var dataPanel = new JJDataPanel(formElement, EntityRepository, DataDictionaryRepository, HttpContext,
            EncryptionService, FieldsService, FormValuesService, ExpressionsService, FieldControlFactory,
            ScriptsHelper);
        return dataPanel;
    }

    public async Task<JJDataPanel> CreateDataPanelAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        var dataPanel = CreateDataPanel(formElement);

        return dataPanel;
    }
}