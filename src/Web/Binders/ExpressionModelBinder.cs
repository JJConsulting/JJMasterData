using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Binders;

public class ExpressionModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }
        
        var propertyName = bindingContext.ModelName;

        var expressionType = bindingContext.ValueProvider.GetValue($"{propertyName}-ExpressionType").FirstValue;
        var expressionValue = bindingContext.ValueProvider.GetValue($"{propertyName}-ExpressionValue").FirstValue;

        if (!string.IsNullOrWhiteSpace(expressionValue))
        {
            var result = $"{expressionType}:{expressionValue}";
        
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }

        return Task.CompletedTask;
    }
}
