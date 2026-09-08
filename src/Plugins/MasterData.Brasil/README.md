# JJMasterData.Brasil

🇧🇷 Plugin com utilidades de consulta de CNPJ, CPF e CEP na Receita Federal. 
- Integrações com Sintegra, HubDev, cpfcnpj e ViaCep

## Configuração

O provider de CPF/CNPJ é inferido pela presença de configuração em `JJMasterData`:

- `JJMasterData:HubDev:ApiKey`
- `JJMasterData:Sintegra:ApiKey`
- `JJMasterData:CpfCnpj:ApiKey`

Se `HubDev:ApiKey` estiver preenchido, o plugin usa HubDev. Se apenas `Sintegra:ApiKey` estiver preenchido, o plugin usa Sintegra. Se apenas `CpfCnpj:ApiKey` estiver preenchido, o plugin usa cpfcnpj.

### cpfcnpj

O provider [cpfcnpj.com.br](https://www.cpfcnpj.com.br/dev/) consome `GET https://api.cpfcnpj.com.br/{token}/{pacote}/{documento}`. O token vai na configuração `JJMasterData:CpfCnpj:ApiKey`. Os pacotes usados são configuráveis:

- `JJMasterData:CpfCnpj:CnpjPackage` (padrão `6`, com situação cadastral, porte e Simples Nacional)
- `JJMasterData:CpfCnpj:CpfPackage` (padrão `2`)

Registro manual, lado a lado com os demais:

```csharp
builder.WithCpfCnpjCnpjActionPlugin();
builder.WithCpfCnpjCpfActionPlugin();
```
