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
    public const string ActionName = "import";

        
    [DataMember(Name = "processOptions")]
    public ProcessOptions ProcessOptions { get; set; }
    public override bool IsUserCreated => false;
    public ImportAction()
    {
        Name = ActionName;
        ToolTip = "Upload";
        Icon = IconType.Upload;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 4;
        ProcessOptions = new ProcessOptions();
        SetVisible(false);
    }
}