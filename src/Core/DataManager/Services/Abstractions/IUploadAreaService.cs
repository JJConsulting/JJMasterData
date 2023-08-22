using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Services;

public interface IUploadAreaService
{
    event EventHandler<FormUploadFileEventArgs> OnFileUploaded;
    event AsyncEventHandler<FormUploadFileEventArgs> OnFileUploadedAsync;
    Task<UploadAreaResultDto> UploadFileAsync(string fileName = "file", string? allowedTypes = null);
}