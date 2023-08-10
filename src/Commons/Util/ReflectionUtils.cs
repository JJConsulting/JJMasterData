using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JJMasterData.Commons.Util;

public class ReflectionUtils
{

    /// <summary>
    /// Try to load an assembly into the application's app domain.
    /// Loads by name first then checks for filename
    /// </summary>
    /// <param name="assemblyName">Assembly name or full path</param>
    /// <returns>null on failure</returns>
    public static Assembly LoadAssembly(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
            

        if (assembly != null)
            return assembly;

        if (!File.Exists(assemblyName)) return null;
        assembly = Assembly.LoadFrom(assemblyName);
                
        return assembly;
    }

    /// <summary>
    /// Helper routine that looks up a type name and tries to retrieve the
    /// full type reference using GetType() and if not found looking 
    /// in the actively executing assemblies and optionally loading
    /// the specified assembly name.
    /// </summary>
    /// <param name="typeName">type to load</param>
    /// <param name="assemblyName">
    /// Optional assembly name to load from if type cannot be loaded initially. 
    /// Use for lazy loading of assemblies without taking a type dependency.
    /// </param>
    /// <returns>null</returns>
    public static Type GetTypeFromName(string typeName, string assemblyName = null)
    {
        var type = Type.GetType(typeName, false);
        if (type != null)
            return type;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        // try to find manually
        foreach (var asm in assemblies)
        {
            type = asm.GetType(typeName, false);

            if (type != null)
                break;
        }
        if (type != null)
            return type;

        // see if we can load the assembly
        if (string.IsNullOrEmpty(assemblyName)) return null;
        var a = LoadAssembly(assemblyName);
        if (a == null) return null;
        type = Type.GetType(typeName, false);
        return type;
    }

    /// <summary>
    /// Retrieves a value from  a static property by specifying a type full name and property
    /// </summary>
    /// <param name="typeName">Full type name (namespace.class)</param>
    /// <param name="property">Property to get value from</param>
    /// <returns></returns>
    public static object GetStaticProperty(string typeName, string property)
    {
        var type = GetTypeFromName(typeName);
        return type == null ? null : GetStaticProperty(type, property);
    }

    /// <summary>
    /// Returns a static property from a given type
    /// </summary>
    /// <param name="type">Type instance for the static property</param>
    /// <param name="property">Property name as a string</param>
    /// <returns></returns>
    public static object GetStaticProperty(Type type, string property)
    {
        object result;
        try
        {
            result = type.InvokeMember(property, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty, null, type, null);
        }
        catch
        {
            return null;
        }

        return result;
    }

    public static bool IsMethodImplemented(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        return method != null && method.DeclaringType == type;
    }
    
    public static IEnumerable<TypeInfo> GetDefinedTypes<T>(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(a => a?.DefinedTypes.Where(x =>
            x.GetInterfaces().Any(i => i == typeof(T)))).ToList();
    }
    
    public static IEnumerable<TypeInfo> GetTypeFromProperty<T>(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(a => a?.DefinedTypes.Where(x =>
            x.GetInterfaces().Any(i => i == typeof(T)))).ToList();
    }
}