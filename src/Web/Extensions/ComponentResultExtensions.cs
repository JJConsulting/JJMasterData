using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc;
using ContentType = JJMasterData.Core.UI.Components.ComponentResult.ContentType;

namespace JJMasterData.Web.Extensions;

public static class ComponentResultExtensions
{
    public static bool IsActionResult(this ComponentResult componentResult)
    {
        return componentResult.Type is ContentType.HtmlData or ContentType.JsonData;
    }   
    
    public static IActionResult ToActionResult(this ComponentResult componentResult)
    {
        if (componentResult.Type is not (ContentType.HtmlData or ContentType.JsonData))
            throw new JJMasterDataException("ComponentResults of ContentType.RenderedComponent must be rendered at your View.");
        
        var content = new ContentResult
        {
            Content = componentResult.Content,
            ContentType = componentResult.Type is ContentType.HtmlData ? "text/plain" : "application/json"
        };
        
        return content;
    }    
}