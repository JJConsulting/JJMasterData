#nullable  enable

using System;
using System.Configuration;
using System.Runtime.InteropServices;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace JJMasterData.Commons.Settings;

/// <summary>
/// Default JJMasterDataSettings
/// On .NET Core, they're populated from JJMasterData section on appsettings.json, following the property name.
/// On .NET Framework, they're populated using the specified web.config keys.
/// You can instantiate this class and inject it following the example:
/// <example>
/// <code lang="cs">
/// <![CDATA[
/// var settings = new JJMasterDataSettings()
/// {
///     BootstrapVersion = 5
/// };
/// builder.Services.AddJJMasterDataWeb().With(settings); ]]>
/// </code>
/// </example>
/// </summary>
public sealed class JJMasterDataSettings : ISettings
{
    /// <summary>
    /// Default value is the Connection String with the name "ConnectionString"
    /// </summary>
    public string? ConnectionString { get; set; }
    /// <summary> 
    /// Default value: "System.Data.SqlClient"
    /// </summary>
    public string? ConnectionProvider { get; set; } 

    /// <summary>
    /// Default value: 5 <br></br>
    /// Web.Config key: JJMasterData.BootstrapTheme
    /// </summary>
    public int BootstrapVersion { get; set; }

    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// Web.Config key: JJMasterData.TableName
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// Web.Config key: JJMasterData.TableResources
    /// </summary>
    public string ResourcesTableName { get; set; }
    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// Web.Config key: JJMasterData.AuditLog
    /// </summary>
    public string? AuditLogTableName { get; set; }

    /// <summary>
    /// Default value: {table_name}Get <br></br>
    /// Web.Config key: JJMasterData.PrefixProcGet
    /// </summary>
    public string? PrefixGetProc { get; set; }

    /// <summary>
    /// Default value: {table_name}Set <br></br>
    /// Web.Config key: JJMasterData.PrefixProcSet
    /// </summary>
    public string? PrefixSetProc { get; set; }

    /// <summary>
    /// Default value: null <br></br>
    /// Web.Config: JJMasterData.URL
    /// </summary>
    public string? JJMasterDataUrl { get; set; }

    /// <summary>
    /// Default value: ~/Views/Shared/_MasterDataLayout.cshtml <br></br>
    /// Web.Config: JJMasterData.LayoutUrl
    /// </summary>
    public string? LayoutPath { get; set; }
        
    /// <summary>
    /// Default value: ~/Views/Shared/_MasterDataLayout.Popup.cshtml <br></br>
    /// Web.Config: JJMasterData.LayoutPopupUrl
    /// </summary>
    public string? PopUpLayoutPath { get; set; }

    /// <summary>
    /// Default value: null <br></br>
    /// Web.Config: JJMasterData.ExternalAssemblies
    /// </summary>
    public string[]? ExternalAssembliesPath { get; set; }
    public string? ExportationFolderPath { get; set; }
    
    /// <summary>
    /// Default value: "ChangeMe" <br></br>
    /// Web.Config: JJMasterData.SecretKey
    /// </summary>
    public string SecretKey { get; set; }

    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    internal static IConfiguration Configuration
    {
        get
        {
            var configuration = JJService.Provider?.GetService(typeof(IConfiguration)) ?? new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true)
                .Build();

            return (IConfiguration)configuration;
        }
    }

    public JJMasterDataSettings()
    {
        ConnectionString = GetConnectionString("ConnectionString");
        ConnectionProvider = GetConnectionProvider("ConnectionString");

        BootstrapVersion = int.Parse(GetOption(nameof(BootstrapVersion)) ?? "5");
        TableName = GetOption(nameof(TableName)) ?? "JJMasterData";
        ResourcesTableName = GetOption(nameof(ResourcesTableName)) ?? "JJMasterDataResources";
        AuditLogTableName = GetOption(nameof(AuditLogTableName));
        PrefixGetProc = GetOption(nameof(PrefixGetProc));
        PrefixSetProc = GetOption(nameof(PrefixSetProc));
        JJMasterDataUrl = GetOption(nameof(JJMasterDataUrl));
        ExternalAssembliesPath = GetOption(nameof(ExternalAssembliesPath))?.Split(';');
        LayoutPath = GetOption(nameof(LayoutPath));
        PopUpLayoutPath = GetOption(nameof(PopUpLayoutPath));
        ExportationFolderPath = GetOption(nameof(ExportationFolderPath)) ?? @$"{FileIO.GetApplicationPath()}\App_Data\JJExportFiles\";
        SecretKey = GetOption(nameof(SecretKey)) ?? "ChangeMe";
    }

    private string? GetOption(string option)
    {
        if (IsNetFramework)
            return ConfigurationManager.AppSettings[GetNetFrameworkLegacyKey(option)];
        
        return Configuration.GetJJMasterData(option);
    }

    private string GetNetFrameworkLegacyKey(string key)
    {
        return key switch
        {
            nameof(BootstrapVersion) => "JJMasterData.BootstrapVersion",
            nameof(TableName) => "JJMasterData.TableName",
            nameof(ResourcesTableName) => "JJMasterData.TableResources",
            nameof(AuditLogTableName) => "JJMasterData.AuditLog",
            nameof(PrefixGetProc) => "JJMasterData.PrefixProcGet",
            nameof(PrefixSetProc) => "JJMasterData.PrefixProcSet",
            nameof(JJMasterDataUrl) => "JJMasterData.URL",
            nameof(ExternalAssembliesPath) => "JJMasterData.ExternalAssemblies",
            nameof(LayoutPath) => "JJMasterData.LayoutUrl",
            nameof(PopUpLayoutPath) => "JJMasterData.LayoutUrlPopup",
            nameof(ExportationFolderPath) => "JJMasterData.ExportationFolderPath",
            nameof(SecretKey) => "JJMasterData.SecretKey",
            _ => throw new SettingsPropertyNotFoundException("Settings option not implemented."),
        };
    }

    public string? GetConnectionString(string name)
    {
        if(IsNetFramework)
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        return Configuration.GetConnectionString(name);
    }
            

    public string? GetConnectionProvider(string name)
    {
        if (IsNetFramework)
            return ConfigurationManager.ConnectionStrings[name]?.ProviderName;
        return Configuration.GetJJMasterData("ConnectionProvider") ?? DataAccess.MSSQL;
    }

    internal static string GetProcNameGet(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameGet))
            return element.CustomProcNameGet;

        string tablename = GetTableName(element);
        string prefix = JJService.Settings.PrefixGetProc;
        prefix = string.IsNullOrEmpty(prefix) ? $"{tablename}Get" : prefix.Replace("{tablename}", tablename);

        return prefix;
    }

    internal static string GetProcNameSet(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameSet))
            return element.CustomProcNameSet;

        string tablename = GetTableName(element);
        string prefix = JJService.Settings.PrefixSetProc;
        prefix = string.IsNullOrEmpty(prefix) ? $"{tablename}Set" : prefix.Replace("{tablename}", tablename);
        return prefix;
    }
        
    private static string GetTableName(Element element)
    {
        return string.IsNullOrEmpty(element.TableName) ? element.Name : element.TableName;
    }

    public static string GetDefaultProcNameGet(string tablename)
    {
        string dicname;
        if (tablename.ToLower().StartsWith("tb_"))
            dicname = tablename.Substring(3);
        else if (tablename.ToLower().StartsWith("tb"))
            dicname = tablename.Substring(2);
        else
            dicname = tablename;

        string prefix = JJService.Settings.PrefixGetProc;
        
        prefix = string.IsNullOrEmpty(prefix) ? $"{dicname}Get" : prefix.Replace("{tablename}", dicname);

        return prefix;
    }

    public static string GetDefaultProcNameSet(string tablename)
    {
        string dicname;
        if (tablename.ToLower().StartsWith("tb_"))
            dicname = tablename.Substring(3);
        else if (tablename.ToLower().StartsWith("tb"))
            dicname = tablename.Substring(2);
        else
            dicname = tablename;

        string prefix = JJService.Settings.PrefixSetProc;
        prefix = string.IsNullOrEmpty(prefix) ? $"{dicname}Set" : prefix.Replace("{tablename}", dicname);

        return prefix;
    }
}