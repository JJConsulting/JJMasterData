using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Binders;

public class ExpressionModelBinder(IEnumerable<IExpressionProvider> providers) : IModelBinder
{
    private IEnumerable<string> Providers { get; } = providers.Select(p=>p.Prefix);

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
            foreach (var provider in Providers)
            {
                var providerPrefix = provider + ":";
                if (expressionValue.StartsWith(providerPrefix))
                {
                    expressionValue = expressionValue.Replace(providerPrefix, string.Empty);
                    expressionType = providerPrefix.Replace(":", string.Empty);
                }
            }
            
            var result = $"{expressionType}:{expressionValue}";
        
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
