using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Core.FormEvents;

public static class FormEventManager
{
    internal static Assembly[] Assemblies { get; set; }
    public static IPythonEngine PythonEngine => FormEventEngineFactory.GetEngine<IPythonEngine>();

    public static IFormEvent GetFormEvent(string name)
    {
        var formEventType = GetFormEventTypes(Assemblies).FirstOrDefault(t => t.Name.StartsWith(name));

        IFormEvent formEvent = null;

        if (formEventType != null)
            formEvent = (IFormEvent)JJService.Provider.GetService(formEventType);

        if (formEvent == null && PythonEngine != default)
            formEvent = PythonEngine.GetFormEvent(name);

        return formEvent;
    }

    public static List<string> GetFormEventMethods(IFormEvent assemblyFormEvent)
    {
        var handlers = new List<string>();

        if (assemblyFormEvent == null) return handlers;
        
        var methods = assemblyFormEvent.GetType().GetTypeInfo().DeclaredMethods.ToList();

        handlers.AddRange(methods.Select(method => method.Name));

        return handlers;
    }

    internal static IEnumerable<TypeInfo> GetFormEventTypes(Assembly[] assemblies)
    {
        if (assemblies == null)
            return new List<TypeInfo>();

        var assembliesTypes = GetTypesFromAssemblies(assemblies);

        return assembliesTypes;
    }


    private static IEnumerable<TypeInfo> GetTypesFromAssemblies(Assembly[] assemblies)
    {
        return assemblies.SelectMany(a => a.DefinedTypes.Where(x =>
            x.GetInterfaces().Any(i => i == typeof(IFormEvent)))).ToList();
    }

}
