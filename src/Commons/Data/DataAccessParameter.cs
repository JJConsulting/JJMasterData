﻿using System;
using System.Data;
using System.Diagnostics;
using System.Text.Json.Serialization;


namespace JJMasterData.Commons.Data;

[DebuggerDisplay("Name = {Name}, Value = {Value}, Type = {Type}")]
public class DataAccessParameter
{
    /// <summary>
    /// Name of the parameter.
    /// Example
    /// MSSQL: @Foo
    /// ORACLE: p_foo
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }


    /// <summary>
    /// Value of the parameter.
    /// To send null, use DBNull.Value
    /// </summary>
    [JsonPropertyName("value")]
    public object Value { get; set; }

    /// <summary>
    /// Specifies the data type of field, a property, or a Parameter object of a .NET
    /// </summary>
    [JsonPropertyName("type")]
    public DbType Type { get; set; }

    /// <summary>
    /// Field size. For numbers is optional.
    /// </summary>
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    public bool IsNullable { get; set; } = true;
    
    [JsonPropertyName("direction")]
    public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public DataAccessParameter()
    {
    }

    public DataAccessParameter(string name, string value)
    {
        Name = name;
        Value = value;
        Type = DbType.AnsiString;
        Direction = ParameterDirection.Input;
    }

    public DataAccessParameter(string name, int value)
    {
        Name = name;
        Value = value;
        Type = DbType.Int32;
        Direction = ParameterDirection.Input;
    }

    public DataAccessParameter(string name, float value)
    {
        Name = name;
        Value = value;
        Type = DbType.Double;
        Direction = ParameterDirection.Input;
    }

    public DataAccessParameter(string name, DateTime value)
    {
        Name = name;
        Value = value;
        Type = DbType.DateTime;
        Direction = ParameterDirection.Input;
    }

    public DataAccessParameter(string name, object value)
    {
        Name = name;
        Value = value;
        Type = GetDbTypeFromObject(value);
        Direction = ParameterDirection.Input;
    }
    
    public DataAccessParameter(string name, object value, DbType type)
    {
        Name = name;
        Value = value;
        Type = type;
        Direction = ParameterDirection.Input;
    }

    public DataAccessParameter(string name, object value, DbType type, int size)
    {
        Name = name;
        Value = value;
        Type = type;
        Size = size;
        Direction = ParameterDirection.Input;
    }

    public DataAccessParameter(string name, object value, DbType type, int size, ParameterDirection direction)
    {
        Name = name;
        Value = value;
        Type = type;
        Size = size;
        Direction = direction;
    }

    public DataAccessParameter(string name, object value, DbType type, ParameterDirection direction)
    {
        Name = name;
        Value = value;
        Type = type;
        Direction = direction;
    }

    private static DbType GetDbTypeFromObject(object value)
    {
        return value switch
        {
            int => DbType.Int32,
            double => DbType.Double,
            decimal => DbType.Decimal,
            float => DbType.Double,
            string => DbType.AnsiString,
            Guid => DbType.Guid,
            DateTime => DbType.DateTime,
            bool => DbType.Boolean,
            _ => DbType.AnsiString
        };
    }
    
    public DataAccessParameter DeepCopy()
    {
        return (DataAccessParameter)MemberwiseClone();
    }
}