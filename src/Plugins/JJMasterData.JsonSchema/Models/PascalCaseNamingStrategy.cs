using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.JsonSchema.Models;

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
    /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>.
    /// </param>
    public PascalCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
    {
        ProcessDictionaryKeys = processDictionaryKeys;
        OverrideSpecifiedNames = overrideSpecifiedNames;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PascalCaseNamingStrategy"/> class.
    /// </summary>
    /// <param name="processDictionaryKeys">
    /// A flag indicating whether dictionary keys should be processed.
    /// </param>
    /// <param name="overrideSpecifiedNames">
    /// A flag indicating whether explicitly specified property names should be processed,
    /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>.
    /// </param>
    /// <param name="processExtensionDataNames">
    /// A flag indicating whether extension data names should be processed.
    /// </param>
    public PascalCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
        : this(processDictionaryKeys, overrideSpecifiedNames)
    {
        ProcessExtensionDataNames = processExtensionDataNames;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PascalCaseNamingStrategy"/> class.
    /// </summary>
    public PascalCaseNamingStrategy()
    {
    }

    public override string GetPropertyName(string name, bool hasSpecifiedName)
    {
        return base.GetPropertyName(name, hasSpecifiedName);
    }

    /// <summary>
    /// Resolves the specified property name.
    /// </summary>
    /// <param name="name">The property name to resolve.</param>
    /// <returns>The resolved property name.</returns>
    protected override string ResolvePropertyName(string name)
    {
        return ToPascalCase(name);
    }

    string ToPascalCase(string s)
    {
        // Find word parts using the following rules:
        // 1. all lowercase starting at the beginning is a word
        // 2. all caps is a word.
        // 3. first letter caps, followed by all lowercase is a word
        // 4. the entire string must decompose into words according to 1,2,3.
        // Note that 2&3 together ensure MPSUser is parsed as "MPS" + "User".

        var m = Regex.Match(s, "^(?<word>^[a-z]+|[A-Z]+|[A-Z][a-z]+)+$");
        var g = m.Groups["word"];

        // Take each word and convert individually to TitleCase
        // to generate the final output.  Note the use of ToLower
        // before ToTitleCase because all caps is treated as an abbreviation.
        var t = Thread.CurrentThread.CurrentCulture.TextInfo;
        var sb = new StringBuilder();
        foreach (var c in g.Captures.Cast<Capture>())
            sb.Append(t.ToTitleCase(c.Value.ToLower()));
        return sb.ToString();
    }
}