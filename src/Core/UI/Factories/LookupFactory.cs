using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class LookupFactory
{
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IExpressionsService ExpressionsService { get; }

    public LookupFactory(
        IHttpContext httpContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        JJMasterDataUrlHelper urlHelper,
        JJMasterDataEncryptionService encryptionService,
        IExpressionsService expressionsService)
    {
        HttpContext = httpContext;
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        ExpressionsService = expressionsService;
    }

    internal JJLookup CreateLookup(FormElementField field, ExpressionOptions expOptions, object value, string panelName)
    {
        var search = new JJLookup(
            HttpContext, 
            EntityRepository,
            DataDictionaryRepository, 
            UrlHelper,
            EncryptionService,
            ExpressionsService);
        search.SetAttr(field.Attributes);
        search.Name = field.Name;
        search.SelectedValue = value?.ToString();
        search.Visible = true;
        search.DataItem = field.DataItem;
        search.AutoReloadFormFields = false;
        search.Attributes.Add("pnlname", panelName);
        search.FormValues = expOptions.FormValues;
        search.PageState = expOptions.PageState;
        search.UserValues = expOptions.UserValues;

        if (field.DataType is FieldType.Int)
        {
            search.OnlyNumbers = true;
            search.MaxLength = 11;
        }
        else
        {
            search.MaxLength = field.Size;
        }

        return search;
    }
}