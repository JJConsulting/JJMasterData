using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class ExportCommand(ConsoleRunner consoleRunner) : AsyncCommand<ExportCommandSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        ExportCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await consoleRunner.ExportAsync(settings, cancellationToken);
        return 0;
    }
}
