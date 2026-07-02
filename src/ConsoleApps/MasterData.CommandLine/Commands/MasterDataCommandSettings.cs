using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public class MasterDataCommandSettings : CommandSettings
{
    [CommandOption("-p|--path <PATH>")]
    [Description("Path containing the JSON dictionaries to use.")]
    public string? DictionaryPath { get; init; }

    [CommandOption("-c|--connection <CONNECTION>")]
    [Description("Database connection string used by the command.")]
    public string? Connection { get; init; }

    [CommandOption("-t|--table <TABLE>")]
    [Description("Data dictionaries SQL table name. Schema included.")]
    public string? Table { get; init; }
    
    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(DictionaryPath))
            return ValidationResult.Error("The path is required.");

        if (string.IsNullOrWhiteSpace(Connection))
            return ValidationResult.Error("The connection string is required.");
        
        return ValidationResult.Success();
    }
}
