#nullable enable

using System.Runtime.InteropServices;
using JJMasterData.Commons.Data.Entity;
using Microsoft.Extensions.Configuration;

namespace JJMasterData.Commons.Configuration.Options;

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
public class JJMasterDataCommonsOptions
{
    /// <summary>
    /// Default value: JJMasterDataResources <br></br>
    /// </summary>
    public string ResourcesTableName { get; set; }

    /// <summary>
    /// Default value: {tablename}Get <br></br>
    /// </summary>
    [ConfigurationKeyName("PrefixGetProc")]
    public string ReadProcedurePattern { get; set; }

    /// <summary>
    /// Default value: {tablename}Set <br></br>
    /// </summary>
    [ConfigurationKeyName("PrefixSetProc")]
    public string WriteProcedurePattern { get; set; }

    /// <summary>
    /// Default value: "ChangeMe" <br></br>
    /// </summary>
    public string? SecretKey { get; set; }

    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    public JJMasterDataCommonsOptions()
    {
        ResourcesTableName = "JJMasterDataResources";
        ReadProcedurePattern = "{tablename}Get";
        WriteProcedurePattern = "{tablename}Set";
    }

    public string GetReadProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameGet))
            return element.CustomProcNameGet;

        string tableName = element.TableName;

        return ReadProcedurePattern.Replace("{tablename}", tableName);
    }

    public string GetWriteProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.CustomProcNameSet))
            return element.CustomProcNameSet;

        string tableName = element.TableName;

        return WriteProcedurePattern.Replace("{tablename}", tableName);
    }

    public string GetReadProcedureName(string tableName)
    {
        var dicName = RemovePrefixChars(tableName);

        return ReadProcedurePattern.Replace("{tablename}", dicName);
    }

    public string GetWriteProcedureName(string tableName)
    {
        var dicName = RemovePrefixChars(tableName);

        return WriteProcedurePattern.Replace("{tablename}", dicName);
    }

    private static string RemovePrefixChars(string tableName)
    {
        if (tableName.ToLower().StartsWith("tb_"))
            return tableName[3..];

        if (tableName.ToLower().StartsWith("tb"))
            return tableName[2..];

        return tableName;
    }
}