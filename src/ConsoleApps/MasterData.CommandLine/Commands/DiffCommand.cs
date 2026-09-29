using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class DiffCommand(ConsoleRunner consoleRunner) : AsyncCommand<DiffCommandSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        DiffCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await consoleRunner.DiffAsync(settings, cancellationToken);
        return 0;
    }
}
