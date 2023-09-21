using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
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
        var encryptedDictionaryName = context.RouteData.Values["elementName"];
        var elementName = EncryptionService.DecryptStringWithUrlUnescape(encryptedDictionaryName?.ToString());
        if (elementName != null)
        {
            context.ActionArguments["formElement"] = await DataDictionaryRepository.GetMetadataAsync(elementName);
        }
        
        await base.OnActionExecutionAsync(context, next);
    }
}