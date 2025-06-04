namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Execution scope of the process
/// </summary>
public enum ProcessScope
{
    /// <summary>
    /// Global
    /// </summary>
    /// <remarks>
    /// Only one import of this dictionary can be executed at a time, 
    /// and all users can view the last log 
    /// and the execution of a process in progress.
    /// </remarks>
    Global = 0,

    /// <summary>
    /// User
    /// </summary>
    /// <remarks>
    /// Multiple users can execute this process simultaneously, 
    /// but each can only view the log and execution of their own process.
    /// </remarks>
    User = 1
}
