using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Brasil.Actions;

public class CpfPluginActionHandler : IPluginFieldActionHandler
{
    private IReceitaFederalService ReceitaFederalService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CpfPluginActionHandler));
    public string Title => "Cpf";

    public HtmlBuilder? AdditionalInformationHtml => null;
    public IEnumerable<PluginConfigurationField>? ConfigurationFields => null;
    public IEnumerable<string> FieldMapKeys
    {
        get
        {
            yield return nameof(CpfResult.NomeDaPf);
            yield return nameof(CpfResult.ComprovanteEmitido);
            yield return nameof(CpfResult.SituacaoCadastral);
        }
    }

    public CpfPluginActionHandler(IReceitaFederalService receitaFederalService)
    {
        ReceitaFederalService = receitaFederalService;
    }
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cpf = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());

        CpfResult cpfResult;

        try
        {
            cpfResult = await ReceitaFederalService.SearchCpfAsync(cpf);
        }
        catch (ReceitaFederalException)
        {
            ClearCpf(context);
            return PluginActionResult.Success();
        }
        
        var cpfDictionary = cpfResult.ToDictionary();
        
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = cpfDictionary[parameter.Key];
        
        return PluginActionResult.Success();
    }

    private static void ClearCpf(PluginFieldActionContext context)
    {
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = null;
    }
}