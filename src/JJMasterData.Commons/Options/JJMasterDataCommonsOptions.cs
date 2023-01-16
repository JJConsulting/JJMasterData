#nullable enable

using System.Runtime.InteropServices;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
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
public sealed class JJMasterDataCommonsOptions
{
    /// <summary>
    /// Default value: JJMasterDataResources <br></br>
    /// </summary>
    public string ResourcesTableName { get; set; }
    
    /// <summary>
    /// Default value: {tablename}Get <br></br>
    /// </summary>
    public string PrefixGetProc { get; set; }

    /// <summary>
    /// Default value: {tablename}Set <br></br>
    /// </summary>
    public string PrefixSetProc { get; set; }

    /// <summary>
    /// Default value: "ChangeMe" <br></br>
    /// </summary>
    public string SecretKey { get; set; }

    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    internal static IConfiguration Configuration
    {
        get
        {
            var configuration = JJService.Provider.GetService(typeof(IConfiguration))!;
            return (IConfiguration)configuration;
        }
    }

    public JJMasterDataCommonsOptions()
    {
        ResourcesTableName = "JJMasterDataResources";
        PrefixGetProc = "{tablename}Get";
        PrefixSetProc = "{tablename}Set";
        SecretKey = "ChangeMe";
    }

    public static string? GetConnectionString(string name = "ConnectionString")
    {
        if (IsNetFramework)
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;

        return Configuration.GetConnectionString(name);
    }


    public static string? GetConnectionProvider(string name = "ConnectionString")
    {
        if (IsNetFramework)
            return ConfigurationManager.ConnectionStrings[name]?.ProviderName;

        return Configuration.GetSection("ConnectionProviders").GetValue<string>(name) ??
               DataAccessProviderType.SqlServer.GetDescription();
    }

    internal static string GetReadProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameGet))
            return element.CustomProcNameGet;

        string tableName = element.TableName;
        string pattern = JJService.Options.PrefixGetProc;

        return pattern.Replace("{tablename}", tableName);
    }

    internal static string GetWriteProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameSet))
            return element.CustomProcNameSet;

        string tableName = element.TableName;
        string pattern = JJService.Options.PrefixSetProc;

        return pattern.Replace("{tablename}", tableName);
    }

    public static string GetReadProcedureName(string tableName)
    {
        var dicName = RemovePrefixChars(tableName);
        var pattern = JJService.Options.PrefixGetProc;

        return pattern.Replace("{tablename}", dicName);
    }

    public static string GetWriteProcedureName(string tableName)
    {
        var dicName = RemovePrefixChars(tableName);
        var pattern = JJService.Options.PrefixSetProc;

        return pattern.Replace("{tablename}", dicName);
    }

    private static string RemovePrefixChars(string tableName)
    {
        if (tableName.ToLower().StartsWith("tb_"))
            return tableName.Substring(3);

        if (tableName.ToLower().StartsWith("tb"))
            return tableName.Substring(2);

        return tableName;
    }
}