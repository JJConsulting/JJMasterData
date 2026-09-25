# WebApi and OpenAPI

Install `JJMasterData.WebApi`, then register its services and dictionary-aware OpenAPI transformer:

```csharp
using JJMasterData.WebApi.Configuration;
using JJMasterData.WebApi.Endpoints;
using JJMasterData.WebApi.OpenApi;

builder.Services.AddJJMasterDataWebApi();
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<DataDictionaryDocumentTransformer>());
```

After building the host, map `app.MapMasterDataApi()` for records and optionally `app.MapDictionaries()` for metadata. Attach the host's authorization policy to each. `app.MapOpenApi()` exposes the generated document. Database configuration and initialization are still required.

The record routes are relative to `/api/masterdata/{elementName}`:

| Method / path | Request |
| --- | --- |
| `GET /` | `pag` (default 1), `regporpag` (default 5), `orderby`, `tot`. |
| `GET /{id}` | Read one record. |
| `POST /` | JSON array of field/value objects; optional `replace=true`. |
| `PUT /` | JSON array for full updates, including keys. |
| `PATCH /` | JSON array for partial updates, including keys. |
| `DELETE /{id}` | Delete one record. |

Enable the required methods in the dictionary's `ApiOptions`. Batch writes can return `207 Multi-Status`; inspect each response item.

The generic routes are excluded from normal endpoint descriptions; the transformer adds paths for configured dictionaries. In the current implementation it describes PUT/PATCH under `/{id}`, while the actual handlers accept arrays at the collection route shown above. Use the registered routes when implementing clients.
