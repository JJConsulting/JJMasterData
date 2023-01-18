using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data;

[Serializable]
[DataContract]
public class DataAccessCommand
{
    [DataMember(Name = "cmdType")]
    public CommandType CmdType { get; set; }

    [DataMember(Name = "sql")]
    public string Sql { get; set; }

    [DataMember(Name = "parameters")]
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
}