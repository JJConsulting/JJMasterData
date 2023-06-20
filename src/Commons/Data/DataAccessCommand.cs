using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data;

public class DataAccessCommand
{
    [JsonProperty("cmdType")]
    public CommandType CmdType { get; set; }

    [JsonProperty("sql")]
    public string Sql { get; set; }

    [JsonProperty("parameters")]
    public List<DataAccessParameter> Parameters { get; set; }

    public DataAccessCommand()
    {
        CmdType = CommandType.Text;
        Parameters = new List<DataAccessParameter>();
    }

    public DataAccessCommand(string sql)
    {
        Sql = sql;
        Parameters = new List<DataAccessParameter>();
        CmdType = CommandType.Text;
    }

    public DataAccessCommand(string sql, List<DataAccessParameter> parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        
        Sql = sql;
        Parameters = parameters;
        CmdType = CommandType.Text;
    }

    public DataAccessCommand(string sql, List<DataAccessParameter> parameters, CommandType type) : this(sql,parameters)
    {
        CmdType = type;
    }

    public void AddParameter(string name, object? value, DbType dbType)
    {
        Parameters ??= new List<DataAccessParameter>();
        Parameters.Add(new DataAccessParameter(name, value, dbType));
    }
}