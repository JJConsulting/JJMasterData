using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Brasil.Actions;

public class CnpjPluginActionHandler : IPluginFieldActionHandler
{
    private IReceitaFederalService ReceitaFederalService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CnpjPluginActionHandler));
    public string Title => "Cnpj";
    
    public HtmlBuilder? AdditionalInformationHtml { get; }
    public IEnumerable<PluginConfigurationField>? ConfigurationFields => null;
    public IEnumerable<string> FieldMapKeys
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

    public CnpjPluginActionHandler(IReceitaFederalService receitaFederalService)
    {
        ReceitaFederalService = receitaFederalService;
    }
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cnpj = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());

        CnpjResult cnpjResult;

        try
        {
            cnpjResult = await ReceitaFederalService.SearchCnpjAsync(cnpj);
        }
        catch (ReceitaFederalException)
        {
            ClearCnpj(context);
            return PluginActionResult.Success();
        }
        
        var cnpjDictionary = cnpjResult.ToDictionary();
        
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = cnpjDictionary[parameter.Key];
        
        return PluginActionResult.Success();
    }

    private static void ClearCnpj(PluginFieldActionContext context)
    {
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = null;
    }
}