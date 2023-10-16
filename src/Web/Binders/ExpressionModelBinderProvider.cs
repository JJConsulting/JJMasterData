using JJMasterData.Core.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace JJMasterData.Web.Binders;

public class ExpressionModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        
        if (context.Metadata.PropertyName != null)
        {
            var propertyInfo = context.Metadata.ContainerType?.GetProperty(context.Metadata.PropertyName);
            
            var hasExpressionAttribute = propertyInfo?.GetCustomAttributes(typeof(ExpressionAttribute), false).Any() ?? false;

            if (context.Metadata.ModelType == typeof(string) && hasExpressionAttribute)
            {
                return new BinderTypeModelBinder(typeof(ExpressionModelBinder));
            }
        }

        return null;
    }
}