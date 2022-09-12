namespace JJMasterData.Commons.Settings;

public interface ISettings
{
    string AuditLogTableName { get; set; }
    int BootstrapVersion { get; set; }
    string ConnectionProvider { get; set; }
    string ConnectionString { get; set; }

    /// <summary>
    /// Path to the data dictionary form events .DLLs
    /// </summary>
    string[] ExternalAssembliesPath { get; set; }

    /// <summary>
    /// If you are using .NET Framework, URL to the DataDictionary website.
    /// </summary>
    string JJMasterDataUrl { get; set; }
    string PrefixGetProc { get; set; }
    string PrefixSetProc { get; set; }
    string ResourcesTableName { get; set; }

    /// <summary>
    /// If you are using JJMasterData.Web .dll in your project, path to the Razor Layout.
    /// </summary>
    string LayoutPath { get; set; }
    /// <summary>
    /// If you are using JJMasterData.Web .dll in your project, path to the Razor Layout for popups.
    /// </summary>
    string PopUpLayoutPath { get; set; }
    string TableName { get; set; }
    string ExportationFolderPath { get; set; }
    
    /// <summary>
    /// Secret key used in <see cref="JJMasterData.Commons.Util.Cript"/>.
    /// </summary>
    string SecretKey { get; set; }

    string GetConnectionProvider(string name);
    string GetConnectionString(string name);
}