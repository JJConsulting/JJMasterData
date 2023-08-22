#nullable enable

using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data;

public class DataAccessCommand
{
    [JsonProperty("cmdType")]
    public CommandType CmdType { get; set; }

    [JsonProperty("sql")]
    public string? Sql { get; set; }

    [JsonProperty("parameters")]
    public List<DataAccessParameter> Parameters { get; set; }

    [SetsRequiredMembers]
    public DataAccessCommand()
    {
        Sql = string.Empty;
        CmdType = CommandType.Text;
        Parameters = new List<DataAccessParameter>();
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
        CmdType = CommandType.Text;
    }

    public DataAccessCommand(string sql, List<DataAccessParameter> parameters, CommandType type) : this(sql,parameters)
    {
        CmdType = type;
    }

    public void AddParameter(string name, object? value, DbType dbType)
    {
        Parameters.Add(new DataAccessParameter(name, value, dbType));
    }
}