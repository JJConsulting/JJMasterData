using JJMasterData.CommandLine.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class InteractiveCommand(IAnsiConsole console, ConsoleRunner consoleRunner) : AsyncCommand
{
    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var action = console.Prompt(
            new SelectionPrompt<string>()
                .Title("Welcome to JJMasterData command line tool.")
                .AddChoices("Import", "Export", "Diff", "Exit"));

        if (string.Equals(action, "Exit", StringComparison.Ordinal))
            return 0;

        var path = console.Prompt(
            new TextPrompt<string>("Path")
                .Validate(value => string.IsNullOrWhiteSpace(value)
                    ? ValidationResult.Error("[red]Path is required.[/]")
                    : ValidationResult.Success()));

        var connection = console.Prompt(
            new TextPrompt<string>("Connection")
                .Validate(value => string.IsNullOrWhiteSpace(value)
                    ? ValidationResult.Error("[red]Connection is required.[/]")
                    : ValidationResult.Success()));

        return action switch
        {
            "Import" => await ExecuteAsync(
                consoleRunner.ImportAsync(path, connection, cancellationToken)),
            "Export" => await ExecuteAsync(
                consoleRunner.ExportAsync(path, connection, cancellationToken)),
            "Diff" => await ExecuteAsync(
                consoleRunner.DiffAsync(path, connection, cancellationToken)),
            _ => 1
        };
    }

    private static async Task<int> ExecuteAsync(Task command)
    {
        await command;
        return 0;
    }
}
