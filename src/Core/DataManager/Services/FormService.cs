using System;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager;

public class FormService : IFormService
{
    #region Properties
    private IEntityRepository EntityRepository { get; }
    private FormFileService FormFileService { get; }

    private IFieldValidationService FieldValidationService { get; }

    private IAuditLogService AuditLogService { get; }

    public bool EnableErrorLinks { get; set; }

    public bool EnableAuditLog { get; set; }
    internal bool Loaded { get; set; }
    #endregion

    #region Events

    public event EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;
    public event EventHandler<FormAfterActionEventArgs> OnAfterDelete;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;
    public event EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;
    public event EventHandler<FormAfterActionEventArgs> OnAfterUpdate;
    public event EventHandler<FormBeforeActionEventArgs> OnBeforeImport;

    #endregion

    #region Constructor

    public FormService(
        IEntityRepository entityRepository,
        FormFileService formFileService,
        IFieldValidationService fieldValidationService,
        IAuditLogService auditLogService)
    {
        FieldValidationService = fieldValidationService;
        EntityRepository = entityRepository;
        FormFileService = formFileService;
        AuditLogService = auditLogService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update records applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    public async Task<FormLetter> UpdateAsync(FormElement formElement, IDictionary<string,dynamic> values, DataContext dataContext)
    {
        var errors = await FieldValidationService.ValidateFieldsAsync(formElement,values, PageState.Update, EnableErrorLinks);
        var result = new FormLetter(errors);

        if (OnBeforeUpdate != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeUpdate.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        int rowsAffected = 0;

        try
        {
            rowsAffected = await EntityRepository.UpdateAsync(formElement, (IDictionary)values);
        }
        catch (Exception e)
        {
            errors.Add("DbException",ExceptionManager.GetMessage(e));
        }
        
        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);

        if (EnableAuditLog)
            await AuditLogService.LogAsync(formElement,dataContext, values, CommandOperation.Update);

        if (OnAfterUpdate != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    public async Task<FormLetter> InsertAsync(FormElement formElement,IDictionary<string,dynamic> values, DataContext dataContext, bool validateFields = true)
    {
        IDictionary<string,dynamic>errors;
        if (validateFields)
            errors = await FieldValidationService.ValidateFieldsAsync(formElement,values, PageState.Insert, EnableErrorLinks);
        else
            errors = new Dictionary<string,dynamic>();

        var result = new FormLetter(errors);
        if (OnBeforeInsert != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeInsert.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        try
        {
            await EntityRepository.InsertAsync(formElement, values as IDictionary);
        }
        catch (Exception e)
        {
            errors.Add("DbException",ExceptionManager.GetMessage(e));
        }

        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(formElement, values);

        if (EnableAuditLog)
            await AuditLogService.LogAsync(formElement,dataContext, values, CommandOperation.Insert);

        if (OnAfterInsert != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert.Invoke(dataContext, afterEventArgs);
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
    public async Task<FormLetter<CommandOperation>> InsertOrReplaceAsync(FormElement formElement,IDictionary<string,dynamic> values,  DataContext dataContext)
    {
        var errors = await FieldValidationService.ValidateFieldsAsync(formElement,values, PageState.Import, EnableErrorLinks);
        var result = new FormLetter<CommandOperation>(errors);

        if (OnBeforeImport != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeImport.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;


        try
        {
            result.Result = await EntityRepository.SetValuesAsync(formElement, (IDictionary)values);
        }
        catch (Exception e)
        {
            errors.Add("DbException",ExceptionManager.GetMessage(e));
        }
        

        if (errors.Count > 0)
            return result;

        if (EnableAuditLog)
            await AuditLogService.LogAsync(formElement,dataContext, values, result.Result);

        if (OnAfterInsert != null && result.Result == CommandOperation.Insert)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if (OnAfterUpdate != null && result.Result == CommandOperation.Update)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if (OnAfterDelete != null && result.Result == CommandOperation.Delete)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterDelete.Invoke(dataContext, afterEventArgs);
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
    public async Task<FormLetter> DeleteAsync(FormElement formElement,IDictionary<string,dynamic> primaryKeys,  DataContext dataContext)
    {
        IDictionary<string,dynamic>errors = new Dictionary<string, dynamic>();
        var result = new FormLetter(errors);

        if (OnBeforeDelete != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(primaryKeys, errors);
            OnBeforeDelete?.Invoke(dataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        try
        {
            int rowsAffected = await EntityRepository.DeleteAsync(formElement, (IDictionary)primaryKeys);
            result.NumberOfRowsAffected = rowsAffected;
        }
        catch (Exception e)
        {
            errors.Add("DbException",ExceptionManager.GetMessage(e));
        }


        if (errors.Count > 0)
            return result;

        if (dataContext.Source == DataContextSource.Form)
            FormFileService.DeleteFiles(formElement, primaryKeys);

        if (EnableAuditLog)
            await AuditLogService.LogAsync(formElement,dataContext, primaryKeys, CommandOperation.Delete);

        if (OnAfterDelete != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(primaryKeys);
            OnAfterDelete.Invoke(dataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }
    
    public void AddFormEvent(IFormEvent formEvent)
    {
        if (formEvent != null)
        {
            OnBeforeInsert += formEvent.OnBeforeInsert;
            OnBeforeDelete += formEvent.OnBeforeDelete;
            OnBeforeUpdate += formEvent.OnBeforeUpdate;
            OnBeforeImport += formEvent.OnBeforeImport;

            OnAfterDelete += formEvent.OnAfterDelete;
            OnAfterInsert += formEvent.OnAfterInsert;
            OnAfterUpdate += formEvent.OnAfterUpdate;
        }
    }

    #endregion
}