#nullable  enable

using System.Runtime.InteropServices;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace JJMasterData.Commons.Settings;

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
    public int BootstrapVersion { get; set; }

    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// </summary>
    public string TableName{ get; set; }

    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// </summary>
    public string ResourcesTableName{ get; set; }

    /// <summary>
    /// Default value: JJMasterData <br></br>
    /// </summary>
    public string? AuditLogTableName{ get; set; }

    /// <summary>
    /// Default value: {table_name}Get <br></br>
    /// </summary>
    public string? PrefixGetProc{ get; set; }

    /// <summary>
    /// Default value: {table_name}Set <br></br>
    /// </summary>
    public string? PrefixSetProc{ get; set; }

    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public string? JJMasterDataUrl{ get; set; }

    /// <summary>
    /// Default value: ~/Views/Shared/_MasterDataLayout.cshtml <br></br>
    /// </summary>
    public string? LayoutPath{ get; set; }

    /// <summary>
    /// Default value: ~/Views/Shared/_MasterDataLayout.Popup.cshtml <br></br>
    /// </summary>
    public string? PopUpLayoutPath{ get; set; }

    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public string[]? ExternalAssembliesPath{ get; set; }

    public string? ExportationFolderPath{ get; set; }

    /// <summary>
    /// Default value: "ChangeMe" <br></br>
    /// </summary>
    public string SecretKey{ get; set; }

    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    internal static IConfiguration Configuration
    {
        get
        {
            var configuration = JJService.Provider.GetService(typeof(IConfiguration))!;
            return (IConfiguration)configuration;
        }
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
        return Configuration.GetSection("ConnectionProviders").GetValue<string>(name) ?? DataAccessProvider.MSSQL;
    }

    internal static string GetProcNameGet(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameGet))
            return element.CustomProcNameGet;

        string tablename = element.TableName;
        string prefix = JJService.Options.PrefixGetProc;
        prefix = string.IsNullOrEmpty(prefix) ? $"{tablename}Get" : prefix.Replace("{tablename}", tablename);

        return prefix;
    }

    internal static string GetProcNameSet(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameSet))
            return element.CustomProcNameSet;

        string tablename = element.TableName;
        string prefix = JJService.Options.PrefixSetProc;
        prefix = string.IsNullOrEmpty(prefix) ? $"{tablename}Set" : prefix.Replace("{tablename}", tablename);
        return prefix;
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

        string prefix = JJService.Options.PrefixGetProc;
        
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

        string prefix = JJService.Options.PrefixSetProc;
        prefix = string.IsNullOrEmpty(prefix) ? $"{dicname}Set" : prefix.Replace("{tablename}", dicname);

        return prefix;
    }
}