using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Extensions;

public static class HtmlHelperExtensions
{
    public const string MasterDataFormId = "masterdata-form";
    
    public static MvcForm BeginMasterDataForm(this IHtmlHelper htmlHelper, 
        [AspMvcAction]string actionName,
        [AspMvcController]string controllerName,
        FormMethod formMethod = FormMethod.Post
        )
    {
        return htmlHelper.BeginForm(actionName, controllerName, formMethod, new { id = MasterDataFormId });
    }   
    
    
    public static MvcForm BeginMasterDataForm(this IHtmlHelper htmlHelper, 
        [AspMvcAction]string actionName,
        [AspMvcController]string controllerName,
        object routeValues,
        FormMethod formMethod = FormMethod.Post
    )
    {
        return htmlHelper.BeginForm(actionName, controllerName,routeValues, formMethod,null, new { id = MasterDataFormId });
    }   
    
    public static MvcForm BeginMasterDataForm(this IHtmlHelper htmlHelper)
    {
        return htmlHelper.BeginForm(null, null, FormMethod.Post, new { id = MasterDataFormId });
    }   
}