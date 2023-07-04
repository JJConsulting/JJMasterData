using System;
using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager;

public interface IFormService
{
    bool EnableErrorLinks { get; set; }
    bool EnableAuditLog { get; set; }
    event EventHandler<FormBeforeActionEventArgs> OnBeforeDelete;
    event EventHandler<FormAfterActionEventArgs> OnAfterDelete;
    event EventHandler<FormBeforeActionEventArgs> OnBeforeInsert;
    event EventHandler<FormAfterActionEventArgs> OnAfterInsert;
    event EventHandler<FormBeforeActionEventArgs> OnBeforeUpdate;
    event EventHandler<FormAfterActionEventArgs> OnAfterUpdate;
    event EventHandler<FormBeforeActionEventArgs> OnBeforeImport;

    /// <summary>
    /// Update records applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    FormLetter Update(FormElement formElement, IDictionary<string,dynamic> values, DataContext dataContext);

    FormLetter Insert(FormElement formElement,IDictionary<string,dynamic> values, DataContext dataContext, bool validateFields = true);

    /// <summary>
    /// Insert or update if exists, applying expressions and default values.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="values">Values to be inserted.</param>
    /// <param name="dataContext"></param>
    FormLetter<CommandOperation> InsertOrReplace(FormElement formElement,IDictionary<string,dynamic> values,  DataContext dataContext);

    /// <summary>
    /// Delete records in the database using the primaryKeys filter.
    /// </summary>
    /// <param name="formElement"></param>
    /// <param name="primaryKeys">Primary keys to delete records on the database.</param>
    /// <param name="dataContext"></param>
    /// >
    FormLetter Delete(FormElement formElement,IDictionary<string,dynamic> primaryKeys,  DataContext dataContext);

    void AddFormEvent(IFormEvent formEvent);
}