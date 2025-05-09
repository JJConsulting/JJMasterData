#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using Microsoft.Extensions.Configuration;


namespace JJMasterData.Commons.Configuration.Options;

/// <summary>
/// JJMasterData key/value configurations.
/// They're populated from JJMasterData section on <see cref="IConfiguration"/>, following its implementations.
/// On .NET Framework, add an <see cref="IConfiguration"/> builder to your application.
/// </summary>
public sealed class MasterDataCommonsOptions
{
    public required string ConnectionString { get; set; }
    public DataAccessProvider ConnectionProvider { get; set; }

    /// <summary>
    /// A collection of additional connection string definitions used for database connections.
    /// Each connection string includes provider-specific information and is identified by a unique GUID.
    /// This property can be used to define and manage multiple database connections within a single application.
    /// Default value: an empty list.
    /// </summary>
    public List<ConnectionString> AdditionalConnectionStrings { get; set; } = [];

    /// <summary>
    /// Pattern used to generate the names of stored procedures for reading data.
    /// The placeholder {tablename} will be replaced with the name of the table being accessed.
    /// Default value: {tablename}Get
    /// </summary>
    [Display(Name = "Read Procedure Pattern")]
    public string ReadProcedurePattern { get; set; } = "{tablename}Get";

    /// <summary>
    /// Pattern used to generate the names of stored procedures for writing data.
    /// The placeholder {tablename} will be replaced with the name of the table being accessed.
    /// Default value: {tablename}Set
    /// </summary>
    [Display(Name = "Write Procedure Pattern")]
    public string WriteProcedurePattern { get; set; } = "{tablename}Set";

    /// <summary>
    /// Secret key used at JJMasterDataEncryptionService
    /// </summary>
    [Display(Name = "Cryptography Secret Key")]
    public string? SecretKey { get; set; }

    [JsonIgnore]
    public static bool IsNetFramework { get; } = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

    internal ConnectionString GetConnectionString(Guid? guid)
    {
        if (guid is null)
            return new ConnectionString(ConnectionString!, ConnectionProvider.GetAdoNetTypeName());

        var connectionString = AdditionalConnectionStrings.Find(c => c.Guid == guid);

        if (connectionString is null)
            throw new JJMasterDataException($"ConnectionString {guid} does not exist.");

        return connectionString;
    }
    
    public string GetReadProcedureName(Element element)
    {
        string name;

        if (!string.IsNullOrEmpty(element.ReadProcedureName))
        {
            name = element.ReadProcedureName!;
        }
        else
        {
            var tableName = element.TableName;

            name = ReadProcedurePattern.Replace("{tablename}", tableName);
        }
        
        return $"[{element.Schema ?? "dbo"}].[{name}]";
    }

    public string GetWriteProcedureName(Element element)
    {
        string name;

        if (!string.IsNullOrEmpty(element.WriteProcedureName))
        {
            name = element.WriteProcedureName!;
        }
        else
        {
            var tableName = element.TableName;

            name = WriteProcedurePattern.Replace("{tablename}", tableName);
        }
        
        return $"[{element.Schema ?? "dbo"}].[{name}]";
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