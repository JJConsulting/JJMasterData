using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.Web.Components;

internal class UploadViewParams
{
    public PageState PageState { get; set; }

    public bool Enable { get; set; }

    /// <summary>
    /// Primary key values separated by | (pipe).
    /// </summary>
    public string PkValues { get; set; }
}