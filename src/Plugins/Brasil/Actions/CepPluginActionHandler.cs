using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Brasil.Actions;

public class CepPluginActionHandler : IPluginActionHandler
{
    private ICepService CepService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CepPluginActionHandler));
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

    public CepPluginActionHandler(ICepService cepService)
    {
        CepService = cepService;
    }
    
    public bool CanCreate(ActionSource actionSource) => actionSource is ActionSource.Field;
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginActionContext context)
    {
        var cep = context.Values[context.TriggeredFieldName!].ToString();

        CepResult cepResult;

        try
        {
            cepResult = await CepService.SearchCepAsync(cep);
        }
        catch (ViaCepException)
        {
            return new PluginActionResult
            {
                Modal = new PluginActionModal
                {
                    Title = "Erro",
                    Content = "CEP não encontrado",
                    Icon = MessageIcon.Error
                }
            };
        }
        
        var cepDictionary = cepResult.ToDictionary();
        foreach (var parameter in context.AdditionalParameters)
        {
            context.Values[parameter.Key] = cepDictionary[parameter.Value!.ToString()!];
        }

        return new PluginActionResult();
    }
}