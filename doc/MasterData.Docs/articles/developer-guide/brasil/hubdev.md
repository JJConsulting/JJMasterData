# HubDev: CPF, CNPJ, and CEP

`HubDevService` looks up CPF, CNPJ, and CEP through the HubDev API. It is the default CPF/CNPJ provider for `WithBrasilActionPlugins()` and takes precedence in the overload that accepts `IConfiguration`. Automatic registration uses ViaCEP for the CEP action; register HubDev explicitly if you want it to handle CEP too.

## Installation and configuration

Install `JJMasterData.Brasil` as described in [Brazil integrations](index.md#installation). Obtain a HubDev API key and configure it in the host:

```bash
dotnet user-secrets set "JJMasterData:HubDev:ApiKey" "<your-key>"
```

The equivalent `appsettings.json` structure is shown below. Supply the real key through your secret store rather than committing it to source control.

```json
{
  "JJMasterData": {
    "HubDev": {
      "ApiKey": "<your-key>",
      "Url": "ws.hubdodesenvolvedor.com.br/v2/"
    }
  }
}
```

`Url` is optional and defaults to the value above. Provide the host and path without `https://`; the service adds the protocol. To use HubDev for CPF/CNPJ and ViaCEP for CEP, register the default actions:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithBrasilActionPlugins(builder.Configuration);
```

To use HubDev for all three field actions, register them explicitly:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithHubDevCpfActionPlugin()
    .WithHubDevCnpjActionPlugin()
    .WithHubDevCepActionPlugin();
```

Add `using JJMasterData.Brasil.Configuration;` and `using JJMasterData.Web.Configuration;` to these examples.

## Using the actions

In the dictionary editor, add a plugin action to a CPF or CNPJ field and map result keys such as `NomeDaPf` or `Nome` to form fields. For CPF, `BirthDate` must point to a populated birth date field. CPF requests send `data` in `dd/MM/yyyy` format; CNPJ requests send the document number. The CEP action can map `Logradouro`, `Bairro`, `Localidade`, and `Uf`. See [field action setup](index.md#using-field-actions-in-a-dictionary).

CPF and CNPJ actions expose `IgnoreDb`. When enabled, the service adds `ignore_db=1` to the request. The plugin's field description says this mode uses three credits instead of one; check current pricing with the provider. `TimeoutSeconds` limits the request and defaults to five seconds; `0` also means five seconds. `ShowErrorMessage` displays an error when the lookup fails.

For direct use through dependency injection, `WithHubDev()` registers `IReceitaFederalService` as `HubDevService`. It provides `SearchCpfAsync(cpf, birthDate)`, `SearchCnpjAsync(cnpj)`, and `SearchCepAsync(cep)`. If you register only the service, also register `HttpClient`. The explicit three-action example and automatic registration both provide it through the CEP action. Avoid registering multiple `IReceitaFederalService` providers in the same host.
