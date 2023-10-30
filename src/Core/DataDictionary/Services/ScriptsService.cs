using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ScriptsService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public ScriptsService(
        IEntityRepository entityRepository, 
        IDataDictionaryRepository dataDictionaryRepository)
    {
        _entityRepository = entityRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
    }
    public async Task<ScriptsResult> GetScriptsAsync(FormElement formElement)
    {
        var createTableScript = _entityRepository.GetCreateTableScript(formElement);
        var readProcedureScript = formElement.UseReadProcedure ? _entityRepository.GetReadProcedureScript(formElement) : null;
        var writeProcedureScript = formElement.UseWriteProcedure ? _entityRepository.GetWriteProcedureScript(formElement) : null;
        var alterTableScript = await _entityRepository.GetAlterTableScriptAsync(formElement);
        
        return new ScriptsResult
        {
            CreateTableScript = createTableScript,
            ReadProcedureScript = readProcedureScript,
            WriteProcedureScript = writeProcedureScript,
            AlterTableScript = alterTableScript
        };
    }
    
    public async Task ExecuteScriptsAsync(string id, string scriptOption)
    {
        var formElement = await _dataDictionaryRepository.GetFormElementAsync(id);

        switch (scriptOption)
        {
            case "ExecuteProcedures":
                var sql = new StringBuilder();
                sql.AppendLine(_entityRepository.GetWriteProcedureScript(formElement));
                sql.AppendLine(_entityRepository.GetReadProcedureScript(formElement));
                await _entityRepository.ExecuteBatchAsync(sql.ToString());
                break;
            case "ExecuteCreateDataModel":
                await _entityRepository.CreateDataModelAsync(formElement);
                break;
            case "ExecuteAlterTable":
                var alterTableScript = await _entityRepository.GetAlterTableScriptAsync(formElement);
                await _entityRepository.ExecuteBatchAsync(alterTableScript);
                break;
        }
    }
}