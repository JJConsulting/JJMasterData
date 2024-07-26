using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class FormService(
    IEntityRepository entityRepository,
    FormFileService formFileService,
    FieldValidationService fieldValidationService,
    AuditLogService auditLogService,
    IStringLocalizer<MasterDataResources> localizer,
    ILogger<FormService> logger)
{
    #region Properties
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private FormFileService FormFileService { get; } = formFileService;

    private FieldValidationService FieldValidationService { get; } = fieldValidationService;

    private AuditLogService AuditLogService { get; } = auditLogService;
    private IStringLocalizer<MasterDataResources> Localizer { get; } = localizer;
    private ILogger<FormService> Logger { get; } = logger;
    
    #endregion

    #region Events
    
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeDeleteAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterDeleteAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeInsertAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterInsertAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeUpdateAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterUpdateAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeImportAsync;

    #endregion

    #region Methods

    /// <summary>
    /// Update records applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    public async Task<FormLetter> UpdateAsync(FormElement formElement, Dictionary<string, object> values, DataContext dataContext)
    {
        var isForm = dataContext.Source is DataContextSource.Form;
        var errors =  FieldValidationService.ValidateFields(formElement, values, PageState.Update, isForm);
        var result = new FormLetter(errors);

        if (OnBeforeUpdateAsync != null)
        {           
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            await OnBeforeUpdateAsync(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        int rowsAffected = 0;

        try
        {
            rowsAffected = await EntityRepository.UpdateAsync(formElement, values);
        }
        catch (Exception e)
        {
            Logger.LogFormServiceError(e,  nameof(UpdateAsync));
            errors.Add("DbException", Localizer[ExceptionManager.GetMessage(e)]);
        }

        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);

        if (formElement.Options.EnableAuditLog)
            await AuditLogService.LogAsync(formElement, dataContext, values, CommandOperation.Update);
        
        if (OnAfterUpdateAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterUpdateAsync.Invoke(this, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }
        
        return result;
    }

    public async Task<FormLetter> InsertAsync(FormElement formElement, Dictionary<string, object> values, DataContext dataContext, bool validateFields = true)
    {
        var isForm = dataContext.Source is DataContextSource.Form;
        Dictionary<string, string> errors;
        if (validateFields)
            errors = FieldValidationService.ValidateFields(formElement, values, PageState.Insert, isForm);
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
            await EntityRepository.InsertAsync(formElement, values);
        }
        catch (Exception e)
        {
            Logger.LogFormServiceError(e,  nameof(InsertAsync));
            errors.Add("DbException", Localizer[ExceptionManager.GetMessage(e)]);
        }

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);

        if (formElement.Options.EnableAuditLog)
            await AuditLogService.LogAsync(formElement, dataContext, values, CommandOperation.Insert);
        
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
    public async Task<FormLetter<CommandOperation>> InsertOrReplaceAsync(FormElement formElement, Dictionary<string, object> values, DataContext dataContext)
    {
        var isForm = dataContext.Source is DataContextSource.Form;
        var errors = FieldValidationService.ValidateFields(formElement, values, PageState.Import, isForm);
        var result = new FormLetter<CommandOperation>(errors);

        if (OnBeforeImportAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            
            await OnBeforeImportAsync.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;
        
        try
        {
            result.Result = await EntityRepository.SetValuesAsync(formElement, values);
        }
        catch (Exception e)
        {
            Logger.LogFormServiceError(e,  nameof(InsertOrReplaceAsync));
            errors.Add("DbException", Localizer[ExceptionManager.GetMessage(e)]);
        }

        if (errors.Count > 0)
            return result;

        if (formElement.Options.EnableAuditLog)
            await AuditLogService.LogAsync(formElement, dataContext, values, result.Result);

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);
        
        if (result.Result == CommandOperation.Insert && OnAfterInsertAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterInsertAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }
        
        
        if (result.Result == CommandOperation.Update && OnAfterUpdateAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterUpdateAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }
        
        if (result.Result == CommandOperation.Delete && OnAfterDeleteAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    /// <summary>
    /// Delete records in the database using the primaryKeys filter.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="primaryKeys">Primary keys to delete records on the database.</param>
    /// <param name="dataContext"></param>
    /// >
    public async Task<FormLetter> DeleteAsync(FormElement formElement, Dictionary<string, object> primaryKeys, DataContext dataContext)
    {
        var errors = new Dictionary<string, string>();
        var result = new FormLetter(errors);

        if (OnBeforeDeleteAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(primaryKeys, errors);
            await OnBeforeDeleteAsync(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        try
        {
            int rowsAffected = await EntityRepository.DeleteAsync(formElement, primaryKeys);
            result.NumberOfRowsAffected = rowsAffected;
        }
        catch (Exception e)
        {
            Logger.LogFormServiceError(e,  nameof(DeleteAsync));
            errors.Add("DbException", Localizer[ExceptionManager.GetMessage(e)]);
        }

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.DeleteFiles(formElement, primaryKeys);

        if (formElement.Options.EnableAuditLog)
            await AuditLogService.LogAsync(formElement, dataContext, primaryKeys, CommandOperation.Delete);

        if (OnAfterDeleteAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(primaryKeys);
            await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    #endregion
}