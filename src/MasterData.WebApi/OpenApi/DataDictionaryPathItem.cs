using Microsoft.OpenApi;

namespace JJMasterData.WebApi.OpenApi;

internal sealed class DataDictionaryPathItem
{
    internal string Key { get; }
    internal OpenApiPathItem PathItem { get; }

    internal DataDictionaryPathItem(string key)
    {
        Key = key;
        PathItem = new OpenApiPathItem
        {
            Summary = key
        };
    }

    internal void AddOperation(HttpMethod type,OpenApiOperation operation)
    {
        PathItem.AddOperation(type, operation);
    }
}
