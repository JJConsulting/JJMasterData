using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.Extensions;

public static class ExpressionProviderEnumerableExtensions
{
    public static IEnumerable<string> GetProvidersPrefixes(this IEnumerable<IExpressionProvider> expressionProviders)
    {
        return expressionProviders.Select(p => p.Prefix);
    }
    public static IEnumerable<string> GetAsyncProvidersPrefixes(this IEnumerable<IExpressionProvider> expressionProviders)
    {
        return expressionProviders.OfType<IAsyncExpressionProvider>().Select(p => p.Prefix);
    }
    
    public static IEnumerable<string> GetSyncProvidersPrefixes(this IEnumerable<IExpressionProvider> expressionProviders)
    {
        return expressionProviders.OfType<ISyncExpressionProvider>().Select(p => p.Prefix);
    }
}