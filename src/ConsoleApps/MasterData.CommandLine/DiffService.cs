using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Spectre.Console;

namespace JJMasterData.CommandLine;

public sealed class DiffService(
    IDataDictionaryRepository dataDictionaryRepository,
    IAnsiConsole console)
{
    public async Task<DictionaryDiffResult> DiffAsync(string dictionariesPath, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(dictionariesPath);
        var result = await CompareAsync(dictionariesPath, cancellationToken);

        console.MarkupLine("[grey]Starting diff[/]");
        console.MarkupLine($"[grey]Path:[/] {Markup.Escape(fullPath)}");

        var summaryTable = new Table()
            .AddColumn("Create")
            .AddColumn("Update")
            .AddColumn("Delete")
            .AddColumn("Unchanged");

        summaryTable.AddRow(
            result.FolderOnly.Count.ToString(),
            result.Changed.Count.ToString(),
            result.DatabaseOnly.Count.ToString(),
            result.Unchanged.Count.ToString());

        console.Write(summaryTable);

        if (!result.HasChanges)
        {
            console.MarkupLine("[green]No differences found.[/]");
            return result;
        }

        var detailsTable = new Table()
            .AddColumn("Change")
            .AddColumn("Name")
            .AddColumn("Path");

        foreach (var item in result.FolderOnly)
            detailsTable.AddRow("[green]Create[/]", Markup.Escape(item.Name), Markup.Escape(item.FilePath));

        foreach (var item in result.Changed)
            detailsTable.AddRow("[yellow]Update[/]", Markup.Escape(item.Name), Markup.Escape(item.Local.FilePath));

        foreach (var item in result.DatabaseOnly)
            detailsTable.AddRow("[red]Delete[/]", Markup.Escape(item.Name), "-");

        console.Write(detailsTable);
        return result;
    }

    public async Task<DictionaryDiffResult> CompareAsync(string dictionariesPath, CancellationToken cancellationToken)
    {
        var localFiles = await DictionaryFileService.LoadAsync(dictionariesPath, cancellationToken);
        var databaseElements = await dataDictionaryRepository.GetFormElementListAsync();
        return DictionaryComparisonService.Compare(localFiles, databaseElements);
    }
}
