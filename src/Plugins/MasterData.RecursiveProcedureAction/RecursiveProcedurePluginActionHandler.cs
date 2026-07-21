using System.Data;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Components;
using JJMasterData.Protheus.Abstractions;
using JJMasterData.RecursiveProcedureAction.UI;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.RecursiveProcedureAction;

public class RecursiveProcedurePluginActionHandler(
    IComponentFactory componentFactory,
    IProtheusService protheusService,
    IHttpContextAccessor httpContextAccessor,
    IEntityRepository entityRepository,
    IMasterDataUser masterDataUser) : IPluginActionHandler
{
    public Guid Id => new("8b9a6606-6322-46a0-98bb-c7ef0b62c200");
    public string Title => "WebSales™ Recursive Stored Procedure";


    private const string ConfigPrimaryKey = "id";
    private const string ConfigProcedureName = "ProcedureName";
    private const string ConfigEnableObs = "enableObs";
    internal const string ConfigObsLabel = "obsLabel";
    internal const string ConfigTitle = "title";
    internal const string ConfigObsModalSize = "obsModalSize";
    public IEnumerable<PluginConfigurationField>? ConfigurationFields
    {
        get
        {
            yield return new PluginConfigurationField
            {
                Name = ConfigProcedureName,
                Required = true
            };
            yield return new PluginConfigurationField
            {
                Name = ConfigTitle,
                Label = "Título",
                Required = true,
                Type = PluginConfigurationFieldType.Text
            };
            yield return new PluginConfigurationField
            {
                Name = ConfigPrimaryKey,
                Label = "Chave Primária",
                Required = true,
                Type = PluginConfigurationFieldType.FormElementField
            };
            yield return new PluginConfigurationField
            {
                Name = ConfigEnableObs,
                Label = "Habilitar observação",
                Type = PluginConfigurationFieldType.Boolean
            };
            yield return new PluginConfigurationField
            {
                Name = ConfigObsLabel,
                Label = "Label da observação",
                Type = PluginConfigurationFieldType.Text
            };
            yield return new PluginConfigurationField
            {
                Name = ConfigObsModalSize,
                Label = "Tamanho do modal da observação (Fullscreen, ExtraLarge, Large, Default, Small) (Default: Large)",
                Type = PluginConfigurationFieldType.Text
            };
        }
    }

    public bool CanCreate(ActionSource actionSource) => actionSource is not ActionSource.Field;

    public async Task<PluginActionResult> ExecuteActionAsync(PluginActionContext context)
    {
        int executionSequence;
        
        if (!context.Values.TryGetValue("ExecutionSequence", out var exec))
            executionSequence = GetExecutionSequence();
        else
            executionSequence = (int)exec!;

        var showObsModal = IsShowObsModal(context, executionSequence);
        if (showObsModal)
        {
            var modal = new RecursiveProcedureObsModal(context, componentFactory);
            return modal.GetResult();
        }

        if (executionSequence < 0)
            executionSequence = 0;

        var inputParameters = GetInputParameters(context, executionSequence);

        var outputParameters = new RecursiveProcedureOutputParameters();

        var command = GetCommand(context, inputParameters, outputParameters);

        await entityRepository.SetCommandAsync(command, context.FormElement.ConnectionId);

        var messageBox = new RecursiveProcedureMessageBox(context);

        var isGoBackResult = outputParameters.EndExecutionParameter.Value is true &&
                             string.IsNullOrEmpty(outputParameters.MessageContentParameter.Value?.ToString());


        if (!string.IsNullOrEmpty(outputParameters.UrlPostParameter.Value?.ToString()))
        {
            executionSequence += 1;
            var protheusResult =
                await protheusService.CallFunctionAsync(outputParameters.UrlPostParameter.Value.ToString()!);
            inputParameters.ExecutionSequenceParameter.Value = executionSequence;
            outputParameters.MessageContentParameter.Value = protheusResult;
            command = GetCommand(context, inputParameters, outputParameters);
            await entityRepository.SetCommandAsync(command, context.FormElement.ConnectionId);
        }

        var messageType = outputParameters.MessageTypeParameter.Value;

        switch (messageType)
        {
            case 4 when EnableObs(context):
                executionSequence -= 1;
                break;
            case 4:
                executionSequence = 0;
                break;
            default:
                executionSequence += 1;
                break;
        }

        if (isGoBackResult)
            return messageBox.GoBackResult();

        if (string.IsNullOrEmpty(outputParameters.MessageContentParameter.Value?.ToString()))
        {
            context.Values["ExecutionSequence"] = executionSequence;
            return await ExecuteActionAsync(context);
        }


        return messageBox.GetResult(outputParameters, executionSequence);
    }

    private static DataAccessCommand GetCommand(PluginActionContext context,
        RecursiveProcedureInputParameters inputParameters, RecursiveProcedureOutputParameters outputParameters)
    {
        var command = new DataAccessCommand
        {
            Sql = context.ConfigurationMap[ConfigProcedureName]?.ToString() ?? "jj_setOrcamentoAcao",
            Type = CommandType.StoredProcedure
        };

        command.Parameters.AddRange(inputParameters.GetAllParameters());
        command.Parameters.AddRange(outputParameters.GetAllParameters());
        return command;
    }


    private RecursiveProcedureInputParameters GetInputParameters(
        PluginActionContext context,
        int executionSequence)
    {
        var observationFormValue = httpContextAccessor.HttpContext?.Request.GetFormValue("ExecutionSequenceObs");
        var inputParameters = new RecursiveProcedureInputParameters
        {
            UserIdParameter =
            {
                Value = masterDataUser.Id
            },
            PrimaryKeyParameter =
            {
                Value = context.Values[
                    context.ConfigurationMap[ConfigPrimaryKey]?.ToString() ??
                    throw new JJMasterDataException("You must set your primary key at your plugin configuration.")]
            },
            ExecutionSequenceParameter =
            {
                Value = executionSequence
            },
            ObsParameter =
            {
                Value = observationFormValue
            }
        };
        return inputParameters;
    }

    private static bool EnableObs(PluginActionContext context)
    {
        return context.ConfigurationMap.TryGetValue(ConfigEnableObs, out var obs) && obs is true;
    }

    private static bool IsShowObsModal(PluginActionContext context, int executionSequence)
    {
        return EnableObs(context) && executionSequence < 0;
    }

    private int GetExecutionSequence()
    {
        var executionSequenceString = httpContextAccessor.HttpContext?.Request.GetFormValue("ExecutionSequence");

#pragma warning disable CA1806
        int.TryParse(executionSequenceString, out var executionSequence);
#pragma warning restore CA1806

        return executionSequenceString is null ? -1 : executionSequence;
    }
}
