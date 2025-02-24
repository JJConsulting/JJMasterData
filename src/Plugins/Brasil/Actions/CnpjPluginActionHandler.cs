using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;


namespace JJMasterData.Brasil.Actions;

public class CnpjPluginActionHandler(IReceitaFederalService receitaFederalService, ExpressionsService expressionsService)
    : BrasilPluginActionHandler(expressionsService)
{
    private const string IgnoreDbFieldKey = "IgnoreDb";
    public override Guid Id => GuidGenerator.FromValue(nameof(CnpjPluginActionHandler));
    public override string Title => "Cnpj";
    
    protected override IEnumerable<string> CustomFieldMapKeys
    {
        get
        {
            yield return nameof(CnpjResult.Nome);
            yield return nameof(CnpjResult.Fantasia);
            yield return nameof(CnpjResult.Email);
            yield return nameof(CnpjResult.CapitalSocial);
            yield return nameof(CnpjResult.Cep);
            yield return nameof(CnpjResult.Uf);
            yield return nameof(CnpjResult.Municipio);
            yield return nameof(CnpjResult.Bairro);
            yield return nameof(CnpjResult.Logradouro);
            yield return nameof(CnpjResult.Numero);
            yield return nameof(CnpjResult.Complemento);
            yield return nameof(CnpjResult.Abertura);
            yield return nameof(CnpjResult.Telefone);
            yield return nameof(CnpjResult.QuadroSocios);
            yield return nameof(CnpjResult.Situacao);
            yield return "AtividadePrincipal.Codigo";
            yield return "AtividadePrincipal.Descricao";
        }
    }
    
    protected override IEnumerable<PluginConfigurationField> CustomConfigurationFields
    {
        get
        {
            yield return new PluginConfigurationField
            {
                Name = IgnoreDbFieldKey,
                Label = "Quando habilitado, a busca é realizada diretamente na Receita Federal. Nessa modalidade de consulta, serão consumidos 3 créditos ao invés de somente 1.",
                Type = PluginConfigurationFieldType.Boolean
            };
        }
    }

    protected override async Task<Dictionary<string, object?>> GetResultAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cnpj = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());
        
        receitaFederalService.IgnoreDb = context.ConfigurationMap[IgnoreDbFieldKey] is true;
        
        var cnpjResult = await receitaFederalService.SearchCnpjAsync(cnpj);

        return cnpjResult.ToDictionary();
    }
}