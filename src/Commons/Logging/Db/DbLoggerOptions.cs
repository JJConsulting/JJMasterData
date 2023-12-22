namespace JJMasterData.Commons.Logging.Db;

public class DbLoggerOptions
{
    public string TableName { get; set; } = "tb_log";
    
    public string ConnectionStringName { get; set; } = "ConnectionString";
    public string CreatedColumnName { get; set; } = "log_dat_evento";
    public string LevelColumnName { get; set; } = "log_txt_tipo";
    public string MessageColumnName { get; set; } = "log_txt_message";
    public string CategoryColumnName { get; set; } = "log_txt_source";
}
