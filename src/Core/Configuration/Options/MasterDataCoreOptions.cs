#nullable enable

using System.IO;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.Configuration.Options;

public class MasterDataCoreOptions : MasterDataCommonsOptions
{
    /// <summary>
    /// Default value: tb_masterdata
    /// </summary>
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";

    /// <summary>
    /// Default value: tb_masterdata_auditlog
    /// </summary>
    public string AuditLogTableName { get; set; } = "tb_masterdata_auditlog";
    
    /// <summary>
    /// Default value: null
    /// </summary>
    public string? MasterDataUrl { get; set; }
    
    /// <summary>
    /// Default value: {ApplicationPath}/JJExportationFiles
    /// </summary>
    public string ExportationFolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(), "JJExportationFiles");
}