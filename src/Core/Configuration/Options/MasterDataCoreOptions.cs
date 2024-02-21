#nullable enable

using System.IO;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.Configuration.Options;

public class MasterDataCoreOptions
{
    /// <summary>
    /// Default value: tb_masterdata
    /// </summary>
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";

    /// <summary>
    /// Default value: tb_masterdata_auditlog
    /// </summary>
    public string AuditLogTableName { get; set; } = "tb_masterdata_auditlog";
    
    #if !NET
    /// <summary>
    /// Default value: null
    /// </summary>
    public string? MasterDataUrl { get; set; }

    public bool EnableCultureProviderAtUrl { get; set; } = true;
    #endif
    /// <summary>
    /// Default value: {ApplicationPath}/JJExportationFiles
    /// </summary>
    public string ExportationFolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(), "JJExportationFiles");
}