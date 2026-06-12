#nullable enable

using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Components;

public class UploadAreaManager(IHttpContextAccessor currentContext, FileValidationService fileValidationService)
{
    public event EventHandler<FormUploadFileEventArgs>? OnFileUploaded;
    public event AsyncEventHandler<FormUploadFileEventArgs>? OnFileUploadedAsync;

    public async Task<UploadAreaResultDto> UploadFileAsync(IFormFile file, string? allowedTypes = null)
    {
        UploadAreaResultDto dto = new();
        var message = string.Empty;
        var validation = fileValidationService.Validate(file, allowedTypes);
        if (!validation.IsSuccess)
        {
            dto.ErrorMessage = validation.ErrorMessage;
            return dto;
        }

        var args = new FormUploadFileEventArgs(file);
        OnFileUploaded?.Invoke(this, args);

        if (OnFileUploadedAsync != null)
            await OnFileUploadedAsync.Invoke(this, args);

        validation = fileValidationService.Validate(args.File, allowedTypes);
        if (!validation.IsSuccess)
        {
            dto.ErrorMessage = validation.ErrorMessage;
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

        return dto;
    }

    public bool TryGetFile(string fileName, out IFormFile? formFile)
    {
        formFile = currentContext.HttpContext!.Request.Form.Files[fileName];
        return formFile != null;
    }
}
