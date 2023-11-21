using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class FormService(IEntityRepository entityRepository,
    ExpressionsService expressionsService,
    FormFileService formFileService,
    FieldValidationService fieldValidationService,
    AuditLogService auditLogService,
    ILogger<FormService> logger)
{
    #region Properties
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private FormFileService FormFileService { get; } = formFileService;

    private FieldValidationService FieldValidationService { get; } = fieldValidationService;

    private AuditLogService AuditLogService { get; } = auditLogService;
    private ILogger<FormService> Logger { get; } = logger;

    public bool EnableErrorLinks { get; set; }
    
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
    public async Task<FormLetter> UpdateAsync(FormElement formElement, IDictionary<string, object> values, DataContext dataContext)
    {
        var errors =  FieldValidationService.ValidateFields(formElement, values, PageState.Update, EnableErrorLinks);
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
            Logger.LogError(e, "Error at {Method}", nameof(UpdateAsync));
            errors.Add("DbException", ExceptionManager.GetMessage(e));
        }

        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);

        if (IsAuditLogEnabled(formElement, PageState.Update, values))
            await AuditLogService.LogAsync(formElement, dataContext, values, CommandOperation.Update);
        
        if (OnAfterUpdateAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            await OnAfterUpdateAsync.Invoke(this, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }
        
        return result;
    }

    public async Task<FormLetter> InsertAsync(FormElement formElement, IDictionary<string, object> values, DataContext dataContext, bool validateFields = true)
    {
        IDictionary<string, string> errors;
        if (validateFields)
            errors = FieldValidationService.ValidateFields(formElement, values, PageState.Insert, EnableErrorLinks);
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
            Logger.LogError(e, "Error at {Method}", nameof(InsertAsync));
            errors.Add("DbException", ExceptionManager.GetMessage(e));
        }

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);

        if (IsAuditLogEnabled(formElement, PageState.Insert, values))
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
    public async Task<FormLetter<CommandOperation>> InsertOrReplaceAsync(FormElement formElement, IDictionary<string, object> values, DataContext dataContext)
    {
        var errors = FieldValidationService.ValidateFields(formElement, values, PageState.Import, EnableErrorLinks);
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
            Logger.LogError(e, "Error at {Method}", nameof(InsertOrReplaceAsync));
            errors.Add("DbException", ExceptionManager.GetMessage(e));
        }

        if (errors.Count > 0)
            return result;

        if (IsAuditLogEnabled(formElement, PageState.Import, values))
            await AuditLogService.LogAsync(formElement, dataContext, values, result.Result);

        
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
    public async Task<FormLetter> DeleteAsync(FormElement formElement, IDictionary<string, object> primaryKeys, DataContext dataContext)
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
            Logger.LogError(e, "Error at {Method}", nameof(DeleteAsync));
            errors.Add("DbException", ExceptionManager.GetMessage(e));
        }

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.DeleteFiles(formElement, primaryKeys);

        if (IsAuditLogEnabled(formElement, PageState.Delete, primaryKeys))
            await AuditLogService.LogAsync(formElement, dataContext, primaryKeys, CommandOperation.Delete);

        if (OnAfterDeleteAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(primaryKeys);
            await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    public void AddFormEventHandler(IFormEventHandler formEventHandler)
    {
        if (formEventHandler != null)
        {
            AddEventHandlers(formEventHandler);
        }
    }

    private void AddEventHandlers(IFormEventHandler eventHandler)
    {
        var type = eventHandler.GetType();
        
        
        OnBeforeInsertAsync += eventHandler.OnBeforeInsertAsync;
        OnBeforeDeleteAsync += eventHandler.OnBeforeDeleteAsync;
        

        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeUpdateAsync)))
        {
            OnBeforeUpdateAsync += eventHandler.OnBeforeUpdateAsync;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeImportAsync)))
        {
            OnBeforeImportAsync += eventHandler.OnBeforeImportAsync;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnAfterDeleteAsync)))
        {
            OnAfterDeleteAsync += eventHandler.OnAfterDeleteAsync;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnAfterInsertAsync)))
        {
            OnAfterInsertAsync += eventHandler.OnAfterInsertAsync;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnAfterUpdateAsync)))
        {
            OnAfterUpdateAsync += eventHandler.OnAfterUpdateAsync;
        }
    }

    private static bool IsMethodImplemented(Type type, string methodName)
    {
        var method = type.GetMethod(methodName);

        return method is not null;
    }

    private bool IsAuditLogEnabled(FormElement formElement, PageState pageState, IDictionary<string, object> formValues)
    {
        var formState = new FormStateData(formValues, pageState);
        var auditLogExpression = formElement.Options.GridToolbarActions.AuditLogGridToolbarAction.VisibleExpression;
        var isEnabled = ExpressionsService.GetBoolValue(auditLogExpression, formState);
        return isEnabled;
    }

    #endregion
}