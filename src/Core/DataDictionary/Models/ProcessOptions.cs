

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

    /// <summary>
    /// Process execution scope
    /// </summary>
    /// <remarks>
    /// Global<para></para>
    /// Only one import of this dictionary can be performed at a time
    /// and all users will be able to view the last log
    /// and the execution of an ongoing process.
    /// <para></para>
    /// User<para></para>
    /// Varios usuários poderão executar esse processo simultaneamente, 
    /// porém visualiza apenas o log e a execução do seu processo.
    /// </remarks>
    [JsonPropertyName("scope")]
    public ProcessScope Scope { get; set; } = ProcessScope.Global;

    public ProcessOptions DeepCopy()
    {
        return (ProcessOptions)MemberwiseClone();
    }
}