#nullable enable

using System.Collections.Frozen;
using System.IO;
using System.Linq;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
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

    public void Validate(IFormFile file, string? allowedTypes = null)
    {
        var fileName = Path.GetFileName(file.FileName);
        ValidateFileName(fileName);
        ValidateAllowedExtensions(fileName, allowedTypes);
    }

    public void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new JJMasterDataException(stringLocalizer["Required file name"]);

        if (Path.GetFileName(fileName).Contains(','))
            throw new JJMasterDataException(stringLocalizer["The filename cannot contain comma."]);
    }

    public void ValidateAllowedExtensions(string fileName, string? allowedTypes)
    {
        var extension = FileIO.GetFileNameExtension(fileName);
        if (BlockedExtensions.Contains(extension))
            throw new JJMasterDataException(stringLocalizer["You cannot upload system files"]);

        if (string.IsNullOrWhiteSpace(allowedTypes) || allowedTypes.Equals("*"))
            return;

        var allowedExtensions = allowedTypes
            .Split(',')
            .Select(type => type.Trim())
            .Where(type => type.Length > 0)
            .Select(type => type.StartsWith('.') ? type : "." + type)
            .ToHashSet();

        if (allowedExtensions.Count > 0 && !allowedExtensions.Contains(extension))
            throw new JJMasterDataException(
                stringLocalizer["File type is not allowed. Allowed extensions: {0}", allowedTypes]);
    }
}
