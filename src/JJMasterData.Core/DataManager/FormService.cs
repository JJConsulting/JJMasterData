using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Logging;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;
using CommandType = JJMasterData.Commons.Dao.Entity.CommandType;

namespace JJMasterData.Core.DataManager;

public class FormService
{
    #region Properties

    private Hashtable _userValues;
    private IDataAccess _dataAccess;
    private Factory _factory;
    private FormManager _formManager;


    /// <inheritdoc cref="JJBaseView.UserValues"/>
    public Hashtable UserValues
    {
        get => _userValues ??= new Hashtable();
        set =>_userValues = value;
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

    public FormElement FormElement { get; private set; }

    public AuditLogService AuditLog { get; set; }
    
    public bool EnableErrorLink { get; set; }

    public object Sender { get; set; }

    #endregion

    #region Events

    public EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;
    public EventHandler<FormAfterActionEventArgs> OnAfterDelete;
    public EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;
    public EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    public EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;
    public EventHandler<FormAfterActionEventArgs> OnAfterUpdate;

    #endregion

    #region Constructor


    public FormService(FormElement formElement, AuditLogService auditLogService = null,
        bool enableFormEvents = true)
    {
        FormElement = formElement;
        AuditLog = auditLogService;

        if (enableFormEvents)
            AddFormEvent(FormEventManager.GetFormEvent(FormElement.Name));
    }

    #endregion

    #region Methods

    
    /// <summary>
    /// Get a specific record in the database.
    /// </summary>
    public DataDictionaryResult<Hashtable> GetHashtable(Hashtable filters)
    {
        var errors = new Hashtable();
        var hashtable = RunDatabaseCommand(() => FormRepository.GetFields(FormElement, filters), ref errors);

        return new()
        {
            Errors = errors,
            Result = hashtable,
            Total = 1
        };
    }
    
    /// <summary>
    /// Get records in the database.
    /// </summary>
    public DataDictionaryResult<DataTable> GetDataTable(
        Hashtable filters,
        string orderBy = null,
        int regPerPage = int.MaxValue,
        int pag = 1 )
    {
        var errors = new Hashtable();

        int total = 0;
        
        var dataTable = 
            RunDatabaseCommand(() => FormRepository.GetDataTable(
                FormElement,
                filters,
                orderBy,
                regPerPage,
                pag,
                ref total
                
            ), ref errors);

        return new()
        {
            Errors = errors,
            Result = dataTable,
            Total = total
        };
    }

    /// <summary>
    /// Update records in the database using the provided values.
    /// </summary>
    /// <param name="sender">Object that called this method. Used for events.</param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="validationFunc">Function to validate the fields.</param>
    public FormLetter Update(object sender, Hashtable values, Func<Hashtable> validationFunc = null)
    {
        var errors = new Hashtable();


        errors = validationFunc?.Invoke();

        var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);

        OnBeforeUpdate?.Invoke(sender, beforeActionArgs);

        if (errors?.Count > 0) return new(errors);

        RunDatabaseCommand(() => FormRepository.Update(FormElement, values), ref errors);

        if (sender is JJFormView jjFormView)
            SaveFiles(jjFormView, values);

        AuditLog?.AddLog(FormElement, values, CommandType.Update);

        var afterEventArgs = new FormAfterActionEventArgs(values);
        OnAfterUpdate?.Invoke(sender, afterEventArgs);

        var result = new FormLetter
        {
            Errors = errors,
            UrlRedirect = afterEventArgs.UrlRedirect
        };

        return result;
    }


    public FormLetter Update(Hashtable formValues)
    {
        var values = FormManager.MergeWithExpressionValues(formValues, PageState.Update, true);
        var errors = FormManager.ValidateFields(values, PageState.Update, EnableErrorLink);

        if (OnBeforeUpdate != null)
        {
            var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
            OnBeforeUpdate?.Invoke(Sender, beforeActionArgs);
        }

        if (errors.Count > 0) 
            return new FormLetter(errors);

        RunDatabaseCommand(() => FormRepository.Update(FormElement, values), ref errors);

        if (errors.Count > 0)
            return new FormLetter(errors);

        if (Sender is JJFormView jjFormView)
            SaveFiles(jjFormView, values); //TODO:

        AuditLog?.AddLog(FormElement, values, CommandType.Update);

        FormLetter result;
        if (OnAfterUpdate != null)
        {
            var afterEventArgs = new FormAfterActionEventArgs(values);
            OnAfterUpdate?.Invoke(Sender, afterEventArgs);

            result = new FormLetter
            {
                Errors = errors,
                UrlRedirect = afterEventArgs.UrlRedirect
            };
        }
        else
        {
            result = new FormLetter();
        }

        return result;
    }


    /// <summary>
    /// Insert records in the database using the provided values.
    /// </summary>
    /// <param name="sender">Object that called this method. Used for events.</param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="validationFunc">Function to validate the fields.</param>
    public FormLetter Insert(object sender, Hashtable values, Func<Hashtable> validationFunc = null)
    {
        var errors = new Hashtable();

        errors = validationFunc?.Invoke();

        var beforeActionArgs = new FormBeforeActionEventArgs(values, errors);
        OnBeforeInsert?.Invoke(sender, beforeActionArgs);

        if (errors?.Count > 0) 
            return new FormLetter(errors);

        RunDatabaseCommand(() => FormRepository.Insert(FormElement, values), ref errors);

        if (sender is JJFormView jjFormView)
            SaveFiles(jjFormView, values);

        AuditLog?.AddLog(FormElement, values, CommandType.Insert);

        var afterEventArgs = new FormAfterActionEventArgs(values);
        OnAfterInsert?.Invoke(sender, afterEventArgs);

        var result = new FormLetter
        {
            Errors = errors,
            UrlRedirect = afterEventArgs.UrlRedirect
        };

        return result;
    }

    /// <summary>
    /// Delete records in the database using the primaryKeys filter.
    /// </summary>
    /// <param name="sender">Object that called this method. Used for events.</param>
    /// <param name="primaryKeys">Primary keys to delete records on the database.</param>>
    public FormLetter Delete(object sender, Hashtable primaryKeys)
    {
        var errors = new Hashtable();

        if (sender is JJFormView formView)
        {
            OnAfterDelete += formView.InvokeOnAfterDelete;
            OnBeforeDelete += formView.InvokeOnBeforeDelete;
        }

        var beforeActionArgs = new FormBeforeActionEventArgs(primaryKeys, errors);

        OnBeforeDelete?.Invoke(sender, beforeActionArgs);

        if (errors.Count > 0) return new(errors);

        int total = RunDatabaseCommand(() => FormRepository.Delete(FormElement, primaryKeys), ref errors);

        DeleteFiles(primaryKeys);

        var afterEventArgs = new FormAfterActionEventArgs(primaryKeys);

        OnAfterDelete?.Invoke(sender, afterEventArgs);

        AuditLog?.AddLog(FormElement, primaryKeys, CommandType.Delete);

        var result = new FormLetter
        {
            Errors = errors,
            Total = total,
            UrlRedirect = afterEventArgs.UrlRedirect
        };

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
            HandleException(ex, ref errors);
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
            HandleException(ex, ref errors);
        }

        return default;
    }

    private static void HandleException(SqlException ex, ref Hashtable errors)
    {
        if (ex.Number == 547 | ex.Number == 2627 | ex.Number == 2601 | ex.Number == 170 | ex.Number == 50000)
        {
            errors.Add("Database Exception", ExceptionManager.GetMessage(ex));
        }
        else
        {
            Log.AddError(ex.Message);
            throw ex;
        }
    }

    private void AddFormEvent(IFormEvent formEvent)
    {
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
            }
        }
    }

    private void SaveFiles(JJFormView formView, Hashtable values)
    {
        var uploadFields = FormElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        var fieldManager = new FieldManager(formView, FormElement);
        foreach (var field in uploadFields)
        {
            string value = string.Empty;
            if (values.ContainsKey(field.Name))
                value = values[field.Name].ToString();

            var upload = (JJTextFile)fieldManager.GetField(field, PageState.Insert, values, value);
            upload.SaveMemoryFiles();
        }
    }

    private void DeleteFiles(Hashtable primaryKeys)
    {
        var uploadFields = FormElement.Fields.ToList()
            .FindAll(x => x.Component == FormComponent.File);

        if (uploadFields.Count == 0)
            return;

        var fieldManager = new FieldManager(FormElement);
        foreach (var field in uploadFields)
        {
            string value = string.Empty;
            if (primaryKeys.ContainsKey(field.Name))
                value = primaryKeys[field.Name].ToString();

            var jjTextFile = (JJTextFile)fieldManager.GetField(field, PageState.Delete, primaryKeys, value);
            jjTextFile.DeleteAll();
        }
    }

    #endregion
}