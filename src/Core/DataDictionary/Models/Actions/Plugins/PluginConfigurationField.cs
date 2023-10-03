#nullable enable

using System.ComponentModel;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginConfigurationField
{
    public required string Name { get; set; }
    public PluginConfigurationFieldType Type { get; set; }
    public bool Required { get; set; }
}

public enum PluginConfigurationFieldType
{
    Text,
    Number,
    Boolean
}