using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.CommandLine;

public static class DictionaryComparisonService
{
    public static DictionaryDiffResult Compare(
        IEnumerable<LocalFormElementFile> localFiles,
        IEnumerable<FormElement> databaseElements)
    {
        var folderOnly = new List<LocalFormElementFile>();
        var changed = new List<ChangedDictionary>();
        var unchanged = new List<string>();

        var databaseByName = databaseElements
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var localFile in localFiles.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!databaseByName.Remove(localFile.Name, out var databaseElement))
            {
                folderOnly.Add(localFile);
                continue;
            }

            var databaseCanonicalJson = DictionaryFileService.SerializeCanonical(databaseElement);
            if (string.Equals(localFile.CanonicalJson, databaseCanonicalJson, StringComparison.Ordinal))
            {
                unchanged.Add(localFile.Name);
                continue;
            }

            changed.Add(new ChangedDictionary(localFile.Name, localFile, databaseElement, databaseCanonicalJson));
        }

        var databaseOnly = databaseByName.Values
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        unchanged.Sort(StringComparer.OrdinalIgnoreCase);
        return new DictionaryDiffResult(folderOnly, changed, databaseOnly, unchanged);
    }
}

public sealed class DictionaryDiffResult(
    List<LocalFormElementFile> folderOnly,
    List<ChangedDictionary> changed,
    List<FormElement> databaseOnly,
    List<string> unchanged)
{
    public List<LocalFormElementFile> FolderOnly { get; } = folderOnly;
    public List<ChangedDictionary> Changed { get; } = changed;
    public List<FormElement> DatabaseOnly { get; } = databaseOnly;
    public List<string> Unchanged { get; } = unchanged;

    public bool HasChanges => FolderOnly.Count > 0 || Changed.Count > 0 || DatabaseOnly.Count > 0;
}

public sealed record ChangedDictionary(
    string Name,
    LocalFormElementFile Local,
    FormElement Database,
    string DatabaseCanonicalJson);
