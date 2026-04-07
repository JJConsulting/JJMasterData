using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class ExportCommand(ConsoleRunner consoleRunner) : AsyncCommand<ExportCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        ExportCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await consoleRunner.ExportAsync(settings.DictionaryPath!, settings.Connection!, cancellationToken);
        return 0;
    }
}
