﻿using Microsoft.OpenApi.Models;

namespace JJMasterData.WebApi.Swagger;

internal sealed class DataDictionaryPathItem
{
    internal string Key { get; }
    internal OpenApiPathItem PathItem { get; }

    internal DataDictionaryPathItem(string key)
    {
        Key = key;
        PathItem = new OpenApiPathItem();
    }

    internal void AddOperation(OperationType type,OpenApiOperation operation)
    {
        PathItem.AddOperation(type, operation);
    }
}
