
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class FormService(
    IEntityRepository entityRepository,
    FieldValidationService fieldValidationService,
    AuditLogService auditLogService,
    IStringLocalizer<MasterDataResources> localizer,
    ILogger<FormService> logger)
{
    #region Events

    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeDeleteAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs>? OnAfterDeleteAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeInsertAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs>? OnAfterInsertAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeUpdateAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs>? OnAfterUpdateAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeImportAsync;

    #endregion

    #region Methods

    private Element ApplyWriteOwner(FormElement formElement, Dictionary<string, object?> values, DataContext context)
    {
        var ownerName = formElement.ApiOptions.ApplyUserIdOn;
        if (context.Source != DataContextSource.Api || string.IsNullOrEmpty(ownerName))
            return formElement;

        if (string.IsNullOrWhiteSpace(context.UserId) || formElement.UseWriteProcedure ||
            entityRepository is not EntityRepository || !formElement.Fields.Contains(ownerName))
            throw new UnauthorizedAccessException("Owner-scoped writes require the built-in generated SQL Server path.");

        var owner = formElement.Fields[ownerName];
        var keys = formElement.GetPrimaryKeys();
        if (owner.AutoNum || owner.DataBehavior != FieldBehavior.Real ||
            keys.Count == 0 || keys.Any(key => key.DataBehavior != FieldBehavior.Real))
            throw new UnauthorizedAccessException("Invalid owner-scoped dictionary configuration.");

        var actor = context.UserId;
        if (values.TryGetValue(owner.Name, out var requestedOwner) && requestedOwner is not (null or DBNull) &&
            !string.Equals(requestedOwner.ToString(), actor, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("The ownership field cannot be changed.");

        object expectedOwner = owner.DataType switch
        {
            FieldType.NVarchar when owner.Size < 1 || actor.Length <= owner.Size => actor,
            FieldType.Varchar when (owner.Size < 1 || actor.Length <= owner.Size) && actor.All(c => c <= 127) => actor,
            FieldType.Int when int.TryParse(actor, CultureInfo.InvariantCulture, out var id) &&
                               id.ToString(CultureInfo.InvariantCulture) == actor => id,
            FieldType.UniqueIdentifier when Guid.TryParseExact(actor, "D", out var id) => id,
            _ => throw new UnauthorizedAccessException("The user ID does not match the configured ownership field.")
        };

        values[owner.Name] = actor;
        return formElement.WithWriteOwner(owner, expectedOwner);
    }

    public async Task<FormLetter<Dictionary<string, object>>> GetAsync(FormElement formElement,
        Dictionary<string, object?> filters)
    {
        var errors = new Dictionary<string, string>();
        var formLetter = new FormLetter<Dictionary<string, object>>(errors);

        try
        {
            formLetter.Result = (await entityRepository.GetFieldsAsync(formElement, filters!))!;
        }
        catch (Exception e)
        {
            logger.LogFormServiceError(e, nameof(GetAsync));
            errors.Add("DbException", localizer.GetExceptionMessage(e));
        }

        return formLetter;
    }

    /// <summary>
    /// Update records applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    public async Task<FormLetter> UpdateAsync(
        FormElement formElement, 
        Dictionary<string, object?> values,
        DataContext dataContext)
    {
        ApplyWriteOwner(formElement, values, dataContext);
        ApplyTextCaseTransform(formElement, values);

        var errors = await fieldValidationService.ValidateFieldsAsync(
            formElement,
            values,
            PageState.Update);

        var result = new FormLetter(errors);

        if (OnBeforeUpdateAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            await OnBeforeUpdateAsync(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        var rowsAffected = 0;

        try
        {
            rowsAffected = await entityRepository.UpdateAsync(ApplyWriteOwner(formElement, values, dataContext), values);
        }
        catch (Exception e)
        {
            logger.LogFormServiceError(e, nameof(UpdateAsync));
            errors.Add("DbException", localizer.GetExceptionMessage(e));
        }

        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (formElement.Options.EnableAuditLog)
            await auditLogService.LogAsync(formElement, dataContext, values, CommandOperation.Update);

        if (OnAfterUpdateAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterUpdateAsync.Invoke(this, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    public async Task<FormLetter> InsertAsync(FormElement formElement, Dictionary<string, object?> values,
        DataContext dataContext, bool validateFields = true)
    {
        ApplyWriteOwner(formElement, values, dataContext);
        ApplyTextCaseTransform(formElement, values);

        Dictionary<string, string> errors;
        if (validateFields)
            errors = await fieldValidationService.ValidateFieldsAsync(
                formElement,
                values,
                PageState.Insert);
        else
            errors = new Dictionary<string, string>();

        var result = new FormLetter(errors);

        if (OnBeforeInsertAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            await OnBeforeInsertAsync.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        try
        {
            await entityRepository.InsertAsync(ApplyWriteOwner(formElement, values, dataContext), values);
        }
        catch (Exception e)
        {
            logger.LogFormServiceError(e, nameof(InsertAsync));
            errors.Add("DbException", localizer.GetExceptionMessage(e));
        }

        if (errors.Count > 0)
            return result;

        if (formElement.Options.EnableAuditLog)
            await auditLogService.LogAsync(formElement, dataContext, values, CommandOperation.Insert);

        if (OnAfterInsertAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterInsertAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    /// <summary>
    /// Insert or update if exists, applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    public async Task<FormLetter<CommandOperation>> InsertOrReplaceAsync(FormElement formElement, Dictionary<string, object?> values, DataContext dataContext, bool validateFields = true)
    {
        ApplyWriteOwner(formElement, values, dataContext);
        ApplyTextCaseTransform(formElement, values);
        var isForm = dataContext.Source is DataContextSource.Form;
        
        Dictionary<string, string> errors;
        if (validateFields)
            errors = await fieldValidationService.ValidateFieldsAsync(formElement, values, PageState.Import);
        else
            errors = new();
        
        var letter = new FormLetter<CommandOperation>(errors);

        if (OnBeforeImportAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);

            await OnBeforeImportAsync.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return letter;

        try
        {
            letter.Result = await entityRepository.SetValuesAsync(ApplyWriteOwner(formElement, values, dataContext), values);
        }
        catch (Exception e)
        {
            logger.LogFormServiceError(e, nameof(InsertOrReplaceAsync));
            errors.Add("DbException", localizer.GetExceptionMessage(e));
        }

        if (errors.Count > 0)
            return letter;

        if (formElement.Options.EnableAuditLog)
            await auditLogService.LogAsync(formElement, dataContext, values, letter.Result);

        switch (letter.Result)
        {
            case CommandOperation.Insert when OnAfterInsertAsync != null:
            {
                var afterEventArgs = new FormAfterActionEventArgs(values);
                await OnAfterInsertAsync.Invoke(dataContext, afterEventArgs);
                letter.UrlRedirect = afterEventArgs.UrlRedirect;
                break;
            }
            case CommandOperation.Update when OnAfterUpdateAsync != null:
            {
                var afterEventArgs = new FormAfterActionEventArgs(values);
                await OnAfterUpdateAsync.Invoke(dataContext, afterEventArgs);
                letter.UrlRedirect = afterEventArgs.UrlRedirect;
                break;
            }
            case CommandOperation.Delete when OnAfterDeleteAsync != null:
            {
                var afterEventArgs = new FormAfterActionEventArgs(values);
                await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
                letter.UrlRedirect = afterEventArgs.UrlRedirect;
                break;
            }
        }


        return letter;
    }

    /// <summary>
    /// Delete records in the database using the primaryKeys filter.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="primaryKeys">Primary keys to delete records on the database.</param>
    /// <param name="dataContext"></param>
    /// >
    public async Task<FormLetter> DeleteAsync(FormElement formElement, Dictionary<string, object> primaryKeys,
        DataContext dataContext)
    {
        ApplyWriteOwner(formElement, primaryKeys!, dataContext);
        var errors = await fieldValidationService.ValidateRulesAsync(
            formElement,
            primaryKeys!,
            PageState.Delete);
        
        var result = new FormLetter(errors);

        if (OnBeforeDeleteAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(primaryKeys!, errors);
            await OnBeforeDeleteAsync(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        try
        {
            int rowsAffected = await entityRepository.DeleteAsync(
                ApplyWriteOwner(formElement, primaryKeys!, dataContext), primaryKeys);
            result.NumberOfRowsAffected = rowsAffected;
        }
        catch (Exception e)
        {
            logger.LogFormServiceError(e, nameof(DeleteAsync));
            errors.Add("DbException", localizer.GetExceptionMessage(e));
        }

        if (errors.Count > 0)
            return result;

        if (formElement.Options.EnableAuditLog)
            await auditLogService.LogAsync(formElement, dataContext, primaryKeys!, CommandOperation.Delete);

        if (OnAfterDeleteAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(primaryKeys!);
            await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    #endregion

    private static void ApplyTextCaseTransform(FormElement formElement, Dictionary<string, object?> values)
    {
        foreach (var field in formElement.Fields)
        {
            if (field.DataType.IsString && field.TextCase is not TextCase.None
                                        && values.TryGetValue(field.Name, out var value) 
                                        && value is string textValue)
            {
                var currentCulture = CultureInfo.CurrentCulture;
                
                values[field.Name] = field.TextCase switch
                {
                    TextCase.Uppercase => currentCulture.TextInfo.ToUpper(textValue),
                    TextCase.Lowercase => currentCulture.TextInfo.ToLower(textValue),
                    TextCase.TitleCase => currentCulture.TextInfo.ToTitleCase(textValue.ToLower(currentCulture)),
                    _ => textValue
                };
            }

        }
    }
}
