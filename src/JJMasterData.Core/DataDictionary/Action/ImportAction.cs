using System;
using System.Runtime.Serialization;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class ImportAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "import";

        
    [DataMember(Name = "processOptions")]
    public ProcessOptions ProcessOptions { get; set; }

    public ImportAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Upload";
        Icon = IconType.Upload;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 4;
        ProcessOptions = new ProcessOptions();
        SetVisible(false);
    }
}