using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Extensions;

public static class ComponentResultExtensions
{
    public static bool IsActionResult(this ComponentResult componentResult)
    {
        return componentResult is HtmlComponentResult or JsonComponentResult;
    }   
    
    public static IActionResult ToActionResult(this ComponentResult componentResult)
    {
        if (componentResult is not (HtmlComponentResult or JsonComponentResult))
            throw new JJMasterDataException("ComponentResults of ContentType.RenderedComponent must be rendered at your View.");
        
        var content = new ContentResult
        {
            Content = componentResult.Content,
            ContentType = componentResult is HtmlComponentResult ? "text/plain" : "application/json"
        };
        
        return content;
    }    
}