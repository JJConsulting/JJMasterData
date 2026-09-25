# Configuration

Use `AddJJMasterDataWeb(builder.Configuration)` from `JJMasterData.Web.Configuration`. It registers MVC views, localization and the Core/Commons services; it does not configure authentication.

The options bind to the `JJMasterData` section. Override host-specific UI settings after registration:

```csharp
using JJMasterData.Web.Configuration.Options;

builder.Services.PostConfigure<MasterDataWebOptions>(options =>
{
    options.LayoutPath = "_Layout";
    options.ModalLayoutPath = "_ModalLayout";
    options.CustomStylesheetsPaths.Add("~/css/masterdata-overrides.css");
});
```

Include `_MasterDataStylesheets` and `_MasterDataScripts` in your custom layout, and serve static files so `/_content/JJMasterData.Web/` assets resolve.

`UseMasterDataSeedingAsync()` initializes both the metadata repository and audit storage. It requires database DDL permissions when these structures are absent. Map `MapDataDictionary()` for administration and `MapMasterData()` for the runtime UI, then attach the host's authorization policies.

See the [complete startup example](../getting-started/configuration.md).

## Configuration reference

Unless noted otherwise, these properties bind under `JJMasterData`.

| Option | Default / purpose |
| --- | --- |
| `ConnectionString` | Default database connection; supply it in host configuration. |
| `ConnectionProvider` | Default enum value selects SQL Server. |
| `AdditionalConnectionStrings` | Empty list; entries use `Guid`, `Name`, `Connection`, `ConnectionProvider`. |
| `ReadProcedurePattern` | `{tablename}Get`. |
| `WriteProcedurePattern` | `{tablename}Set`. |
| `DataDictionaryTableName` | `tb_masterdata`. |
| `AuditLogTableName` | `tb_masterdata_auditlog`. |
| `EnableDataDictionaryCaching` | `true`. |
| `ExportationFolderPath` | `{app.path}/JJExportationFiles`. |
| `UserIdClaimType` | `ClaimTypes.NameIdentifier`. |
| `LayoutPath` | `_MasterDataLayout`. |
| `ModalLayoutPath` | `_MasterDataLayout.Modal`. |
| `CustomBootstrapPath` | `null`; uses the bundled Bootstrap stylesheet. |

`DataDictionaryTableSchema` and `AuditLogTableSchema` select schemas for the internal tables. Per-element read/write procedure names override the naming patterns.

Configure filesystem dictionaries under `JJMasterData:DataDictionary:FolderPath` when using the parameterless `WithFileSystemDataDictionary()`. Background options live under `JJMasterData:BackgroundJobs`: `Capacity = int.MaxValue`, `MaxConcurrency = 1000`, `CompletedJobRetention = 01:00:00`.

Use `PostConfigure<MasterDataWebOptions>` for custom script/style lists, and `PostConfigure<MasterDataCoreOptions>` for `ExpressionContext` functions.
