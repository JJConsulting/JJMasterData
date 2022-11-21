namespace JJMasterData.Commons.Logging;

public record LoggerTableOptions
{
    public string Name { get; set; } = "tb_log";

    public string ContentColumnName { get; set; } = "log_txt_message";


    public string DateColumnName { get; set; } = "log_dat_evento";

    public string LevelColumnName { get; set; } = "log_txt_tipo";

    public string SourceColumnName { get; set; } = "log_txt_source";
}
