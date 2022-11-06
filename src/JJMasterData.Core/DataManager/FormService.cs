using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using CommandType = JJMasterData.Commons.Dao.Entity.CommandType;

namespace JJMasterData.Core.DataManager;

public class FormService
{
    #region Properties

    private Hashtable _userValues;
    private IDataAccess _dataAccess;
    private Factory _factory;
    private FormManager _formManager;
    private AuditLogService _auditLog;

    /// <inheritdoc cref="JJBaseView.UserValues"/>
    public Hashtable UserValues
    {
        get => _userValues ??= new Hashtable();
        set => _userValues = value;
    }

    /// <inheritdoc cref="Commons.Dao.DataAccess"/>
    public IDataAccess DataAccess
    {
        get => _dataAccess ??= JJService.DataAccess;
        set => _dataAccess = value;
    }

    /// <inheritdoc cref="Factory"/>
    public Factory FormRepository
    {
        get => _factory ??= new Factory(DataAccess);
        set => _factory = value;
    }

    /// <inheritdoc cref="Factory"/>
    public FormManager FormManager
    {
        get => _formManager ??= new FormManager(FormElement, UserValues, DataAccess);
        set => _formManager = value;
    }

    public AuditLogService AuditLog
    {
        get => _auditLog ??= new AuditLogService(DataContext);
        internal set => _auditLog = value;
    }

    public DataContext DataContext { get; internal set; }
    public FormElement FormElement { get; private set; }
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


    public FormService(FormElement formElement, DataContext dataContext)
    {
        FormElement = formElement;
        DataContext = dataContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update records in the database using the provided values.
    /// </summary>
    /// <param name="formValues">Values to be inserted.</param>
    public FormLetter Update(Hashtable formValues)
    {
        var values = FormManager.MergeWithExpressionValues(formValues, PageState.Update, true);
        var errors = FormManager.ValidateFields(values, PageState.Update, EnableErrorLink);
        var result = new FormLetter(errors);

        if (OnBeforeUpdate != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeUpdate.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        int rowsAffected = RunDatabaseCommand(() => FormRepository.Update(FormElement, values), ref errors);
        result.Values = values;
        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (DataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(FormElement, values);

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, CommandType.Update);

        if (OnAfterUpdate != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }
        
        return result;
    }


    /// <summary>
    /// Insert records in the database using the provided values.
    /// </summary>
    /// <param name="formValues">Values to be inserted.</param>
    public FormLetter Insert(Hashtable formValues)
    {
        var values = FormManager.MergeWithExpressionValues(formValues, PageState.Insert, true);
        var errors = FormManager.ValidateFields(values, PageState.Insert, EnableErrorLink);
        var result = new FormLetter(errors);

        if (OnBeforeInsert != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeInsert.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        RunDatabaseCommand(() => FormRepository.Insert(FormElement, values), ref errors);
        result.Values = values;

        if (errors.Count > 0)
            return result;

        if (DataContext.Source == DataContextSource.Form)
            FormFileService.SaveFormMemoryFiles(FormElement, values);

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, CommandType.Insert);

        if (OnAfterInsert != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        return result;
    }

    public DataDictionaryResult<CommandType> InsertOrReplace(Hashtable formValues)
    {
        var values = FormManager.MergeWithExpressionValues(formValues, PageState.Import, true);
        var errors = FormManager.ValidateFields(values, PageState.Import, EnableErrorLink);
        var result = new DataDictionaryResult<CommandType>();
        result.Errors = errors;

        if (OnBeforeInsert != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeInsert.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        result.Result = RunDatabaseCommand(() => FormRepository.SetValues(FormElement, values), ref errors);
        result.Values = values;

        if (errors.Count > 0)
            return result;

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, result.Result);

        if (OnAfterInsert != null && result.Result == CommandType.Insert)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterInsert.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if (OnAfterUpdate != null && result.Result == CommandType.Update)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate.Invoke(DataContext, afterEventArgs);
            result.UrlRedirect = afterEventArgs.UrlRedirect;
        }

        if (OnAfterDelete != null && result.Result == CommandType.Delete)
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
        var values = FormManager.MergeWithExpressionValues(primaryKeys, PageState.Delete, true);
        var result = new FormLetter(errors);

        if (OnBeforeDelete != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeDelete?.Invoke(DataContext, beforeActionArgs);
        }

        if (errors.Count > 0)
            return result;

        int rowsAffected = RunDatabaseCommand(() => FormRepository.Delete(FormElement, values), ref errors);
        result.Values = values;
        result.NumberOfRowsAffected = rowsAffected;

        if (errors.Count > 0)
            return result;

        if (DataContext.Source == DataContextSource.Form)
            FormFileService.DeleteFiles(FormElement, values);

        if (EnableHistoryLog)
            AuditLog.AddLog(FormElement, values, CommandType.Delete);

        if (OnAfterDelete != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
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