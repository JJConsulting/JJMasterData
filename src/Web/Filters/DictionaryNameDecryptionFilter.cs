using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public class DictionaryNameDecryptionFilter : ActionFilterAttribute
{
    private readonly JJMasterDataEncryptionService _encryptionService;
    public DictionaryNameDecryptionFilter(JJMasterDataEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ActionArguments.TryGetValue("dictionaryName", out var encryptedDictionaryName))
            return;
        try
        {
            var decryptedValue = _encryptionService.DecryptStringWithUrlUnescape(encryptedDictionaryName?.ToString());
            context.ActionArguments["dictionaryName"] = decryptedValue;
        }
        catch (Exception)
        {
            context.Result = new BadRequestResult();
        }
    }
}