using System;
using System.Runtime.Serialization;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
[Serializable]
public class ExportAction : BasicAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ACTION_NAME = "export";


    [DataMember(Name = "processOptions")]
    public ProcessOptions ProcessOptions { get; set; }


    public ExportAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Export";
        Icon = IconType.Download;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 3;
        ProcessOptions = new ProcessOptions();
    }
}