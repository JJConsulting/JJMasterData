# Architecture

The runtime loads a `FormElement` from `IDataDictionaryRepository`, builds a `JJFormView`, and reads or writes records through `IEntityRepository` and the configured `IEntityProvider`.

| Project / package suffix | Responsibility |
| --- | --- |
| `Storage.Abstractions` | `IFileStorage`: list, read, save, move and delete files. |
| `Commons` | `DataAccess`, entity providers/repositories, storage and background jobs. |
| `Core` | Dictionary models, expressions, validation, `FormService` and UI components. |
| `Web` | Razor views, MVC areas, static assets and route registration. |
| `WebApi` | Minimal API endpoints and dictionary-driven OpenAPI generation. |

Metadata storage and record storage are separate: selecting a filesystem dictionary repository does not move business records out of the database.

The host owns authentication and route policies. Add `Web` for the generated UI; add `WebApi` for HTTP access. For code that only needs database access, use Commons instead of depending on the UI layer.
