# Brazil integrations

`JJMasterData.Brasil` adds field actions for CPF, CNPJ, and postal code (CEP) lookups. For CPF and CNPJ setup, see the [two provider guides](cpfcnpj.md): [HubDev](hubdev.md) and [Sintegra](sintegra.md). Automatic registration uses [ViaCEP](viacep.md) for CEP. The current package targets .NET 10.

## Installation

In a host project that already uses `JJMasterData.Web`, install the matching JJMasterData package version:

```bash
dotnet add package JJMasterData.Brasil
```

To use the project from this repository directly, add a project reference to `src/Plugins/MasterData.Brasil/MasterData.Brasil.csproj` in the host.

Configure an API key for one CPF/CNPJ provider under `JJMasterData`, then register the actions in `Program.cs`:

```csharp
using JJMasterData.Brasil.Configuration;
using JJMasterData.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithBrasilActionPlugins(builder.Configuration);
```

Keep the key outside source control. For local development, use `dotnet user-secrets set "JJMasterData:HubDev:ApiKey" "<your-key>"`. In production, use the host's configuration and secret storage.

When multiple keys are present, the overload that accepts `IConfiguration` selects providers in this order: **HubDev > Sintegra > cpfcnpj.com.br**. With no keys, it still registers HubDev, but lookups require a valid key. The overload without `IConfiguration` always registers HubDev for CPF/CNPJ and ViaCEP for CEP. To select a provider explicitly, use the methods in its article. Register only one CPF/CNPJ provider's field actions in a host.

## Using field actions in a dictionary

1. Open the dictionary editor and add a **field plugin action** to a CPF, CNPJ, or CEP field.
2. Select the registered `Cpf`, `Cnpj`, or `Cep` plugin. Enable `Trigger On Change` to run the lookup when the field changes; this also requires `AutoPostBack` on the field.
3. In **FieldMap**, map each desired result key to a form field name. The field holding the action supplies the document number or CEP to look up.
4. For CPF, set `BirthDate` to a form field containing a `DateTime` birth date.

The actions expose `ShowErrorMessage`, `AllowEditingOnError`, and `TimeoutSeconds` (5 seconds by default for CPF/CNPJ lookups). Map `IsResultValid` to a Boolean field to receive `true` after success or `false` after a failed lookup. On failure, mapped fields are cleared; `AllowEditingOnError` enables editing them. Use the plugin's result keys in the map, rather than the external API's JSON property names.

CNPJ result keys include `Nome`, `Fantasia`, `Email`, `CapitalSocial`, `Cep`, `Uf`, `Municipio`, `Bairro`, `Logradouro`, `Numero`, `Complemento`, `Abertura`, `Telefone`, `QuadroSocios`, `Situacao`, `AtividadePrincipal.Codigo`, and `AtividadePrincipal.Descricao`. CPF keys are `NomeDaPf`, `ComprovanteEmitido`, and `SituacaoCadastral`. CEP keys are `Logradouro`, `Complemento`, `Bairro`, `Localidade`, `Uf`, and `Unidade`.
