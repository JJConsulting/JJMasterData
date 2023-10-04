using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Exceptions;
using JJMasterData.Brasil.Models;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Brasil.Actions;

public class CepPluginActionHandler : IPluginFieldActionHandler
{
    private ICepService CepService { get; }
    private ExpressionsService ExpressionService { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(CepPluginActionHandler));
    public string Title => "Cep";

    public const string AllowEditingOnErrorFieldName = "AllowEditingOnError";
    public const string ShowErrorMessageFieldName = "ShowErrorMessage";
    
    public IEnumerable<PluginConfigurationField>? ConfigurationFields
    {
        get
        {
            yield return new PluginConfigurationField
            {
                Name = AllowEditingOnErrorFieldName,
                Label = "Habilitar campos desabilitados para edição quando o CEP não for encontrado.",
                Type = PluginConfigurationFieldType.Boolean
            };
            yield return new PluginConfigurationField
            {
                Name = ShowErrorMessageFieldName,
                Label = "Mostrar mensagem de erro quando o CEP não for encontrado.",
                Type = PluginConfigurationFieldType.Boolean
            };
        }
    }

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
    
    public CepPluginActionHandler(
        ICepService cepService, 
        ExpressionsService expressionService,
        HtmlComponentFactory htmlComponentFactory)
    {
        CepService = cepService;
        ExpressionService = expressionService;
        MessageBoxFactory = htmlComponentFactory.MessageBox;
    }
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginFieldActionContext context)
    {
        var values = context.Values;
        
        var cep = StringManager.ClearCpfCnpjChars(values[context.FieldName!]!.ToString());

        CepResult cepResult;

        try
        {
            cepResult = await CepService.SearchCepAsync(cep);
        }
        catch (ViaCepException)
        {
            return OnCepNotFound(context);
        }
        
        return await OnCepFound(context, cepResult);
    }

    private async Task<PluginActionResult> OnCepFound(PluginFieldActionContext context, CepResult cepResult)
    {
        var cepDictionary = cepResult.ToDictionary();

        foreach (var parameter in context.FieldMap)
        {
            if (context.ActionContext.FormElement.Fields.TryGetField(parameter.Value, out var field))
            {
                var isEnabled =
                    await ExpressionService.GetBoolValueAsync(field.EnableExpression, context.ActionContext.FormStateData);

                if (!isEnabled)
                {
                    field.SetEnabled(true);
                    field.SetReadOnly(true);
                }
            }
        }

        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = cepDictionary[parameter.Key];

        return PluginActionResult.Success();
    }

    private static PluginActionResult OnCepNotFound(PluginFieldActionContext context)
    {
        ClearCepFields(context);

        if (context.ConfigurationMap[AllowEditingOnErrorFieldName] is true)
        {
            foreach (var parameter in context.FieldMap)
            {
                if (context.ActionContext.FormElement.Fields.TryGetField(parameter.Value, out var field))
                    field.EnableExpression = "val:1";
            }
        }

        if (context.ConfigurationMap[ShowErrorMessageFieldName] is true)
            return PluginActionResult.Error("Erro", "CEP não encontrado.");

        return PluginActionResult.Success();
    }

    private static void ClearCepFields(PluginFieldActionContext context)
    {
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = null;
    }
}