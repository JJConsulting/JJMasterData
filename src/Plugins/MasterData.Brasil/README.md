# JJMasterData.Brasil

🇧🇷 Plugin com utilidades de consulta de CNPJ, CPF e CEP na Receita Federal. 
- Integrações com Sintegra, HubDev e ViaCep

## Configuração

O provider de CPF/CNPJ é inferido pela presença de configuração em `JJMasterData`:

- `JJMasterData:HubDev:ApiKey`
- `JJMasterData:Sintegra:ApiKey`

Se `HubDev:ApiKey` estiver preenchido, o plugin usa HubDev. Se apenas `Sintegra:ApiKey` estiver preenchido, o plugin usa Sintegra.
