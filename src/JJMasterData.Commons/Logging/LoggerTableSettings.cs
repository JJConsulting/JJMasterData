using System.IO;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Settings;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace JJMasterData.Commons.Logging;

public class LoggerTableSettings
{
    private const string SectionName = "Table";

    public string Name { get; set; }
    public string ContentColumnName { get; set; }
    public string DateColumnName { get; set; }
    public string LevelColumnName { get; set; }
    public string SourceColumnName { get; set; }


    public LoggerTableSettings()
    {
        Name = GetValue(nameof(Name));
        DateColumnName =GetValue(nameof(DateColumnName)); 
        LevelColumnName = GetValue(nameof(LevelColumnName)); 
        SourceColumnName = GetValue(nameof(SourceColumnName)); 
        ContentColumnName = GetValue(nameof(ContentColumnName)); 
    }

    private string GetValue(string key)
    {
        IConfiguration configuration;
        try
        {
            configuration = JJMasterDataSettings.Configuration;
        }
        catch (FileLoadException)
        {
            //IConfiguration not supported on .NET Framework
            configuration = null;
        }
        

        var tableSettings = configuration?.GetJJMasterDataLogger().GetSection(SectionName);

        string tableName;
        if (JJMasterDataSettings.IsNetFramework)
            tableName = ConfigurationManager.AppSettings["LOG_TABLENAME"];
        else
            tableName = tableSettings[nameof(Name)];

        //The default values are from legacy .NET Framework JJConsulting systems,
        //from a time when there was no customization in column names.
        //We kindly ask to not change the values, please customize them in appsettings.json.
        return key switch
        {
            nameof(Name) => tableName ?? "tb_log",
            nameof(ContentColumnName) => tableSettings?[key] ?? "log_txt_message",
            nameof(DateColumnName) => tableSettings?[key] ?? "log_dat_evento",
            nameof(LevelColumnName) => tableSettings?[key] ?? "log_txt_tipo",
            nameof(SourceColumnName) => tableSettings?[key] ?? "log_txt_source",
            _ => null,
        };
    }
}
