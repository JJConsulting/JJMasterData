using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using System;
using System.Collections;
using System.Data.SqlClient;

namespace JJMasterData.Core.DataManager;

public class FormService
{
    #region Properties

    private AuditLogService _auditLog;

    private IEntityRepository EntityRepository => FormManager.EntityRepository;

    private FormElement FormElement => FormManager.FormElement;

    public FormManager FormManager { get; private set; }

    public DataContext DataContext { get; private set; }

    public AuditLogService AuditLog
    {
        get => _auditLog ??= new AuditLogService(DataContext, EntityRepository);
        internal set => _auditLog = value;
    }

    public bool EnableErrorLink { get; set; }

    public bool EnableHistoryLog { get; set; }

    #endregion

    #region Events

    public EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;
    public EventHandler<FormAfterActionEventArgs> OnAfterDelete;
    public EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;
    public EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    public EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;
    public EventHandler<FormAfterActionEventArgs> OnAfterUpdate;
    public EventHandler<FormBeforeActionEventArgs> OnBeforeImport;

    #endregion

    #region Constructor

    public FormService(FormManager formManager, DataContext dataContext)
    {
        FormManager = formManager;
        DataContext = dataContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update records applying expressions and default values.
    /// </summary>
    /// <param name="values">Values to be inserted.</param>
    public FormLetter Update(Hashtable values)
    {
        var errors = FormManager.ValidateFields(values, PageState.Update, EnableErrorLink);
        var result = new FormLetter(errors);

        if (OnBeforeUpdate != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeUpdate.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        int rowsAffected = RunDatabaseCommand(() => EntityRepository.Update(FormElement, values), ref errors);
        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (DataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(FormElement, values);

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, CommandOperation.Update);

        if (OnAfterUpdate != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }
        
        return result;
    }

    public FormLetter Insert(Hashtable values)
    {
        var errors = FormManager.ValidateFields(values, PageState.Insert, EnableErrorLink);
        var result = new FormLetter(errors);

        if (OnBeforeInsert != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeInsert.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        RunDatabaseCommand(() => EntityRepository.Insert(FormElement, values), ref errors);

        if (errors.Count > 0)
            return result;

        if (DataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(FormElement, values);

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, CommandOperation.Insert);

        if (OnAfterInsert != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    /// <summary>
    /// Insert or update if exists, applying expressions and default values.
    /// </summary>
    /// <param name="formValues">Values to be inserted.</param>
    public FormLetter<CommandOperation> InsertOrReplace(Hashtable values)
    {
        var errors = FormManager.ValidateFields(values, PageState.Import, EnableErrorLink);
        var result = new FormLetter<CommandOperation>(errors);

        if (OnBeforeImport != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeImport.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        result.Result = RunDatabaseCommand(() => EntityRepository.SetValues(FormElement, values), ref errors);

        if (errors.Count > 0)
            return result;

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, result.Result);

        if (OnAfterInsert != null && result.Result == CommandOperation.Insert)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if (OnAfterUpdate != null && result.Result == CommandOperation.Update)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if (OnAfterDelete != null && result.Result == CommandOperation.Delete)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterDelete.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    /// <summary>
    /// Delete records in the database using the primaryKeys filter.
    /// </summary>
    /// <param name="primaryKeys">Primary keys to delete records on the database.</param>>
    public FormLetter Delete(Hashtable primaryKeys)
    {
        var errors = new Hashtable();
        var result = new FormLetter(errors);

        if (OnBeforeDelete != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(primaryKeys, errors);
            OnBeforeDelete?.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        int rowsAffected = RunDatabaseCommand(() => EntityRepository.Delete(FormElement, primaryKeys), ref errors);
        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (DataContext.Source == DataContextSource.Form)
            FormFileService.DeleteFiles(FormElement, primaryKeys);

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, primaryKeys, CommandOperation.Delete);

        if (OnAfterDelete != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(primaryKeys);
            OnAfterDelete.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    private static void RunDatabaseCommand(Action action, ref Hashtable errors)
    {
        try
        {
            action.Invoke();
        }
        catch (SqlException ex)
        {
            errors.Add("Database Exception", ExceptionManager.GetMessage(ex));
        }
    }

    private static T RunDatabaseCommand<T>(Func<T> func, ref Hashtable errors)
    {
        try
        {
            return func.Invoke();
        }
        catch (SqlException ex)
        {
            errors.Add("Database Exception", ExceptionManager.GetMessage(ex));
        }

        return default;
    }

    public void AddFormEvent()
    {
        IFormEvent formEvent = FormEventManager.GetFormEvent(FormElement.Name);
        foreach (var method in FormEventManager.GetFormEventMethods(formEvent))
        {
            switch (method)
            {
                case "OnBeforeInsert":
                    OnBeforeInsert += formEvent.OnBeforeInsert;
                    break;
                case "OnBeforeUpdate":
                    OnBeforeUpdate += formEvent.OnBeforeUpdate;
                    break;
                case "OnBeforeDelete":
                    OnBeforeDelete += formEvent.OnBeforeDelete;
                    break;
                case "OnAfterInsert":
                    OnAfterInsert += formEvent.OnAfterInsert;
                    break;
                case "OnAfterUpdate":
                    OnAfterUpdate += formEvent.OnAfterUpdate;
                    break;
                case "OnAfterDelete":
                    OnAfterDelete += formEvent.OnAfterDelete;
                    break;
                case "OnBeforeImport":
                    OnBeforeImport += formEvent.OnBeforeImport;
                    break;
            }
        }
    }

    #endregion
}