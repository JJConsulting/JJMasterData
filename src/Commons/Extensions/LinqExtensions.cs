using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JJMasterData.Commons.Extensions;

public static class LinqExtensions 
{
    private static PropertyInfo GetPropertyInfo(Type objType, string name)
    {
        var properties = objType.GetProperties();
        var matchedProperty = properties.FirstOrDefault (p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (matchedProperty == null)
            throw new ArgumentException("name");

        return matchedProperty;
    }
    private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
    {
        var paramExpr = Expression.Parameter(objType);
        var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
        var expr = Expression.Lambda(propAccess, paramExpr);
        return expr;
    }

    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
            return query;
        
        string[] splintedOrder = orderBy.Split(' ');
        string orderByField = splintedOrder[0];
        string orderByDirection = "ASC";
        if (splintedOrder.Length > 1)
            orderByDirection = splintedOrder[1].ToUpper(); 
        
        var propInfo = GetPropertyInfo(typeof(T), orderByField);
        var expr = GetOrderExpression(typeof(T), propInfo);
        var methodName = orderByDirection.Equals("ASC") ? "OrderBy" : "OrderByDescending";
        var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2);
        var genericMethod = method!.MakeGenericMethod(typeof(T), propInfo.PropertyType);     
        return (IEnumerable<T>) genericMethod.Invoke(null, [query, expr.Compile()]);
    }

 
}