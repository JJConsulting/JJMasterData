using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class DiffCommand(ConsoleRunner consoleRunner) : AsyncCommand<DiffCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        DiffCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await consoleRunner.DiffAsync(settings.DictionaryPath!, settings.Connection!, cancellationToken);
        return 0;
    }
}
