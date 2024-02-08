using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Brasil.Actions;

public abstract class BrasilPluginActionHandler(ExpressionsService expressionsService) : IPluginFieldActionHandler
{
    private ExpressionsService ExpressionsService { get; } = expressionsService;

    private const string AllowEditingOnErrorKey  = "AllowEditingOnError";
    private const string ShowErrorMessageKey = "ShowErrorMessage";
    private const string IsResultValidKey = "IsResultValid";
    
    public abstract Guid Id { get; }
    public abstract string Title { get; }

    protected virtual IEnumerable<PluginConfigurationField>? CustomConfigurationFields => null;
    protected abstract IEnumerable<string> CustomFieldMapKeys { get; }

    public IEnumerable<string> FieldMapKeys
    {
        get
        {
 
            foreach (var field in CustomFieldMapKeys)
            {
                yield return field;
            }
            

            yield return IsResultValidKey;
        }
    }
    
    public virtual IEnumerable<PluginConfigurationField> ConfigurationFields
    {
        get
        {
            if (CustomConfigurationFields is not null)
            {
                foreach (var field in CustomConfigurationFields)
                {
                    yield return field;
                }
            }
            yield return new PluginConfigurationField
            {
                Name = AllowEditingOnErrorKey,
                Label = $"Habilitar campos desabilitados para edição quando o {Title.ToUpper()} não for encontrado.",
                Type = PluginConfigurationFieldType.Boolean
            };
            yield return new PluginConfigurationField
            {
                Name = ShowErrorMessageKey,
                Label = $"Mostrar mensagem de erro quando o {Title.ToUpper()} não for encontrado.",
                Type = PluginConfigurationFieldType.Boolean
            };
        }
    }

    public abstract HtmlBuilder? AdditionalInformationHtml { get; }

    private static void ClearFields(PluginFieldActionContext context)
    {
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = null;
    }

    private PluginActionResult OnResultFound(PluginFieldActionContext context, Dictionary<string,object?> result)
    {
        foreach (var parameter in context.FieldMap)
        {
            if (context.ActionContext.FormElement.Fields.TryGetField(parameter.Value, out var field))
            {
                var isEnabled = ExpressionsService.GetBoolValue(field.EnableExpression, context.ActionContext.FormStateData);

                if (!isEnabled)
                {
                    field.SetEnabled(true);
                    field.SetReadOnly(true);
                }
            }
        }

        result[IsResultValidKey] = true;
        
        foreach (var parameter in context.FieldMap)
            context.Values[parameter.Value] = result[parameter.Key];
        
        return PluginActionResult.Success();
    }

    private PluginActionResult OnResultNotFound(PluginFieldActionContext context)
    {
        ClearFields(context);

        if (context.ConfigurationMap[AllowEditingOnErrorKey] is true)
        {
            foreach (var parameter in context.FieldMap.Where(f=> f.Key != IsResultValidKey))
            {
                if (context.ActionContext.FormElement.Fields.TryGetField(parameter.Value, out var field) )
                    field.EnableExpression = "val:1";
            }
        }
        
        if(context.FieldMap.TryGetValue(IsResultValidKey, out var fieldName))
            context.Values[fieldName] = false;
        
        if (context.ConfigurationMap[ShowErrorMessageKey] is true)
            return PluginActionResult.Error("Erro", $"{Title} não encontrado.");

        return PluginActionResult.Success();
    }

    protected abstract Task<Dictionary<string, object?>> GetResultAsync(PluginFieldActionContext context);
    
    public async Task<PluginActionResult> ExecuteActionAsync(PluginFieldActionContext context)
    {
        Dictionary<string, object?> result;

        try
        {
            result = await GetResultAsync(context);
        }
        catch
        {
            return OnResultNotFound(context);
        }
        
        return OnResultFound(context, result);
    }
}