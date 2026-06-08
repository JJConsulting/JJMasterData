#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class UploadAreaService(IHttpContextAccessor currentContext, IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public event EventHandler<FormUploadFileEventArgs>? OnFileUploaded;
    public event AsyncEventHandler<FormUploadFileEventArgs>? OnFileUploadedAsync;

    public async Task<UploadAreaResultDto> UploadFileAsync(IFormFile file, string? allowedTypes = null)
    {
        UploadAreaResultDto dto = new();

        await using var stream = file.OpenReadStream();
        var formFile = new FormFileContent
        {
            FileName = Path.GetFileName(file.FileName),
            Stream = stream,
            Length = file.Length,
            LastWriteTime = DateTime.Now
        };

        try
        {
            var message = string.Empty;

            ValidateAllowedExtensions(formFile.FileName, allowedTypes);

            var args = new FormUploadFileEventArgs(formFile);
            OnFileUploaded?.Invoke(this, args);

            if (OnFileUploadedAsync != null)
                await OnFileUploadedAsync.Invoke(this, args);

            if (formFile.FileName.Contains(","))
            {
                dto.ErrorMessage = stringLocalizer["The filename cannot contain comma."];
                return dto;
            }

            var errorMessage = args.ErrorMessage;
            if (args.SuccessMessage != null)
                message = args.SuccessMessage;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                dto.ErrorMessage = errorMessage;
                return dto;
            }

            dto.SuccessMessage = message;
        }
        catch (JJMasterDataException ex)
        {
            dto.ErrorMessage = ex.Message;
        }

        return dto;
    }

    public bool TryGetFile(string fileName, out IFormFile? formFile)
    {
        formFile = currentContext.HttpContext!.Request.Form.Files[fileName];
        return formFile != null;
    }

    private void ValidateAllowedExtensions(string filename, string? allowedTypes)
    {
        if (allowedTypes != null && !allowedTypes.Equals("*"))
            return;

        var list = new List<string>
        {
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
        };

        string ext = FileIO.GetFileNameExtension(filename);
        if (list.Contains(ext))
            throw new JJMasterDataException(stringLocalizer["You cannot upload system files"]);
    }
}
