# File storage

The default provider is `DiskFileStorage`. To replace it, implement `IFileStorage` from `JJConsulting.MasterData.Storage.Abstractions` and register it through the builder:

```csharp
using JJMasterData.Commons.Storage;
using JJMasterData.Web.Configuration;

builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithFileStorage<MyFileStorage>();
```

`MyFileStorage` is your implementation of list, read, save, delete, folder delete and move operations. Storage is also used for generated import/export files.

Direct attachment actions check the existing record before accessing storage. Download, rename, and delete also require a filename listed in the record's file field. When `ApiOptions.ApplyUserIdOn` is configured, the signed-in user's ID must match the value stored in that field. The API also requires the dictionary option for the requested operation, including `EnableGetDetail` for downloads.

For a file field, set `Component = FormComponent.File` and configure `DataFile`:

| Property | Meaning |
| --- | --- |
| `FolderPath` | Destination path used by the file services. |
| `AllowedTypes` | Comma-separated extensions, e.g. `pdf,png,jpg`; default `*`. |
| `MaxFileSize` | Upload limit in MB. |
| `MultipleFile` | Allow multiple files; default `false`. |
| `ExportAsLink` | Export a download link with the file name. |
| `ViewGallery` | Display image previews as a gallery. |

Ensure the configured destination is writable by the application. For custom storage, preserve the path conventions passed to the provider so upload, download, rename and delete resolve the same files.
