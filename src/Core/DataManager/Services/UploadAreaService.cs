#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Events.Args;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class UploadAreaService(IHttpContext currentContext, IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public event EventHandler<FormUploadFileEventArgs>? OnFileUploaded;
    public event AsyncEventHandler<FormUploadFileEventArgs>? OnFileUploadedAsync;

    public async Task<UploadAreaResultDto> UploadFileAsync(FormFileContent formFile, string? allowedTypes = null)
    {
        UploadAreaResultDto dto = new();

        try
        {
            var message = string.Empty;

            ValidateAllowedExtensions(formFile.FileName, allowedTypes);

            var args = new FormUploadFileEventArgs(formFile);
            OnFileUploaded?.Invoke(this, args);

            if (OnFileUploadedAsync != null)
            {
                await OnFileUploadedAsync.Invoke(this, args);
            }

            var errorMessage = args.ErrorMessage;
            if (args.SuccessMessage != null)
            {
                message = args.SuccessMessage;
            }

            if (!string.IsNullOrEmpty(errorMessage))
                throw new JJMasterDataException(errorMessage);

            dto.SuccessMessage = message;
        }
        catch (JJMasterDataException ex)
        {
            dto.ErrorMessage = ex.Message;
        }

        return dto;
    }

    /// <summary>
    /// Recovers the file after the POST
    /// </summary>
    private FormFileContent? GetFile(string fileName)
    {
        var fileData = currentContext.Request.Form.GetFile(fileName);

        if (fileData is null)
            return null;

        using var stream = new MemoryStream();
        string filename = fileData.FileName;

#if NETFRAMEWORK
        fileData.InputStream.CopyTo(stream);
#else
        fileData.CopyTo(stream);
#endif

        var content = new FormFileContent
        {
            FileName = filename,
            Bytes = stream.ToArray(),
            Length = stream.Length,
            LastWriteTime = DateTime.Now
        };

        return content;
    }

    public bool TryGetFile(string fileName, out FormFileContent? formFile)
    {
        formFile = GetFile(fileName);

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