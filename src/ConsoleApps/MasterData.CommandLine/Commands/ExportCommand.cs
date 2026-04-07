using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class ExportCommand(ImportRunner importRunner) : AsyncCommand<ExportCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        ExportCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await importRunner.ExportAsync(settings.DictionaryPath!, settings.Connection!, cancellationToken);
        return 0;
    }
}
