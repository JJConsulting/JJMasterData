using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ScriptsService(IEntityRepository entityRepository, 
    IDataDictionaryRepository dataDictionaryRepository)
{
    public async Task<ScriptsResult> GetScriptsAsync(FormElement formElement)
    {
        var relationships = await GetFormElementRelationships(formElement).ToListAsync();
        
        var createTableScript = entityRepository.GetCreateTableScript(formElement,relationships);
        var readProcedureScript = formElement.UseReadProcedure ? entityRepository.GetReadProcedureScript(formElement) : null;
        var writeProcedureScript = formElement.UseWriteProcedure ? entityRepository.GetWriteProcedureScript(formElement) : null;
        var alterTableScript = await entityRepository.GetAlterTableScriptAsync(formElement);
        
        return new ScriptsResult
        {
            CreateTableScript = createTableScript,
            ReadProcedureScript = readProcedureScript,
            WriteProcedureScript = writeProcedureScript,
            AlterTableScript = alterTableScript
        };
    }

    private async IAsyncEnumerable<RelationshipReference> GetFormElementRelationships(FormElement formElement)
    {
        foreach (var r in formElement.Relationships.GetElementRelationships())
        {
            var tableName = (await dataDictionaryRepository.GetFormElementAsync(r.ChildElement)).TableName;

            yield return new(r.ChildElement, tableName);
        }
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
                await entityRepository.ExecuteBatchAsync(sql.ToString());
                break;
            case "ExecuteCreateDataModel":
                var relationships = await GetFormElementRelationships(formElement).ToListAsync();
                await entityRepository.CreateDataModelAsync(formElement,relationships);
                break;
            case "ExecuteAlterTable":
                var alterTableScript = await entityRepository.GetAlterTableScriptAsync(formElement);
                await entityRepository.ExecuteBatchAsync(alterTableScript);
                break;
        }
    }
}