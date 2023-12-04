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

public class CpfPluginActionHandler(IReceitaFederalService receitaFederalService, ExpressionsService expressionsService)
    : BrasilPluginActionHandler(expressionsService)
{
    private IReceitaFederalService ReceitaFederalService { get; } = receitaFederalService;

    private const string BirthDateFieldKey = "BirthDate";
    private const string IgnoreDbFieldKey = "IgnoreDb";
    public override Guid Id => GuidGenerator.FromValue(nameof(CpfPluginActionHandler));
    public override string Title => "Cpf";
    public override HtmlBuilder? AdditionalInformationHtml => null;
    protected override IEnumerable<string> CustomFieldMapKeys
    {
        get
        {
            yield return nameof(CpfResult.NomeDaPf);
            yield return nameof(CpfResult.ComprovanteEmitido);
            yield return nameof(CpfResult.SituacaoCadastral);
        }
    }

    protected override IEnumerable<PluginConfigurationField> CustomConfigurationFields
    {
        get
        {
            yield return new PluginConfigurationField
            {
                Name = BirthDateFieldKey,
                Label = "Data de Nascimento",
                Required = true,
                Type = PluginConfigurationFieldType.FormElementField
            };
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
        
        var cpf = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());

        var birthDateFieldName = context.ConfigurationMap[BirthDateFieldKey]?.ToString();
        
        if (birthDateFieldName is null)
            throw new ArgumentNullException(birthDateFieldName, "You must set a BirthDate field");
        
        var birthDate = values[birthDateFieldName];

        if (birthDate is not DateTime birthDateTime)
            throw new ArgumentNullException(nameof(birthDate));

        ReceitaFederalService.IgnoreDb = context.ConfigurationMap[IgnoreDbFieldKey] is true;
        
        var cpfResult = await ReceitaFederalService.SearchCpfAsync(cpf, birthDateTime);

        return cpfResult.ToDictionary();
    }
}