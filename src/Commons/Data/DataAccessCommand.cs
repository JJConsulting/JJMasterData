#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data;

[DebuggerDisplay("Sql = {Sql}, Type = {Type}")]
public class DataAccessCommand
{
    [Obsolete("Please use Type property")]
    [JsonIgnore]
    public CommandType CmdType
    {
        get => Type;
        set => Type = value;
    }
    
    [JsonProperty("cmdType")]
    public CommandType Type { get; set; }

    [JsonProperty("sql")]
    [Display(Name = "Sql Command")]
    public string Sql { get; set; }

    [JsonProperty("parameters")]
    public List<DataAccessParameter> Parameters { get; internal init; }

    [SetsRequiredMembers]
    public DataAccessCommand()
    {
        Sql = string.Empty;
        Type = CommandType.Text;
        Parameters = [];
    }

    [SetsRequiredMembers]
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

    public DataAccessCommand DeepCopy()
    {
        return new DataAccessCommand()
        {
            Type = Type,
            Parameters = Parameters.Select(p => p.DeepCopy()).ToList(),
            Sql = Sql
        };
    }
}