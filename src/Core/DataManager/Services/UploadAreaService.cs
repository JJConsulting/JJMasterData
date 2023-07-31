#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class UploadAreaService : IUploadAreaService
{
    private IHttpContext CurrentContext { get; }
    public event EventHandler<FormUploadFileEventArgs>? OnFileUploaded;

    public UploadAreaService(IHttpContext currentContext)
    {
        CurrentContext = currentContext;
    }
    
    public UploadAreaResultDto UploadFile(string fileName = "file", string? allowedTypes = null)
    {
        UploadAreaResultDto dto = new();
        
        try
        {
            string message = string.Empty;
            
            var file = GetFile(fileName);
            
            ValidateAllowedExtensions(file.FileName, allowedTypes);
            
            var args = new FormUploadFileEventArgs(file);
            OnFileUploaded?.Invoke(this, args);
            var errorMessage = args.ErrorMessage;
            if (args.SuccessMessage != null)
            {
                message = args.SuccessMessage;
            }

            if (!string.IsNullOrEmpty(errorMessage))
                throw new JJMasterDataException(errorMessage);
            
            dto.Message = message;

        }
        catch (Exception ex)
        {
            dto.Error = ex.Message;
        }

        return dto;
    }
    
    /// <summary>
    /// Recovers the file after the POST
    /// </summary>
    private FormFileContent GetFile(string fileName)
    {
        var fileData = CurrentContext.Request.GetFile(fileName);
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

    private static void ValidateAllowedExtensions(string filename, string? allowedTypes)
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
            throw new JJMasterDataException("You cannot upload this file extension.");

    }
}