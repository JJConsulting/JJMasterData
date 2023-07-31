#nullable enable

using System;
using JetBrains.Annotations;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Services;

public interface IUploadAreaService
{
    event EventHandler<FormUploadFileEventArgs> OnFileUploaded;
    UploadAreaResultDto UploadFile(string fileName = "file",string? allowedTypes = null);
}