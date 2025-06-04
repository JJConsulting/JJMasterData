#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Commons.Data.Entity.Providers;

[Obsolete("Please use IEntityProvider.")]
public class EntityProviderBase(IEntityProvider provider) : IEntityProvider
{
    public string VariablePrefix => provider.VariablePrefix;
    public string GetCreateTableScript(Element element, List<RelationshipReference>? relationships = null) =>
        provider.GetCreateTableScript(element, relationships);
    public string? GetWriteProcedureScript(Element element) =>
        provider.GetWriteProcedureScript(element);
    public string? GetReadProcedureScript(Element element) =>
        provider.GetReadProcedureScript(element);
    public string? GetAlterTableScript(Element element, IEnumerable<ElementField> addedFields) =>
        provider.GetAlterTableScript(element, addedFields);
    public Task<Element> GetElementFromTableAsync(string tableName, Guid? connectionId = null) =>
        provider.GetElementFromTableAsync(tableName, connectionId);
    public Task<Element> GetElementFromTableAsync(string schemaName, string connectionId, Guid? guid) =>
        provider.GetElementFromTableAsync(schemaName, connectionId, guid);
    public DataAccessCommand GetInsertCommand(Element element, Dictionary<string, object?> values) =>
        provider.GetInsertCommand(element, values);
    public DataAccessCommand GetUpdateCommand(Element element, Dictionary<string, object?> values) =>
        provider.GetUpdateCommand(element, values);
    public DataAccessCommand GetDeleteCommand(Element element, Dictionary<string, object> primaryKeys) =>
        provider.GetDeleteCommand(element, primaryKeys);
    public DataAccessCommand GetReadCommand(Element element, EntityParameters parameters, DataAccessParameter totalOfRecordsParameter) =>
        provider.GetReadCommand(element, parameters, totalOfRecordsParameter);
    public DataAccessCommand GetInsertOrReplaceCommand(Element element, Dictionary<string, object?> values) =>
        provider.GetInsertOrReplaceCommand(element, values);
    public bool TableExists(string tableName, Guid? connectionId = null) =>
        provider.TableExists(tableName, connectionId);
    public Task<bool> TableExistsAsync(string schema, string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default) =>
        provider.TableExistsAsync(schema, tableName, connectionId, cancellationToken);
    public Task<bool> TableExistsAsync(string tableName, Guid? connectionId = null, CancellationToken cancellationToken = default) =>
        provider.TableExistsAsync(tableName, connectionId, cancellationToken);
    public Task<bool> ColumnExistsAsync(string tableName, string columnName, Guid? connectionId = null, CancellationToken cancellationToken = default) =>
        provider.ColumnExistsAsync(tableName, columnName, connectionId, cancellationToken);
    public Task<string?> GetStoredProcedureDefinitionAsync(string procedureName, Guid? connectionId = null) =>
        provider.GetStoredProcedureDefinitionAsync(procedureName, connectionId);
    public Task DropStoredProcedureAsync(string procedureName, Guid? connectionId = null) =>
        provider.DropStoredProcedureAsync(procedureName, connectionId);
    public Task<List<string>> GetStoredProcedureListAsync(Guid? connectionId = null) =>
        provider.GetStoredProcedureListAsync(connectionId);
}
