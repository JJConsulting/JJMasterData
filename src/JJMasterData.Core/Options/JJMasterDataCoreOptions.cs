using System.ComponentModel.DataAnnotations;
using System.IO;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.Options;

public class JJMasterDataCoreOptions
{
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";
    
    /// <summary>
    /// Default value: 5 <br></br>
    /// </summary>
    [Range(3, 5)]
    public int BootstrapVersion { get; set; } = 5;

    public string AuditLogTableName { get; set; }
    
    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public string JJMasterDataUrl { get; set; }
    public string ExportationFolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(), "JJExportationFiles");


}