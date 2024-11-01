using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class ApiService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryRepository,stringLocalizer)
{
    public async Task<bool> SetFormElementWithApiValidation(FormElement formElement)
    {
        if (ValidateWebApi(formElement))
            await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return IsValid;
    }

    private bool ValidateWebApi(FormElement formElement)
    {
        var hasApiGetEnabled = formElement.ApiOptions is { EnableGetDetail: true, EnableGetAll: true };

        if (formElement.EnableSynchronism && !hasApiGetEnabled)
        {
            AddError(nameof(formElement.EnableSynchronism), StringLocalizer["To enable synchronism, the GET endpoints must be enabled."]);
        }

        return IsValid;
    }

}