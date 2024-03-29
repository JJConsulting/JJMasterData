using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.Extensions;

public static class ExpressionProviderEnumerableExtensions
{
    public static string[] GetProvidersPrefixes(this IEnumerable<IExpressionProvider> expressionProviders)
    {
        return expressionProviders.Select(p => p.Prefix).ToArray();
    }
    public static string[] GetAsyncProvidersPrefixes(this IEnumerable<IExpressionProvider> expressionProviders)
    {
        return expressionProviders.Where(p => p is IAsyncExpressionProvider).Select(p => p.Prefix).ToArray();
    }
    
    public static string[] GetSyncProvidersPrefixes(this IEnumerable<IExpressionProvider> expressionProviders)
    {
        return expressionProviders.Where(p => p is ISyncExpressionProvider).Select(p => p.Prefix).ToArray();
    }
}