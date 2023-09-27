using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Brasil.Actions;

public class CepActionPlugin : IActionPlugin
{
    private ICepService CepService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CepActionPlugin));
    public string Title => "CEP";

    public IEnumerable<string> AdditionalParametersHints
    {
        get
        {
            yield return nameof(CepResult.Logradouro);
            yield return nameof(CepResult.Complemento);
            yield return nameof(CepResult.Bairro);
            yield return nameof(CepResult.Localidade);
            yield return nameof(CepResult.Uf);
            yield return nameof(CepResult.Unidade);
        }
    }
    public HtmlBuilder AdditionalInformationHtml { get; } = new();

    public CepActionPlugin(ICepService cepService)
    {
        CepService = cepService;
    }
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginActionContext context)
    {
        var cep = context.Values[context.TriggeredFieldName!].ToString();
        var cepResult = await CepService.SearchCepAsync(cep);
        var cepDictionary = cepResult.ToDictionary();
        foreach (var parameter in context.AdditionalParameters)
        {
            context.Values[parameter.Key] = cepDictionary[parameter.Value!.ToString()!];
        }

        return new PluginActionResult();
    }
}