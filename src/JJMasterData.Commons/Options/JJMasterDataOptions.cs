#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace JJMasterData.Commons.Options;

/// <summary>
/// JJMasterData key/value configurations.
/// They're populated from JJMasterData section on <see cref="IConfiguration"/>, following it's implementations.
/// On .NET Framework, add a IConfiguration builder to your application.
/// <example>
/// <code lang="cs">
/// <![CDATA[
/// builder.Services.AddOptions<JJMasterDataOptions>();
/// ]]>
/// </code>
/// </example>
/// </summary>
public sealed class JJMasterDataOptions
{
    /// <summary>
    /// Default value: 5 <br></br>
    /// </summary>
    [Range(3, 5)]
    public int BootstrapVersion { get; set; } = 5;

    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// </summary>
    public string TableName { get; set; } = "JJMasterData";

    /// <summary>
    /// Default value: JJMasterDataResources <br></br>
    /// </summary>
    public string ResourcesTableName { get; set; } = "JJMasterDataResources";

    /// <summary>
    /// Default value: JJMasterDataAuditLog <br></br>
    /// </summary>
    public string AuditLogTableName { get; set; } = "JJMasterDataAuditLog";

    /// <summary>
    /// Default value: {TableName}Get <br></br>
    /// </summary>
    public string ReadProcedurePattern{ get; set; } = "{table_name}Get";

    /// <summary>
    /// Default value: {TableName}Set <br></br>
    /// </summary>
    public string WriteProcedurePattern { get; set; } = "{table_name}Set";

    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public string? JJMasterDataUrl{ get; set; }

    /// <summary>
    /// Default value: _MasterDataLayout <br></br>
    /// </summary>
    public string? LayoutPath { get; set; } = "_MasterDataLayout";

    /// <summary>
    /// Default value:_MasterDataLayout.Popup <br></br>
    /// </summary>
    public string? PopUpLayoutPath{ get; set; }= "_MasterDataLayout.Popup";

    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public string[]? ExternalAssembliesPath{ get; set; }

    public string ExportationFolderPath { get; set; } = @$"{FileIO.GetApplicationPath()}\App_Data\JJExportFiles\";

    /// <summary>
    /// Default value: "ChangeMe" <br></br>
    /// </summary>
    public string SecretKey { get; set; } = "ChangeMe";

    public LoggerOptions Logger { get; set; } = new();

    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    internal static IConfiguration Configuration
    {
        get
        {
            var configuration = JJService.Provider.GetService(typeof(IConfiguration))!;
            return (IConfiguration)configuration;
        }
    }
    
    public static string? GetConnectionString(string name = "ConnectionString")
    {
        if(IsNetFramework)
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        return Configuration.GetConnectionString(name);
    }
            

    public static string? GetConnectionProvider(string name = "ConnectionString")
    {
        if (IsNetFramework)
            return ConfigurationManager.ConnectionStrings[name]?.ProviderName;
        return Configuration.GetSection("ConnectionProviders").GetValue<string>(name) ?? DataAccessProviderType.SqlServer.GetDescription();
    }

    internal static string GetReadProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.ReadProcedureName))
            return element.ReadProcedureName;

        string tablename = element.TableName;
        
        string pattern = JJService.Options.ReadProcedurePattern;

        return pattern.Replace("{table_name}", tablename);
        
    }

    internal static string GetWriteProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.WriteProcedureName))
            return element.WriteProcedureName;

        string tablename = element.TableName;
        string pattern = JJService.Options.WriteProcedurePattern;
        
        return pattern.Replace("{table_name}", tablename);
    }

    public static string GetReadProcedureName(string tablename)
    {
        var dicname = RemovedTbFromName(tablename);
        
        string pattern = JJService.Options.ReadProcedurePattern;

        return pattern.Replace("{table_name}", dicname);
    }

    public static string GetWriteProcedureName(string tablename)
    {
        var dicname = RemovedTbFromName(tablename);

        string pattern = JJService.Options.WriteProcedurePattern;

        return pattern.Replace("{table_name}", dicname);
    }

    private static string RemovedTbFromName(string tablename)
    {
        string dicname;
        if (tablename.ToLower().StartsWith("tb_"))
            dicname = tablename.Substring(3);
        else if (tablename.ToLower().StartsWith("tb"))
            dicname = tablename.Substring(2);
        else
            dicname = tablename;
        return dicname;
    }
}