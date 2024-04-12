#nullable enable
using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(
        Message = "Error at {MethodName}.",
        Level = LogLevel.Error)]
    internal static partial void LogFormServiceError(
        this ILogger logger,
        Exception exception,
        string methodName);
    
    [LoggerMessage(
        Message = "Executing expression: {Expression}",
        Level = LogLevel.Debug)]
    internal static partial void LogExpression(
        this ILogger logger,
        string? expression);
    
    [LoggerMessage(
        Message = "Added parsed value to {Field}: {ParsedValue}",
        Level = LogLevel.Debug)]
    internal static partial void LogExpressionParsedValue(
        this ILogger logger,
        string field, 
        object? parsedValue);
    
    [LoggerMessage(
        Message = "Error retrieving expression at {Provider} provider. Expression: {Expression}",
        Level = LogLevel.Error)]
    internal static partial void LogExpressionError(
        this ILogger logger,
        Exception exception,
        string provider,
        string? expression);

    [LoggerMessage(Message = "Error while executing SQL Command. Sql: {Sql}", Level = LogLevel.Critical)]
    internal static partial void LogSqlCommandException(this ILogger logger, Exception exception, string sql);
    
    [LoggerMessage(
        Message = "Error retrieving expression at {Provider} provider. Expression: {Expression}. Field: {Field}.",
        Level = LogLevel.Error)]
    internal static partial void LogExpressionError(
        this ILogger logger,
        Exception exception,
        string provider,
        string? expression,
        string? fieldName);
}