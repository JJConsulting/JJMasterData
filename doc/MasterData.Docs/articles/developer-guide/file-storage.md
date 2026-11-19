# File storage

The default provider is `DiskFileStorage`. To replace it, implement `IFileStorage` from `JJConsulting.MasterData.Storage.Abstractions` and register it through the builder:

```csharp
using JJMasterData.Commons.Storage;
using JJMasterData.Web.Configuration;

builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithFileStorage<MyFileStorage>();
```

`MyFileStorage` is your implementation of list, read, save, delete, folder delete and move operations. Storage is also used for generated import/export files.

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

A record key occupies one folder segment under the configured destination. Portable names keep their existing folders. Empty keys and keys containing path syntax or other nonportable names use a stable encoded folder name. Move existing attachments stored using nonportable names to their new resolved folder before relying on the new mapping. The disk provider refuses links at the target file or record folder. Keep the configured roots and their parent directories under administrative control.
