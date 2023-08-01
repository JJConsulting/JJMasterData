#nullable enable

using System.ComponentModel.DataAnnotations;
using System.IO;
using JetBrains.Annotations;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.Options;

public class JJMasterDataCoreOptions : JJMasterDataCommonsOptions
{
    /// <summary>
    /// Default value: tb_masterdata
    /// </summary>
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";
    
    /// <summary>
    /// Default value: 5 <br></br>
    /// </summary>
    [Range(3, 5)]
    public int BootstrapVersion { get; set; } = 5;

    /// <summary>
    /// Default value: tb_masterdata_auditlog
    /// </summary>
    public string AuditLogTableName { get; set; } = "tb_masterdata_auditlog";
    
    /// <summary>
    /// Default value: null
    /// </summary>
    public string? JJMasterDataUrl { get; set; }
    
    /// <summary>
    /// Default value: {ApplicationPath}/JJExportationFiles
    /// </summary>
    public string ExportationFolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(), "JJExportationFiles");
}