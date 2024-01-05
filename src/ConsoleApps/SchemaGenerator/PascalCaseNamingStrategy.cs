using System.Text;
using System.Text.RegularExpressions;
using JJMasterData.Commons.Util;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.SchemaGenerator;

/// <summary>
/// Uses Pascal case naming strategy, first letter is capitalized, and first letter
/// of subsequent words are capitalized.  This is the recommended naming strategy
/// for .NET classes, interfaces, properties, method names, etc.
/// Example: SomePropertyName
/// </summary>
public class PascalCaseNamingStrategy : NamingStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PascalCaseNamingStrategy"/> class.
    /// </summary>
    /// <param name="processDictionaryKeys">
    /// A flag indicating whether dictionary keys should be processed.
    /// </param>
    /// <param name="overrideSpecifiedNames">
    /// A flag indicating whether explicitly specified property names should be processed,
    /// e.g. a property name customized with a JsonPropertyAttribute.
    /// </param>
    public PascalCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
    {
        ProcessDictionaryKeys = processDictionaryKeys;
        OverrideSpecifiedNames = overrideSpecifiedNames;
    }

    /// <summary>
    /// Resolves the specified property name.
    /// </summary>
    /// <param name="name">The property name to resolve.</param>
    /// <returns>The resolved property name.</returns>
    protected override string ResolvePropertyName(string name)
    {
        return StringManager.ToPascalCase(name);
    }
}