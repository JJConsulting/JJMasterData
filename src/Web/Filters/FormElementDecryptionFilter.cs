using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public class FormElementDecryptionFilter(IEncryptionService encryptionService,
        IDataDictionaryRepository dataDictionaryRepository)
    : ActionFilterAttribute
{
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var encryptedDictionaryName = context.RouteData.Values["elementName"];
        var elementName = EncryptionService.DecryptStringWithUrlUnescape(encryptedDictionaryName?.ToString());
        if (elementName != null)
        {
            context.ActionArguments["formElement"] = await DataDictionaryRepository.GetFormElementAsync(elementName);
        }
        
        await base.OnActionExecutionAsync(context, next);
    }
}