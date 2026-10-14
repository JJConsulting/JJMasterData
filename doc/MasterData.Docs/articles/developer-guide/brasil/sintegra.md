# Sintegra: CPF and CNPJ

`SintegraService` provides CPF and CNPJ lookups. Its `SearchCepAsync` method delegates to the registered `ICepService`, usually ViaCEP.

## Installation and configuration

Install `JJMasterData.Brasil` and configure your key:

```bash
dotnet user-secrets set "JJMasterData:Sintegra:ApiKey" "<your-key>"
```

The configuration section is `JJMasterData:Sintegra`. `Url` is optional and defaults to `www.sintegraws.com.br/api/v1/execute-api.php`, without a protocol prefix. If no HubDev key is configured, `WithBrasilActionPlugins(builder.Configuration)` selects Sintegra ahead of cpfcnpj.com.br:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithBrasilActionPlugins(builder.Configuration);
```

To select Sintegra explicitly:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithSintegraCpfActionPlugin()
    .WithSintegraCnpjActionPlugin()
    .WithCepActionPlugin<ViaCepService>();
```

Add `using JJMasterData.Brasil.Configuration;`, `using JJMasterData.Brasil.Services;`, and `using JJMasterData.Web.Configuration;`. `WithCepActionPlugin<ViaCepService>()` also registers the `HttpClient` used by the services.

## Usage

Add the `Cpf` and `Cnpj` plugins to their respective fields in the dictionary editor. For CPF, set `BirthDate` to a `DateTime` field; the service sends `data-nascimento` in `dd/MM/yyyy` format. For example, map `NomeDaPf` to a person's name or `Nome` to a company name. See the [common result keys and options](index.md#using-field-actions-in-a-dictionary). This provider does not expose `IgnoreDb`.

For direct calls, `WithSintegra()` registers `IReceitaFederalService`. Also register `ICepService` and `HttpClient` when using the service without field actions. Although the shared CPF/CNPJ action shows `TimeoutSeconds`, `SintegraService` does not apply it to the HTTP request.
