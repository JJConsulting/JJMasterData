using System.Collections.Generic;
using System.Reflection;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Attributes;

namespace JJMasterData.Core.Events.Extensions;

public static class EventHandlerExtensions
{
    public static IEnumerable<string> GetCustomizedFields(this IEventHandler eventHandler)
    {
        return eventHandler.GetType().GetCustomAttribute<CustomizedFieldsAttribute>()?.FieldNames ?? [];
    }
}