using System;
using System.Data;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Dao;

[Serializable]
[DataContract]
public class DataAccessParameter
{
    /// <summary>
    /// Name of the parameter.
    /// Example
    /// MSSQL: @Foo
    /// ORACLE: p_foo
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }


    /// <summary>
    /// Value of the parameter.
    /// To send null, use DBNull.Value
    /// </summary>
    [DataMember(Name = "value")]
    public object Value { get; set; }

    /// <summary>
    /// Specifies the data type of a field, a property, or a Parameter object of a .NET
    /// </summary>
    [DataMember(Name = "type")]
    public DbType Type { get; set; }

    /// <summary>
    /// Field size. For numbers is optional.
    /// </summary>
    [DataMember(Name = "size")]
    public int Size { get; set; }
    
    [DataMember(Name = "direction")]
    public ParameterDirection Direction { get; set; }

    public DataAccessParameter() { }

    public DataAccessParameter(string name, string value)
    {
        Name = name;
        Value = value;
        Type = DbType.String;
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
    
}