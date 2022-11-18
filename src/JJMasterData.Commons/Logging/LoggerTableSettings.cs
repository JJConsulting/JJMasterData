using System.IO;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Settings;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace JJMasterData.Commons.Logging;

public class LoggerTableSettings
{
    private string _name;
    private string _contentColumnName;
    private string _dateColumnName;
    private string _levelColumnName;
    private string _sourceColumnName;
    private const string SectionName = "Table";

    public string Name
    {
        get => _name ??= GetValue(nameof(Name));
        set => _name = value;
    }

    public string ContentColumnName
    {
        get => _contentColumnName ??= GetValue(nameof(ContentColumnName));
        set => _contentColumnName = value;
    }

    public string DateColumnName
    {
        get => _dateColumnName ??=GetValue(nameof(DateColumnName));
        set => _dateColumnName = value;
    }

    public string LevelColumnName
    {
        get => _levelColumnName ??= GetValue(nameof(LevelColumnName));
        set => _levelColumnName = value;
    }

    public string SourceColumnName
    {
        get => _sourceColumnName ??=  GetValue(nameof(SourceColumnName));
        set => _sourceColumnName = value;
    }
    private string GetValue(string key)
    {
        IConfiguration configuration;
        try
        {
            configuration = JJMasterDataOptions.Configuration;
        }
        catch (FileLoadException)
        {
            //IConfiguration not supported on .NET Framework
            configuration = null;
        }
        

        var tableSettings = configuration?.GetJJMasterDataLogger().GetSection(SectionName);

        string tableName;
        if (JJMasterDataOptions.IsNetFramework)
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
