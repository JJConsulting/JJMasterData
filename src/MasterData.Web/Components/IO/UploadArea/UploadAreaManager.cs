using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Events.Args;

namespace JJMasterData.Web.Components;

public class UploadAreaManager(IHttpContextAccessor httpContextAccessor, FileValidationService fileValidationService)
{
    public event EventHandler<FormUploadFileEventArgs>? OnFileUploaded;
    public event AsyncEventHandler<FormUploadFileEventArgs>? OnFileUploadedAsync;

    public async Task<UploadAreaResultDto> UploadFileAsync(IFormFile file, string? allowedTypes = null)
    {
        UploadAreaResultDto dto = new();

        try
        {
            var message = string.Empty;

            fileValidationService.Validate(file.FileName, allowedTypes);

            var args = new FormUploadFileEventArgs(file);
            OnFileUploaded?.Invoke(this, args);

            if (OnFileUploadedAsync != null)
                await OnFileUploadedAsync.Invoke(this, args);

            fileValidationService.Validate(args.File.FileName, allowedTypes);

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
        if (httpContextAccessor.HttpContext?.Request.HasFormContentType is false)
        {
            formFile = null;
            return false;
        }
        
        formFile = httpContextAccessor.HttpContext?.Request.Form.Files[fileName];
        return formFile != null;
    }
}
