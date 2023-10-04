using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.UI.Html;


namespace JJMasterData.Brasil.Actions;

public class CnpjPluginActionHandler : BrasilPluginActionHandler
{
    
    private IReceitaFederalService ReceitaFederalService { get; }
    public override Guid Id => GuidGenerator.FromValue(nameof(CnpjPluginActionHandler));
    public override string Title => "Cnpj";
    public override HtmlBuilder? AdditionalInformationHtml => null;
    
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
        }
    }

    public CnpjPluginActionHandler(IReceitaFederalService receitaFederalService, ExpressionsService expressionsService) : base(expressionsService)
    {
        ReceitaFederalService = receitaFederalService;
    }

    protected override async Task<Dictionary<string, object?>> GetResultAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cnpj = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());
        
        var cnpjResult = await ReceitaFederalService.SearchCnpjAsync(cnpj);

        return cnpjResult.ToDictionary();
    }
}