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

public class FormService
{
    #region Properties
    private IEntityRepository EntityRepository { get; }
    private ExpressionsService ExpressionsService { get; }
    private FormFileService FormFileService { get; }

    private FieldValidationService FieldValidationService { get; }

    private AuditLogService AuditLogService { get; }
    private ILogger<FormService> Logger { get; }

    public bool EnableErrorLinks { get; set; }
    
    #endregion

    #region Events

    public event EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;
    public event EventHandler<FormAfterActionEventArgs> OnAfterDelete;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;
    public event EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;
    public event EventHandler<FormAfterActionEventArgs> OnAfterUpdate;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeImport;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeDeleteAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterDeleteAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeInsertAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterInsertAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeUpdateAsync;
    public event AsyncEventHandler<FormAfterActionEventArgs> OnAfterUpdateAsync;
    public event AsyncEventHandler<FormBeforeActionEventArgs> OnBeforeImportAsync;

    #endregion

    #region Constructor
    
    public FormService(
        IEntityRepository entityRepository,
        ExpressionsService expressionsService,
        FormFileService formFileService,
        FieldValidationService fieldValidationService,
        AuditLogService auditLogService,
        ILogger<FormService> logger
        )
    {
        FieldValidationService = fieldValidationService;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FormFileService = formFileService;
        AuditLogService = auditLogService;
        Logger = logger;
    }

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

        if (OnBeforeUpdate != null || OnBeforeUpdateAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeUpdate?.Invoke(dataContext, beforeActionArgs);

            if (OnBeforeUpdateAsync != null)
            {
                await OnBeforeUpdateAsync(dataContext, beforeActionArgs);
            }
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

        if (OnAfterUpdate != null || OnAfterUpdateAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate?.Invoke(dataContext, afterEventArgs);
            if (OnAfterUpdateAsync != null)
            {
                await OnAfterUpdateAsync.Invoke(this, afterEventArgs);
            }
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
        if (OnBeforeInsert != null || OnBeforeInsertAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeInsert?.Invoke(dataContext, beforeActionArgs);

            if (OnBeforeInsertAsync != null)
            {
                await OnBeforeInsertAsync.Invoke(dataContext, beforeActionArgs);
            }
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

        if (OnAfterInsert == null && OnAfterInsertAsync == null) 
            return result;
        
        var afterEventArgs = new FormAfterActionEventArgs(values);
        OnAfterInsert?.Invoke(dataContext, afterEventArgs);

        if (OnAfterInsertAsync != null)
        {
            await OnAfterInsertAsync.Invoke(dataContext, afterEventArgs);
        }
            
        result.UrlRedirect = afterEventArgs.UrlRedirect;

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

        if (OnBeforeImport != null || OnBeforeImportAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeImport?.Invoke(dataContext, beforeActionArgs);

            if (OnBeforeImportAsync != null)
            {
                await OnBeforeImportAsync.Invoke(dataContext, beforeActionArgs);
            }
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

        if ((OnAfterInsert != null || OnAfterInsertAsync != null) && result.Result == CommandOperation.Insert)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert?.Invoke(dataContext, afterEventArgs);

            if (OnAfterInsertAsync != null)
            {
                await OnAfterInsertAsync.Invoke(dataContext, afterEventArgs);
            }
            
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if ((OnAfterUpdate != null || OnAfterUpdateAsync != null) && result.Result == CommandOperation.Update)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate?.Invoke(dataContext, afterEventArgs);

            if (OnAfterUpdateAsync != null)
            {
                await OnAfterUpdateAsync.Invoke(dataContext, afterEventArgs);
            }
            
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if ((OnAfterDelete != null || OnAfterDeleteAsync != null) && result.Result == CommandOperation.Delete)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterDelete.Invoke(dataContext, afterEventArgs);
            
            if (OnAfterDeleteAsync != null)
            {
                await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
            }

            
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

        if (OnBeforeDelete != null || OnBeforeDeleteAsync != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(primaryKeys, errors);
            OnBeforeDelete?.Invoke(dataContext, beforeActionArgs);

            if (OnBeforeDeleteAsync != null)
            {
                await OnBeforeDeleteAsync(dataContext, beforeActionArgs);
            }
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

        if (OnAfterDelete != null || OnAfterDeleteAsync != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(primaryKeys);
            OnAfterDelete?.Invoke(dataContext, afterEventArgs);

            if (OnAfterDeleteAsync != null)
            {
                await OnAfterDeleteAsync.Invoke(dataContext, afterEventArgs);
            }
            
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
    
        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeInsert)))
        {
            OnBeforeInsert += eventHandler.OnBeforeInsert;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeDelete)))
        {
            OnBeforeDelete += eventHandler.OnBeforeDelete;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeUpdate)))
        {
            OnBeforeUpdate += eventHandler.OnBeforeUpdate;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeImport)))
        {
            OnBeforeImport += eventHandler.OnBeforeImport;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnAfterDelete)))
        {
            OnAfterDelete += eventHandler.OnAfterDelete;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnAfterInsert)))
        {
            OnAfterInsert += eventHandler.OnAfterInsert;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnAfterUpdate)))
        {
            OnAfterUpdate += eventHandler.OnAfterUpdate;
        }
        
        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeInsertAsync)))
        {
            OnBeforeInsertAsync += eventHandler.OnBeforeInsertAsync;
        }

        if (IsMethodImplemented(type, nameof(eventHandler.OnBeforeDeleteAsync)))
        {
            OnBeforeDeleteAsync += eventHandler.OnBeforeDeleteAsync;
        }

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

        return method!.DeclaringType != typeof(FormEventHandlerBase);
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