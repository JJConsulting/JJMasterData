﻿#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
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

    public DataAccessCommand DeepCopy()
    {
        var copy = (DataAccessCommand)MemberwiseClone();
        copy.Parameters = Parameters.ConvertAll(p => p.DeepCopy());
        return copy;
    }
}