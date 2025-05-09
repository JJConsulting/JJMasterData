#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Commons.Data.Entity.Providers;

/// <summary>
/// Provides functionality to work with entity metadata and manage the
/// database schema associated with an entity. The interface defines
/// methods for generating scripts for table creation, updates, and
/// stored procedures, as well as commands for data manipulation.
/// </summary>
public interface IEntityProvider
{
    string VariablePrefix { get; }
    string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null);
    string? GetWriteProcedureScript(Element element);
    string? GetReadProcedureScript(Element element);
    string? GetAlterTableScript(Element element, IEnumerable<ElementField> addedFields);
    Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null);
    Task<Element> GetElementFromTableAsync(string schemaName, string tableName, Guid? connectionId);
    DataAccessCommand GetInsertCommand(Element element, Dictionary<string, object?> values);
    DataAccessCommand GetUpdateCommand(Element element, Dictionary<string, object?> values);
    DataAccessCommand GetDeleteCommand(Element element, Dictionary<string, object> primaryKeys);
    DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter);
    DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string, object?> values);
    bool TableExists(string tableName, Guid? connectionId = null);
    Task<bool> TableExistsAsync(string schema, string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default);
    Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default);
    Task<bool> ColumnExistsAsync(string tableName, string columnName, Guid? connectionId = null, CancellationToken cancellationToken = default);
    Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null);
    Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null);
    Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null);
}
