# ViaCEP: CEP lookup

`ViaCepService` looks up Brazilian postal codes (CEPs) without an API key. Automatic registration through `WithBrasilActionPlugins(builder.Configuration)` adds the `Cep` action with ViaCEP regardless of the CPF/CNPJ provider.

## Installation and registration

Install `JJMasterData.Brasil` as described in [Brazil integrations](index.md#installation). To register only the CEP field action:

```csharp
using JJMasterData.Brasil.Configuration;
using JJMasterData.Brasil.Services;
using JJMasterData.Web.Configuration;

builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithCepActionPlugin<ViaCepService>();
```

This method registers `HttpClient`, `ICepService`, and the action handler. To use only the service through dependency injection, register `HttpClient` and call `WithViaCep()`.

## Usage

Add a `Cep` plugin action to the CEP field in the dictionary. Map `Logradouro`, `Complemento`, `Bairro`, `Localidade`, `Uf`, or `Unidade` to destination fields. The service removes punctuation from the CEP and requests `https://viacep.com.br/ws/{cep}/json`. An unknown CEP raises `ViaCepException`; the action clears mapped fields and can show an error with `ShowErrorMessage` or enable editing with `AllowEditingOnError`. See the [shared action options](index.md#using-field-actions-in-a-dictionary).

`TimeoutSeconds` appears in the shared plugin configuration but is not applied to HTTP requests by `ViaCepService`.
