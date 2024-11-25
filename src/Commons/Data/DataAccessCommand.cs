#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Text.Json.Serialization;
using JetBrains.Annotations;


namespace JJMasterData.Commons.Data;

[PublicAPI]
[DebuggerDisplay("Sql = {Sql}, Type = {Type}")]
public class DataAccessCommand
{
    [JsonPropertyName("cmdType")]
    public CommandType Type { get; set; }

    [JsonPropertyName("sql")]
    [Display(Name = "Sql Command")]
    public string Sql { get; set; }

    [JsonPropertyName("parameters")]
    [JsonInclude]
    public List<DataAccessParameter> Parameters { get; private set; }
    
    public DataAccessCommand()
    {
        Sql = string.Empty;
        Type = CommandType.Text;
        Parameters = [];
    }
    
    public DataAccessCommand(string sql) : this()
    {
        Sql = sql;
    }

    public DataAccessCommand(string sql, List<DataAccessParameter> parameters)
    {
        Sql = sql;
        Parameters = parameters;
        Type = CommandType.Text;
    }

    public DataAccessCommand(string sql, List<DataAccessParameter> parameters, CommandType type) : this(sql,parameters)
    {
        Type = type;
    }

    public void AddParameter(string name, object? value, DbType dbType)
    {
        Parameters.Add(new DataAccessParameter(name, value, dbType));
    }

    public static DataAccessCommand FromFormattableString(FormattableString formattableString)
    {
        var parameters = new List<DataAccessParameter>();
        
        var argCount = formattableString.ArgumentCount;
        
        var arguments = new object[argCount];
        
        for (var i = 0; i < argCount; i++)
        {
            var paramName = $"@p{i}";
            arguments[i] = paramName;
            parameters.Add(new DataAccessParameter(paramName, formattableString.GetArgument(i)));
        }
        
        var sql = string.Format(formattableString.Format, arguments);
        
        return new DataAccessCommand(sql, parameters);
    }
    
    public DataAccessCommand DeepCopy()
    {
        var copy = (DataAccessCommand)MemberwiseClone();
        copy.Parameters = Parameters.ConvertAll(p => p.DeepCopy());
        return copy;
    }
    
    public static explicit operator DataAccessCommand(FormattableString query)
    {
        return FromFormattableString(query);
    }
}