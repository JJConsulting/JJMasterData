using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Dao;

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
        Sql = sql;
        Parameters = parameters;
        CmdType = CommandType.Text;
    }

    public DataAccessCommand(string sql, List<DataAccessParameter> parameters, CommandType type)
    {
        Sql = sql;
        Parameters = parameters;
        CmdType = type;
    }
}