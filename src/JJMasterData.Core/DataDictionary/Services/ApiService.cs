using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ApiService : BaseService
{
    public ApiService(IValidationDictionary validationDictionary, IDataDictionaryRepository dataDictionaryRepository)
        : base(validationDictionary, dataDictionaryRepository)
    {
    }

    public bool EditApi(Metadata dicParser)
    {
        if (ValidateApi(dicParser))
            DataDictionaryRepository.InsertOrReplace(dicParser);

        return IsValid;
    }

    public bool ValidateApi(Metadata dicParser)
    {
        bool hasApiGetEnabled;

        if (dicParser.ApiOptions.EnableGetDetail & dicParser.ApiOptions.EnableGetAll)
            hasApiGetEnabled = true;
        else
            hasApiGetEnabled = false;

        if (dicParser.Table.Sync & !hasApiGetEnabled)
        {
            AddError("Api", Translate.Key("To enable sync the get APIs must be enabled."));
        }

        return IsValid;
    }

}