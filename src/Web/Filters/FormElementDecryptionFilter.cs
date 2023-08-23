using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public class FormElementDecryptionFilter : ActionFilterAttribute
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEncryptionService EncryptionService { get; }
    public FormElementDecryptionFilter(IEncryptionService encryptionService, IDataDictionaryRepository dataDictionaryRepository)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EncryptionService = encryptionService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var encryptedDictionaryName = context.RouteData.Values["dictionaryName"];
        var dictionaryName = EncryptionService.DecryptStringWithUrlUnescape(encryptedDictionaryName?.ToString());
        if (dictionaryName != null)
        {
            context.ActionArguments["formElement"] = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        }
        
        await base.OnActionExecutionAsync(context, next);
    }
}