#nullable enable

using System.Runtime.InteropServices;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
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
public class MasterDataCommonsOptions
{

    public string ConnectionString { get; set; } = null!;
    
    public DataAccessProvider ConnectionProvider { get; set; }
    
    /// <summary>
    /// Default value: JJMasterDataResources <br></br>
    /// </summary>
    public string LocalizationTableName { get; set; } = "JJMasterDataResources";

    /// <summary>
    /// Default value: {tablename}Get <br></br>
    /// </summary>
    public string ReadProcedurePattern { get; set; } = "{tablename}Get";

    /// <summary>
    /// Default value: {tablename}Set <br></br>
    /// </summary>
    public string WriteProcedurePattern { get; set; } = "{tablename}Set";

    /// <summary>
    /// Secret key used at JJMasterDataEncryptionService
    /// </summary>
    public string? SecretKey { get; set; }

    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    public string GetReadProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.ReadProcedureName))
            return element.ReadProcedureName;

        string tableName = element.TableName;

        return ReadProcedurePattern.Replace("{tablename}", tableName);
    }

    public string GetWriteProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.WriteProcedureName))
            return element.WriteProcedureName;

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