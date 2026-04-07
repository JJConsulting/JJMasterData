using System.Text.Encodings.Web;
using System.Text.Json;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.CommandLine;

public static class DictionaryFileService
{
    private static readonly JsonSerializerOptions ExportJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CanonicalJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<IReadOnlyList<LocalFormElementFile>> LoadAsync(
        string dictionariesPath,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(dictionariesPath);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Directory not found: {fullPath}");

        var files = Directory.EnumerateFiles(fullPath, "*.json")
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<LocalFormElementFile>(files.Count);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var jsonStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            var formElement = await JsonSerializer.DeserializeAsync<FormElement>(jsonStream, cancellationToken: cancellationToken);

            if (formElement == null)
                throw new InvalidDataException($"Invalid form element file: {file}");

            if (string.IsNullOrWhiteSpace(formElement.Name))
                throw new InvalidDataException($"Form element name is required: {file}");

            if (!names.Add(formElement.Name))
                throw new InvalidDataException($"Duplicate form element name found: {formElement.Name}");

            result.Add(new LocalFormElementFile(
                file,
                formElement,
                SerializeCanonical(formElement)));
        }

        return result;
    }

    public static async Task WriteAsync(string dictionariesPath, FormElement formElement, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(dictionariesPath);
        Directory.CreateDirectory(fullPath);

        var filePath = GetFilePath(fullPath, formElement.Name);
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, formElement, ExportJsonSerializerOptions, cancellationToken);
    }

    public static string SerializeCanonical(FormElement formElement)
    {
        return JsonSerializer.Serialize(formElement, CanonicalJsonSerializerOptions);
    }

    public static string GetFilePath(string dictionariesPath, string elementName)
    {
        if (string.IsNullOrWhiteSpace(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var fileName = elementName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? elementName
            : $"{elementName}.json";

        return Path.Combine(Path.GetFullPath(dictionariesPath), fileName);
    }
}

public sealed record LocalFormElementFile(string FilePath, FormElement FormElement, string CanonicalJson)
{
    public string Name => FormElement.Name;
}
