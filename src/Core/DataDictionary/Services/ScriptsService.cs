using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ScriptsService(IEntityRepository entityRepository, 
    IDataDictionaryRepository dataDictionaryRepository)
{
    
    
    public async Task<ScriptsResult> GetScriptsAsync(FormElement formElement)
    {
        var relationships = await GetFormElementRelationships(formElement);
        
        var createTableScript = entityRepository.GetCreateTableScript(formElement,relationships);
        var readProcedureScript =  entityRepository.GetReadProcedureScript(formElement);
        var writeProcedureScript = entityRepository.GetWriteProcedureScript(formElement);
        var alterTableScript = await entityRepository.GetAlterTableScriptAsync(formElement);
        
        return new ScriptsResult
        {
            CreateTableScript = createTableScript,
            ReadProcedureScript = readProcedureScript,
            WriteProcedureScript = writeProcedureScript,
            AlterTableScript = alterTableScript,
            UseWriteProcedure = formElement.UseWriteProcedure,
            UseReadProcedure = formElement.UseReadProcedure
        };
    }

    private async Task<List<RelationshipReference>> GetFormElementRelationships(FormElement formElement)
    {
        List<RelationshipReference> relationshipList = [];
        foreach (var r in formElement.Relationships.GetElementRelationships())
        {
            if (await dataDictionaryRepository.ExistsAsync(r.ChildElement))
            {
                var tableName = (await dataDictionaryRepository.GetFormElementAsync(r.ChildElement)).TableName;

                relationshipList.Add(new(r.ChildElement, tableName));
            }
            else
            {
                throw new JJMasterDataException("Relationship reference could not be found. Please check the relationship list and create or import any missing elements.");
            }
        }
        return relationshipList;
    }

    public async Task ExecuteScriptsAsync(string id, string scriptOption)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(id);
        switch (scriptOption)
        {
            case "ExecuteProcedures":
                var sql = new StringBuilder();
                sql.AppendLine(entityRepository.GetWriteProcedureScript(formElement));
                sql.AppendLine(entityRepository.GetReadProcedureScript(formElement));
                await entityRepository.ExecuteBatchAsync(sql.ToString(), formElement.ConnectionId);
                break;
            case "ExecuteCreateDataModel":
                var relationships = await GetFormElementRelationships(formElement);
                await entityRepository.CreateDataModelAsync(formElement,relationships);
                break;
            case "ExecuteAlterTable":
                var alterTableScript = await entityRepository.GetAlterTableScriptAsync(formElement);
                await entityRepository.ExecuteBatchAsync(alterTableScript, formElement.ConnectionId);
                break;
        }
    }
}