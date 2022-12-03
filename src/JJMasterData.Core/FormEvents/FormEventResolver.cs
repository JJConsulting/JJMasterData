using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.FormEvents;

/// <summary>
/// Default implementation of IFormEventResolver. 
/// </summary>
internal class FormEventResolver : IFormEventResolver
{
    private IEnumerable<Assembly> Assemblies { get; }
    public FormEventResolver(IOptions<FormEventOptions> options)
    {
        Assemblies = options.Value.Assemblies;
    }
    public IFormEvent GetFormEvent(string elementName)
    {
        var assemblies = new List<Assembly>()
        {
            Assembly.GetEntryAssembly()
        };
        
        if(Assemblies?.Any() ?? false)
            assemblies.AddRange(Assemblies);
        
        var formEventType = GetFormEventTypes(assemblies)
            .FirstOrDefault(info => info.GetCustomAttribute<FormEventAttribute>().ElementName == elementName);

        if (formEventType != null)
            return (IFormEvent)System.Activator.CreateInstance(formEventType);

        return null;
    }
    private static IEnumerable<TypeInfo> GetFormEventTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(a => a.DefinedTypes.Where(x =>
            x.GetInterfaces().Any(i => i == typeof(IFormEvent)))).ToList();
    }
}