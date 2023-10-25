using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class ApiService : BaseService
{

    public ApiService(
        IValidationDictionary validationDictionary, 
        IDataDictionaryRepository dataDictionaryRepository, 
        IStringLocalizer<MasterDataResources> stringLocalizer)
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {

    }

    public async Task<bool> SetFormElementWithApiValidation(FormElement formElement)
    {
        if (ValidateApi(formElement))
            await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return IsValid;
    }

    public bool ValidateApi(FormElement formElement)
    {
        var hasApiGetEnabled = formElement.ApiOptions is { EnableGetDetail: true, EnableGetAll: true };

        if (formElement.ApiOptions.EnableGetAll && !hasApiGetEnabled)
        {
            AddError("Api", StringLocalizer["To enable sync the get APIs must be enabled."]);
        }

        return IsValid;
    }

}