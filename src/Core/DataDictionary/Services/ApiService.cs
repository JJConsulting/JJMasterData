using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class ApiService : BaseService
{

    public ApiService(
        IValidationDictionary validationDictionary, 
        IDataDictionaryRepository dataDictionaryRepository, 
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {

    }

    public bool EditApi(FormElement dicParser)
    {
        if (ValidateApi(dicParser))
            DataDictionaryRepository.InsertOrReplace(dicParser);

        return IsValid;
    }

    public bool ValidateApi(FormElement dicParser)
    {
        bool hasApiGetEnabled;

        if (dicParser.ApiOptions.EnableGetDetail & dicParser.ApiOptions.EnableGetAll)
            hasApiGetEnabled = true;
        else
            hasApiGetEnabled = false;

        if (dicParser.ApiOptions.EnableGetAll & !hasApiGetEnabled)
        {
            AddError("Api", StringLocalizer["To enable sync the get APIs must be enabled."]);
        }

        return IsValid;
    }

}