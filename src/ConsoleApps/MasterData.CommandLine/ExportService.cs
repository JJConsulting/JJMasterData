using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Spectre.Console;

namespace JJMasterData.CommandLine;

public sealed class ExportService(
    IDataDictionaryRepository dataDictionaryRepository,
    IAnsiConsole console)
{
    public async Task ExportAsync(string dictionariesPath, CancellationToken cancellationToken)
    {
        var start = DateTime.Now;
        var fullPath = Path.GetFullPath(dictionariesPath);
        var localFiles = Directory.Exists(fullPath)
            ? await DictionaryFileService.LoadAsync(fullPath, cancellationToken)
            : [];
        var databaseElements = await dataDictionaryRepository.GetFormElementListAsync();
        var diff = DictionaryComparisonService.Compare(localFiles, databaseElements);

        console.MarkupLine("[grey]Starting export[/]");
        console.MarkupLine($"[grey]Path:[/] {Markup.Escape(fullPath)}");

        foreach (var item in diff.Changed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DictionaryFileService.WriteAsync(fullPath, item.Database, cancellationToken);
            console.MarkupLine($"[green]Exported[/] {Markup.Escape(item.Name)}");
        }

        foreach (var formElement in diff.DatabaseOnly)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DictionaryFileService.WriteAsync(fullPath, formElement, cancellationToken);
            console.MarkupLine($"[green]Exported[/] {Markup.Escape(formElement.Name)}");
        }

        console.MarkupLine(
            $"[grey]Summary:[/] exported {diff.Changed.Count + diff.DatabaseOnly.Count}, unchanged {diff.Unchanged.Count}, local-only {diff.FolderOnly.Count}");
        if (diff.FolderOnly.Count > 0)
            console.MarkupLine("[grey]Local-only files were left untouched.[/]");

        console.MarkupLine($"[grey]Started:[/] {start:O}");
        console.MarkupLine($"[grey]Finished:[/] {DateTime.Now:O}");
    }
}
