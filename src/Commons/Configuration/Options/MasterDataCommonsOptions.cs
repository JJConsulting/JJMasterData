#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
public sealed class MasterDataCommonsOptions
{
    public string? ConnectionString { get; set; }
    public DataAccessProvider ConnectionProvider { get; set; }

    public List<ConnectionString> AdditionalConnectionStrings { get; set; } = [];
    
    /// <summary>
    /// Default value: tb_masterdata_resources <br></br>
    /// </summary>
    [Display(Name = "Localization Table Name")]
    public string LocalizationTableName { get; set; } = "tb_masterdata_resources";

    /// <summary>
    /// Default value: {tablename}Get <br></br>
    /// </summary>
    [Display(Name = "Read Procedure Pattern")]
    public string ReadProcedurePattern { get; set; } = "{tablename}Get";

    /// <summary>
    /// Default value: {tablename}Set <br></br>
    /// </summary>
    [Display(Name = "Write Procedure Pattern")]
    public string WriteProcedurePattern { get; set; } = "{tablename}Set";

    /// <summary>
    /// Secret key used at JJMasterDataEncryptionService
    /// </summary>
    [Display(Name = "Cryptography Secret Key")]
    public string? SecretKey { get; set; }

    [JsonIgnore]
    public static bool IsNetFramework => RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    public ConnectionString GetConnectionString(Guid? guid)
    {
        if (guid is null)
            return new ConnectionString(ConnectionString!, ConnectionProvider);

        var connectionString = AdditionalConnectionStrings?.FirstOrDefault(c => c.Guid == guid);

        if (connectionString is null)
            throw new JJMasterDataException($"ConnectionString {guid} does not exist.");

        return connectionString;
    }
    
    public string GetReadProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.ReadProcedureName))
            return element.ReadProcedureName;

        var tableName = element.TableName;

        return ReadProcedurePattern.Replace("{tablename}", tableName);
    }

    public string GetWriteProcedureName(Element element)
    {
        if (!string.IsNullOrEmpty(element.WriteProcedureName))
            return element.WriteProcedureName;

        var tableName = element.TableName;

        return WriteProcedurePattern.Replace("{tablename}", tableName);
    }

    public string GetReadProcedureName(string tableName)
    {
        var dicName = RemoveTbPrefix(tableName);

        return ReadProcedurePattern.Replace("{tablename}", dicName);
    }

    public string GetWriteProcedureName(string tableName)
    {
        var dicName = RemoveTbPrefix(tableName);

        return WriteProcedurePattern.Replace("{tablename}", dicName);
    }

    public static string RemoveTbPrefix(string tableName)
    {
        var loweredTableName = tableName.ToLowerInvariant();
        
        if (loweredTableName.StartsWith("tb_"))
            return tableName[3..];

        if (loweredTableName.StartsWith("tb"))
            return tableName[2..];

        return tableName;
    }
}