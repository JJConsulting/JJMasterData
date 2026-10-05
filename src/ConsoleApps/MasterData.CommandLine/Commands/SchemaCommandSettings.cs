using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class SchemaCommandSettings : CommandSettings
{
    [CommandOption("-o|--output <FILE>")]
    [Description("Path to the generated FormElement JSON Schema file.")]
    public string? Output { get; init; }

    public override ValidationResult Validate() => string.IsNullOrWhiteSpace(Output)
        ? ValidationResult.Error("The output file is required.")
        : ValidationResult.Success();
}
