
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models;

public class ProcessOptions
{
    /// <summary>
    /// SQL command to be executed before starting the import process
    /// </summary>
    [JsonPropertyName("commandBeforeProcess")]
    public string CommandBeforeProcess { get; set; }

    /// <summary>
    /// SQL command to be executed at the end of the import process
    /// </summary>
    [JsonPropertyName("commandAfterProcess")]
    public string CommandAfterProcess { get; set; }
    
    [JsonPropertyName("scope")]
    public ProcessScope Scope { get; set; } = ProcessScope.User;

    public ProcessOptions DeepCopy()
    {
        return (ProcessOptions)MemberwiseClone();
    }
}