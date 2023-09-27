using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Brasil.Actions;

public class CepPluginAction : IActionPlugin
{
    private ICepService CepService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CepPluginAction));
    public string Title => "CEP";
    public IEnumerable<string> AdditionalParametersHints => new List<string>();
    public HtmlBuilder AdditionalInformationHtml { get; } = new();

    public CepPluginAction(ICepService cepService)
    {
        CepService = cepService;
    }
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginActionContext context)
    {
        var cep = context.Values[context.TriggeredFieldName!].ToString();
        var cepResult = await CepService.SearchCepAsync(cep);
        
        foreach (var parameter in context.AdditionalParameters)
        {
            context.Values[parameter.Key] = cepResult.Logradouro;
        }

        return new PluginActionResult();
    }
}