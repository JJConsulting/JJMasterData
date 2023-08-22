using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager;

public interface IFormService
{
    bool EnableErrorLinks { get; set; }
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
    
    /// <summary>
    /// Update records applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    public Task<FormLetter> UpdateAsync(FormElement formElement, IDictionary<string, object?> values, DataContext dataContext);

    public Task<FormLetter> InsertAsync(FormElement formElement,IDictionary<string, object?> values, DataContext dataContext, bool validateFields = true);

    /// <summary>
    /// Insert or update if exists, applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    public Task<FormLetter<CommandOperation>> InsertOrReplaceAsync(FormElement formElement,IDictionary<string, object?> values,  DataContext dataContext);

    /// <summary>
    /// Delete records in the database using the primaryKeys filter.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="primaryKeys">Primary keys to delete records on the database.</param>
    /// <param name="dataContext"></param>
    /// >
    public Task<FormLetter> DeleteAsync(FormElement formElement,IDictionary<string, object> primaryKeys,  DataContext dataContext);

    void AddFormEventHandler(IFormEventHandler formEventHandler);
}