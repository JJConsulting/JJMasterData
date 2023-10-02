using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Brasil.Actions;

public class CepPluginActionHandler : IPluginFieldActionHandler
{
    private ICepService CepService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CepPluginActionHandler));
    public string Title => "CEP";
    public IEnumerable<string> FieldMapKeys
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

    private MessageBoxFactory MessageBoxFactory { get; }
    
    public CepPluginActionHandler(ICepService cepService, HtmlComponentFactory htmlComponentFactory)
    {
        CepService = cepService;
        MessageBoxFactory = htmlComponentFactory.MessageBox;
    }
    
    public bool CanCreate(ActionSource actionSource) => actionSource is ActionSource.Field;
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cep = StringManager.ClearCpfCnpjChars(values[context.FieldName!].ToString());

        CepResult cepResult;

        try
        {
            cepResult = await CepService.SearchCepAsync(cep);
        }
        catch (ViaCepException)
        {
            ClearCep(context);
            return PluginActionResult.Success();
        }
        
        var cepDictionary = cepResult.ToDictionary();
        
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = cepDictionary[parameter.Key];
        
        return PluginActionResult.Success();
    }

    private static void ClearCep(PluginFieldActionContext context)
    {
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = null;
    }
}