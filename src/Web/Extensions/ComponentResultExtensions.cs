using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Extensions;

public static class ComponentResultExtensions
{
    public static bool IsActionResult(this ComponentResult componentResult)
    {
        return AsyncComponent.CanSendResult(componentResult);
    }   
    
    public static IActionResult ToActionResult(this ComponentResult componentResult)
    {
        if (componentResult is not (ContentComponentResult or JsonComponentResult or RedirectComponentResult))
            throw new JJMasterDataException("ComponentResults of ContentType.RenderedComponent must be rendered at your View.");

        if (componentResult is RedirectComponentResult redirectComponentResult)
            return new RedirectResult(redirectComponentResult.Content);
        
        var content = new ContentResult
        {
            Content = componentResult.Content,
            StatusCode = componentResult.StatusCode,
            ContentType = componentResult is ContentComponentResult ? "text/plain" : "application/json"
        };
        
        return content;
    }    
}