using JJMasterData.CommandLine.Hosting;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Commands;

public sealed class DiffCommand(ImportRunner importRunner) : AsyncCommand<DiffCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        DiffCommandSettings settings,
        CancellationToken cancellationToken)
    {
        await importRunner.DiffAsync(settings.DictionaryPath!, settings.Connection!, cancellationToken);
        return 0;
    }
}
