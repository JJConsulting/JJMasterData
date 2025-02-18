using System;
using JetBrains.Annotations;

namespace JJMasterData.Commons.Logging.Db;

public sealed class DbLoggerOptions : BatchingLoggerOptions
{
    public Guid? ConnectionStringId { get; set; } 
    
    [CanBeNull]
    public string TableSchema { get; set; }
    
    public string TableName { get; set; } = "tb_masterdata_log";
    
    public string IdColumnName { get; set; } = "Id";
    
    public string CreatedColumnName { get; set; } = "log_dat_evento";
    public string LevelColumnName { get; set; } = "log_txt_tipo";
    public string MessageColumnName { get; set; } = "log_txt_message";
    public string CategoryColumnName { get; set; } = "log_txt_source";
}
