using System.Reflection;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace JJMasterData.Web.Binders;

public class ExpressionModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.PropertyName is null || context.Metadata.ContainerType is null) 
            return null;

        if (context.Metadata.ModelType != typeof(string))
            return null;
        
        var hasExpressionAttribute =context
            .Metadata
            .ContainerType
            .GetProperty(context.Metadata.PropertyName)
            ?.IsDefined(typeof(ExpressionAttribute)) is true;

        return hasExpressionAttribute ? new BinderTypeModelBinder(typeof(ExpressionModelBinder)) : null;
    }
}