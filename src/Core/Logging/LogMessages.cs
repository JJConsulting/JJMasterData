#nullable enable
using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(
        EventId = 0,
        Message = "Error at {MethodName}.",
        Level = LogLevel.Error)]
    internal static partial void LogFormServiceError(
        this ILogger logger,
        Exception exception,
        string methodName);
    
    [LoggerMessage(
        EventId = 1,
        Message = "Executing expression: {Expression}",
        Level = LogLevel.Debug)]
    internal static partial void LogExpression(
        this ILogger logger,
        string? expression);
    
    [LoggerMessage(
        EventId = 2,
        Message = "Added parsed value to {Field}: {ParsedValue}",
        Level = LogLevel.Debug)]
    internal static partial void LogExpressionParsedValue(
        this ILogger logger,
        string field, 
        object? parsedValue);
    
    [LoggerMessage(
        EventId = 3,
        Message = "Error executing expression. Expression: {Expression}.",
        Level = LogLevel.Error)]
    internal static partial void LogExpressionError(
        this ILogger logger,
        string? expression);
    
    [LoggerMessage(
        EventId = 4,
        Message = "Error while executing SQL Command Action. Sql: {sql}",
        Level = LogLevel.Critical)]
    internal static partial void LogSqlActionException(this ILogger logger, Exception exception, string sql);
    
    [LoggerMessage(
        EventId = 6,
        Message = "Error executing expression. Expression: {Expression}. Field: {Field}",
        Level = LogLevel.Error)]
    internal static partial void LogExpressionErrorWithField(
        this ILogger logger,
        string? expression,
        string? field);
    
    [LoggerMessage(
        EventId = 7,
        Message = "Error recovering DataSource at GridView.\nElement Name: {ElementName}.",
        Level = LogLevel.Error)]
    internal static partial void LogGridViewDataSourceException(
        this ILogger logger,
        Exception exception,
        string elementName
        );
}