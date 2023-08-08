using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public class FormElementDecryptionFilter : ActionFilterAttribute
{
    private IDataDictionaryService DataDictionaryService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    public FormElementDecryptionFilter(JJMasterDataEncryptionService encryptionService, IDataDictionaryService dataDictionaryService)
    {
        DataDictionaryService = dataDictionaryService;
        EncryptionService = encryptionService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var encryptedDictionaryName = context.RouteData.Values["dictionaryName"];
        var dictionaryName = EncryptionService.DecryptStringWithUrlUnescape(encryptedDictionaryName?.ToString());
        if (dictionaryName != null)
        {
            context.ActionArguments["formElement"] = await DataDictionaryService.GetMetadataAsync(dictionaryName);
        }
        
        await base.OnActionExecutionAsync(context, next);
    }
}