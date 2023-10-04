#nullable enable

using System.ComponentModel;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginConfigurationField
{
    public required string Name { get; set; }
    public string? Label { get; set; }
    public string NameOrLabel => Label ?? Name;
    public PluginConfigurationFieldType Type { get; set; }
    public bool Required { get; set; }
}

public enum PluginConfigurationFieldType
{
    Text,
    Number,
    Boolean,
    FormElementField
}