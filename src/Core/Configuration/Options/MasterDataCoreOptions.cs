#nullable enable

using System.ComponentModel.DataAnnotations;
using System.IO;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.Configuration.Options;

public sealed class MasterDataCoreOptions
{
    /// <summary>
    /// Default value: tb_masterdata
    /// </summary>
    [Display(Name = "Data Dictionary Table Name")]
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";

    /// <summary>
    /// Default value: tb_masterdata_auditlog
    /// </summary>
    [Display(Name = "Audit Log Table Name")]
    public string AuditLogTableName { get; set; } = "tb_masterdata_auditlog";
    
    #if !NET
    /// <summary>
    /// Default value: null
    /// </summary>
    public string? MasterDataUrl { get; set; }

    public bool EnableCultureProviderAtUrl { get; set; } = true;
    #endif
    
    [Display(Name = "Enable Data Dictionary Caching")]
    public bool EnableDataDictionaryCaching { get; set; } = true;
    
    /// <summary>
    /// Default value: {ApplicationPath}/JJExportationFiles
    /// </summary>
    [Display(Name = "Exportation Folder Path")]
    public string ExportationFolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(), "JJExportationFiles");
}