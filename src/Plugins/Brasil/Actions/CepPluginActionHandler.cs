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

public class CepPluginActionHandler(ICepService cepService,
        ExpressionsService expressionService)
    : BrasilPluginActionHandler(expressionService)
{
    private ICepService CepService { get; } = cepService;
    public override Guid Id => GuidGenerator.FromValue(nameof(CepPluginActionHandler));
    public override string Title => "Cep";

    protected override IEnumerable<string> CustomFieldMapKeys
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

    protected override async Task<Dictionary<string, object?>> GetResultAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cep = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());
        
        var cepResult = await CepService.SearchCepAsync(cep);
        
        return cepResult.ToDictionary();
    }

}