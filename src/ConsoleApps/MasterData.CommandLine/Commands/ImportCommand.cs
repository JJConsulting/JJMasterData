using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class ImportCommand(ConsoleRunner consoleRunner) : AsyncCommand<ImportCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        ImportCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await consoleRunner.ImportAsync(settings, cancellationToken);
        return 0;
    }
}
