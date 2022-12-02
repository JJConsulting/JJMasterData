using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Core.FormEvents;

/// <summary>
/// Default implementation of IFormEventResolver. 
/// </summary>
internal class FormEventResolver : IFormEventResolver
{
    public IFormEvent GetFormEvent(string elementName)
    {
        var formEventType = GetFormEventTypes().FirstOrDefault(t => t.Name.StartsWith(elementName));

        if (formEventType != null) 
            return (IFormEvent)System.Activator.CreateInstance(formEventType);

        return null;
    }

    private static IEnumerable<TypeInfo> GetFormEventTypes()
    {
        return Assembly.GetEntryAssembly()
            ?.DefinedTypes.Where(x =>
            x.GetInterfaces().Any(i => i == typeof(IFormEvent)));
    }

}
