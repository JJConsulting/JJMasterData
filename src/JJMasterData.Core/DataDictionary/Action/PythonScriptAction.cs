using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class PythonScriptAction : BasicAction
{
    /// <summary>
    /// PythonScript
    /// </summary>
    [DataMember(Name = "pythonScript")]
    public string PythonScript { get; set; }

    /// <summary>
    /// Apply only on the selected lines (default=false)
    /// </summary>
    /// <remarks>
    /// Only for toolbar context.
    /// </remarks>
    [DataMember(Name = "applyOnSelected")]
    public bool ApplyOnSelected { get; set; }
    public override bool IsUserCreated => true;
    
    public PythonScriptAction()
    {
        Icon = IconType.Play;
    }
}