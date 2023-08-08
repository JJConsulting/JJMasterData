using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class LookupService : ILookupService
{
    private IHttpContext HttpContext { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    public LookupService(
        IHttpContext httpContext,
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        JJMasterDataEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper)
    {
        HttpContext = httpContext;
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
    }

    
    public string GetLookupUrl(FormElementDataItem dataItem, FormStateData formStateData, string componentName)
    {
        var elementMap = dataItem.ElementMap;

        var lookupParameters = new LookupParameters(elementMap.ElementName, componentName, elementMap.FieldKey,
            elementMap.EnableElementActions, elementMap.Filters);

        var encryptedLookupParameters =
            EncryptionService.EncryptStringWithUrlEscape(lookupParameters.ToQueryString(ExpressionsService, formStateData));
        
        return UrlHelper.GetUrl("Index", "Lookup", "MasterData",new { lookupParameters = encryptedLookupParameters });
    }
    
    public async Task<string> GetDescriptionAsync(
        FormElementDataItem dataItem,
        FormStateData formStateData,
        string searchId,
        bool allowOnlyNumbers)
    {
        if (string.IsNullOrEmpty(searchId))
            return null;

        if (dataItem.ElementMap.Filters == null)
            return null;

        if (allowOnlyNumbers)
        {
            bool isNumeric = int.TryParse(searchId, out _);
            if (!isNumeric)
                return null;
        }

        var filters = GetFilters(dataItem, searchId, formStateData);

        var fields = await GetFieldsAsync(dataItem, filters);

        if (fields == null)
            return null;

        if (string.IsNullOrEmpty(dataItem.ElementMap.FieldDescription))
            return fields[dataItem.ElementMap.FieldKey]?.ToString();

        return fields[dataItem.ElementMap.FieldDescription]?.ToString();
    }

    private IDictionary<string, dynamic> GetFilters(FormElementDataItem dataItem, string searchId, FormStateData formStateData)
    {
        var filters = new Dictionary<string, dynamic>();

        if (dataItem.ElementMap.Filters.Count > 0)
        {
            foreach (var filter in dataItem.ElementMap.Filters)
            {
                string filterParsed =
                    ExpressionsService.ParseExpression(filter.Value?.ToString(), formStateData, false);
                filters[filter.Key] = StringManager.ClearText(filterParsed);
            }
        }

        filters[dataItem.ElementMap.FieldKey] = StringManager.ClearText(searchId);
        return filters;
    }

    private async Task<IDictionary<string, dynamic>> GetFieldsAsync(FormElementDataItem dataItem, IDictionary<string, dynamic> filters)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dataItem.ElementMap.ElementName);
        return await EntityRepository.GetDictionaryAsync(formElement, filters);
    }


    

    public object GetSelectedValue(string componentName)
    {
        return HttpContext.IsPost ? HttpContext.Request.Form("id_" + componentName) : null;
    }

    
}