#nullable enable

using System.Collections.Frozen;
using System.IO;
using System.Linq;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Util;
using JJMasterData.Commons.Validations;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class FileValidationService(IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private static readonly FrozenSet<string> BlockedExtensions =
    [
        ".ade",
        ".adp",
        ".apk",
        ".appx",
        ".appxbundle",
        ".bat",
        ".cab",
        ".chm",
        ".cmd",
        ".com",
        ".cpl",
        ".dll",
        ".dmg",
        ".ex",
        ".ex_",
        ".exe",
        ".hta",
        ".ins",
        ".isp",
        ".iso",
        ".js",
        ".jse",
        ".lib",
        ".lnk",
        ".mde",
        ".msc",
        ".msi",
        ".msix",
        ".msixbundle",
        ".msp",
        ".mst",
        ".nsh",
        ".pif",
        ".ps1",
        ".scr",
        ".sct",
        ".sh",
        ".shb",
        ".sys",
        ".vb",
        ".vbe",
        ".vbs",
        ".vxd",
        ".wsc",
        ".wsf",
        ".wsh",
        ".jar",
        ".cs",
        ".cshtml",
        ".bin"
    ];

    public ValidationResult Validate(IFormFile file, string? allowedTypes = null)
    {
        var fileName = Path.GetFileName(file.FileName);
        var fileNameValidation = ValidateFileName(fileName);
        if (!fileNameValidation.IsSuccess)
            return fileNameValidation;

        return ValidateAllowedExtensions(fileName, allowedTypes);
    }

    public ValidationResult ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return ValidationResult.Error(stringLocalizer["Required file name"]);

        if (Path.GetFileName(fileName).Contains(','))
            return ValidationResult.Error(stringLocalizer["The filename cannot contain comma."]);

        return ValidationResult.Success;
    }

    public ValidationResult ValidateAllowedExtensions(string fileName, string? allowedTypes)
    {
        var extension = FileIO.GetFileNameExtension(fileName);
        if (BlockedExtensions.Contains(extension))
            return ValidationResult.Error(stringLocalizer["You cannot upload system files"]);

        if (string.IsNullOrWhiteSpace(allowedTypes) || allowedTypes.Equals("*"))
            return ValidationResult.Success;

        var allowedExtensions = allowedTypes
            .Split(',')
            .Select(type => type.Trim())
            .Where(type => type.Length > 0)
            .Select(type => type.StartsWith('.') ? type : "." + type)
            .ToHashSet();

        if (allowedExtensions.Count > 0 && !allowedExtensions.Contains(extension))
            return ValidationResult.Error(
                stringLocalizer["File type is not allowed. Allowed extensions: {0}", allowedTypes]);

        return ValidationResult.Success;
    }
}
