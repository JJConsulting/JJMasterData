using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Spectre.Console;

namespace JJMasterData.CommandLine;

public sealed class ImportService(
    IDataDictionaryRepository dataDictionaryRepository,
    IAnsiConsole console,
    DiffService diffService)
{
    public async Task ImportAsync(string dictionariesPath, CancellationToken cancellationToken)
    {
        var start = DateTime.Now;
        var fullPath = Path.GetFullPath(dictionariesPath);
        var diff = await diffService.CompareAsync(dictionariesPath, cancellationToken);

        console.MarkupLine("[grey]Starting import[/]");
        console.MarkupLine($"[grey]Path:[/] {Markup.Escape(fullPath)}");

        foreach (var file in diff.FolderOnly)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await dataDictionaryRepository.InsertOrReplaceAsync(file.FormElement);
            console.MarkupLine($"[green]Saved[/] {Markup.Escape(file.Name)}");
        }

        foreach (var item in diff.Changed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await dataDictionaryRepository.InsertOrReplaceAsync(item.Local.FormElement);
            console.MarkupLine($"[green]Updated[/] {Markup.Escape(item.Name)}");
        }

        foreach (var formElement in diff.DatabaseOnly)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await dataDictionaryRepository.DeleteAsync(formElement.Name);
            console.MarkupLine($"[yellow]Deleted[/] {Markup.Escape(formElement.Name)}");
        }

        console.MarkupLine(
            $"[grey]Summary:[/] created {diff.FolderOnly.Count}, updated {diff.Changed.Count}, deleted {diff.DatabaseOnly.Count}, unchanged {diff.Unchanged.Count}");
        console.MarkupLine($"[grey]Started:[/] {start:O}");
        console.MarkupLine($"[grey]Finished:[/] {DateTime.Now:O}");
    }
}
